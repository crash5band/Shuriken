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

        private uint vao;
        private uint vbo;
        private uint ebo;
        private uint[] indices;
        public Dictionary<string, ShaderProgram> shaderDictionary;

        private Vertex[] buffer;
        private List<Quad> quads;

        private bool additive;
        private int textureId = -1;
        private ShaderProgram shader;

        public readonly int MaxVertices = 10000;
        public int MaxQuads => MaxVertices / 4;
        public int MaxIndices => MaxQuads * 6;

        public int NumVertices { get; private set; }
        public int NumQuads => quads.Count;
        public int NumIndices { get; private set; }
        public int BufferPos { get; private set; }
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

            // setup vertex indices
            indices = new uint[MaxIndices];
            uint offset = 0;
            for (uint index = 0; index < MaxIndices; index += 6)
            {
                indices[index + 0] = offset + 0;
                indices[index + 1] = offset + 1;
                indices[index + 2] = offset + 2;

                indices[index + 3] = offset + 1;
                indices[index + 4] = offset + 2;
                indices[index + 5] = offset + 3;
                
                offset += 4;
            }

            buffer = new Vertex[MaxVertices];
            quads = new List<Quad>(MaxQuads);
            Init();

            Width = width;
            Height = height;
        }
        
        private void Init()
        {
            // 2 floats for pos, 2 floats for UVs, 4 floats for color
            int stride = Unsafe.SizeOf<Vertex>();

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.GenBuffers(1, out ebo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxVertices, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, MaxIndices, indices, BufferUsageHint.StaticDraw);

            // position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);

            // uv
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            
            // color
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Resets the number of quads, vertices, and indices.
        /// </summary>
        private void ResetRenderStats()
        {
            NumIndices = 0;
            NumVertices = 0;
        }

        /// <summary>
        /// Starts a new rendering batch.
        /// </summary>
        public void BeginBatch()
        {
            BufferPos = 0;
            BatchStarted = true;

            ResetRenderStats();
        }

        /// <summary>
        /// Ends the current rendering batch and flushes the vertex buffer
        /// </summary>
        public void EndBatch()
        {
            if (BufferPos > 0)
            {
                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, BufferPos * Unsafe.SizeOf<Vertex>(), buffer);
                Flush();
            }

            BatchStarted = false;
        }

        private void Flush()
        {
            GL.DrawElements(PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);
        }
        
        /// <summary>
        /// Pushes the quad parameters onto the vertex buffer.
        /// </summary>
        /// <param name="q">The quad to push to the buffer.</param>
        public void PushQuad(Quad q)
        {
            buffer[BufferPos++] = q.TopLeft;
            buffer[BufferPos++] = q.BottomLeft;
            buffer[BufferPos++] = q.TopRight;
            buffer[BufferPos++] = q.BottomRight;
            NumIndices += 6;
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

                if (id != TextureId || Additive != quad.Additive || NumVertices >= MaxVertices)
                {
                    EndBatch();
                    BeginBatch();

                    quad.Texture?.GlTex?.Bind();

                    TextureId = id;
                    Additive = quad.Additive;
                }

                PushQuad(quad);
            }

            if (BatchStarted)
                EndBatch();
        }
    }
}
