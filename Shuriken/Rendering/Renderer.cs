using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shuriken.Misc;
using Shuriken.Misc.Extensions;
using Shuriken.Models;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Shuriken.Rendering
{
    class Renderer
    {
        public readonly string shadersDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");

        public Dictionary<string, ShaderProgram> shaderDictionary;

        private VertexBuffer vertexBuffer;
        private List<Quad> quads;

        private bool additive;
        private bool linearFiltering = true;
        private int textureId = -1;
        private ShaderProgram shader;

        public int NumQuads => quads.Count;
        public bool BatchStarted { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool Additive
        {
            get => additive;
            set
            {
                additive = value;
                GL.BlendFunc(BlendingFactor.SrcAlpha, additive ? BlendingFactor.One : BlendingFactor.OneMinusSrcAlpha);
            }
        }

        public bool LinearFiltering
        {
            get => linearFiltering;
            set
            {
                linearFiltering = value;

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    linearFiltering ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    linearFiltering ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            }
        }


        public int TextureId
        {
            get => textureId;
            set
            {
                textureId = value;
                shader.SetBool("hasTexture", textureId != -1);
            }
        }

        public Renderer(int width, int height)
        {
            shaderDictionary = new Dictionary<string, ShaderProgram>();

            ShaderProgram basicShader = new ShaderProgram("basic", Path.Combine(shadersDir, "basic.vert"), Path.Combine(shadersDir, "basic.frag"));
            shaderDictionary.Add(basicShader.Name, basicShader);

            vertexBuffer = new VertexBuffer();
            quads = new List<Quad>(vertexBuffer.MaxQuads);

            Width = width;
            Height = height;
        }

        ~Renderer()
        {
            vertexBuffer.Dispose();
        }

        /// <summary>
        /// Starts a new rendering batch.
        /// </summary>
        public void BeginBatch()
        {
            vertexBuffer.ResetBufferPos();
            BatchStarted = true;
        }

        /// <summary>
        /// Ends the current rendering batch and flushes the vertex buffer
        /// </summary>
        public void EndBatch()
        {
            if (vertexBuffer.BufferPos > 0)
            {
                vertexBuffer.Upload();
                vertexBuffer.Flush();
            }

            BatchStarted = false;
        }
        
        public void DrawSprite(
            Vector2 topLeft, Vector2 bottomLeft, Vector2 topRight, Vector2 bottomRight, 
            Vector2 position, float rotation, Vector2 scale, float aspectRatio,
            Sprite sprite, Sprite nextSprite, float spriteFactor, Vector4 color, 
            Vector4 gradientTopLeft, Vector4 gradientBottomLeft, Vector4 gradientTopRight, Vector4 gradientBottomRight, 
            int zIndex, uint flags)
        {
            var quad = new Quad();
            var aspect = new Vector2(aspectRatio, 1.0f);
            
            quad.TopLeft.Position = position + (topLeft * scale * aspect).Rotate(rotation) / aspect;
            quad.BottomLeft.Position = position + (bottomLeft * scale * aspect).Rotate(rotation) / aspect;
            quad.TopRight.Position = position + (topRight * scale * aspect).Rotate(rotation) / aspect;
            quad.BottomRight.Position = position + (bottomRight * scale * aspect).Rotate(rotation) / aspect;

            if (sprite != null && nextSprite != null)
            {
                var begin = new Vector2(
                    sprite.Start.X / sprite.Texture.Width,
                    sprite.Start.Y / sprite.Texture.Height);

                var nextBegin = new Vector2(
                    nextSprite.Start.X / nextSprite.Texture.Width,
                    nextSprite.Start.Y / nextSprite.Texture.Height);

                var end = begin + new Vector2(
                    sprite.Dimensions.X / sprite.Texture.Width,
                    sprite.Dimensions.Y / sprite.Texture.Height);

                var nextEnd = nextBegin + new Vector2(
                    nextSprite.Dimensions.X / nextSprite.Texture.Width,
                    nextSprite.Dimensions.Y / nextSprite.Texture.Height);

                begin = (1.0f - spriteFactor) * begin + spriteFactor * nextBegin;
                end = (1.0f - spriteFactor) * end + spriteFactor * nextEnd;

                if ((flags & 0x400) != 0) (begin.X, end.X) = (end.X, begin.X); // Mirror X
                if ((flags & 0x800) != 0) (begin.Y, end.Y) = (end.Y, begin.Y); // Mirror Y

                quad.TopLeft.UV = begin;
                quad.TopRight.UV = new Vector2(end.X, begin.Y);
                quad.BottomLeft.UV = new Vector2(begin.X, end.Y);
                quad.BottomRight.UV = end;
                quad.Texture = sprite.Texture;
            }
            
            quad.TopLeft.Color = color * gradientTopLeft;
            quad.TopRight.Color = color * gradientTopRight;
            quad.BottomLeft.Color = color * gradientBottomLeft;
            quad.BottomRight.Color = color * gradientBottomRight;
            quad.ZIndex = zIndex;
            quad.Additive = (flags & 0x1) != 0;
            quad.LinearFiltering = (flags & 0x1000) != 0;

            quads.Add(quad);
        }

        public void SetShader(ShaderProgram param)
        {
            shader = param;
            shader.Use();
        }

        /// <summary>
        /// Clears the quad buffer and starts a new rendering batch.
        /// </summary>
        public void Start()
        {
            quads.Clear();

            GL.ActiveTexture(TextureUnit.Texture0);
            BeginBatch();
        }

        /// <summary>
        /// Draws the quads in the quad buffer.
        /// </summary>
        public void End()
        {
            quads.Sort((x, y) => x.ZIndex.CompareTo(y.ZIndex));
            
            foreach (var quad in quads)
            {
                int id = quad.Texture?.GlTex?.ID ?? -1;

                if (id != TextureId || Additive != quad.Additive || LinearFiltering != quad.LinearFiltering ||
                    vertexBuffer.NumVertices >= vertexBuffer.MaxVertices)
                {
                    EndBatch();
                    BeginBatch();

                    quad.Texture?.GlTex?.Bind();

                    TextureId = id;
                    Additive = quad.Additive;
                    LinearFiltering = quad.LinearFiltering;
                }

                vertexBuffer.PushQuad(quad);
            }

            if (BatchStarted)
                EndBatch();
        }
    }
}
