﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using OpenTK.Graphics.OpenGL;
using Shuriken.Misc;

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
        private Vector4[] vPos;
        private List<Quad> quads;

        private Camera camera;

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
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

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

                indices[index + 3] = offset + 2;
                indices[index + 4] = offset + 3;
                indices[index + 5] = offset + 0;

                offset += 4;
            }

            buffer = new Vertex[MaxVertices];
            quads = new List<Quad>(MaxQuads);
            Init();

            RenderWidth = width;
            RenderHeight = height;

            Models.Vector3 camPos = new Models.Vector3(RenderWidth / 2.0f, -RenderHeight / 2.0f, 990);
            Models.Vector3 camTgt = new Models.Vector3(RenderWidth / 2.0f, -RenderHeight / 2.0f, -1);
            camera = new Camera("Default", camPos, camTgt);
        }
        private void Init()
        {
            // 3 floats for pos, 4 floats for color, 2 floats for UVs
            const int stride = 10 * sizeof(float);

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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // color
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));

            // uv
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // top-right, bottom-right, top-left, bottom-left
            vPos = new Vector4[4];
            vPos[0] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
            vPos[1] = new Vector4(0.5f, -0.5f, 0.0f, 1.0f);
            vPos[2] = new Vector4(-0.5f, -0.5f, 0.0f, 1.0f);
            vPos[3] = new Vector4(-0.5f, 0.5f, 0.0f, 1.0f);
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
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, BufferPos * 4 * 10, buffer);
                Flush();
            }

            BatchStarted = false;
        }

        private void Flush()
        {
            GL.DrawElements(PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Gets the UV coords of the texture from the supplied sprite parameters.
        /// </summary>
        /// <param name="sprStart">The start coordinates of the sprite.</param>
        /// <param name="sprSize">The dimensions of the sprite.</param>
        /// <param name="texWidth">The width of the texture.</param>
        /// <param name="texHeight">The height of the texture.</param>
        /// <param name="flipX">Whether the X coordinates of the sprite are flipped.</param>
        /// <param name="flipY">Whether the Y coordinates of the sprite are flipped.</param>
        /// <returns></returns>
        public void GetUVCoords(Vector2 sprStart, Vector2 sprSize, float texWidth, float texHeight, bool flipX,
            bool flipY, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2, out Vector2 uv3)
        {
            // Order: top-right, bottom-right, bottom-left, top-left
            Vector2 start = new Vector2(sprStart.X, sprStart.Y);
            Vector2 end = start + new Vector2(sprSize.X, sprSize.Y);

            float right = end.X / texWidth;
            float left = start.X / texWidth;
            float top = 1 - (start.Y / texHeight);
            float bottom = 1 - (end.Y / texHeight);

            uv0 = new Vector2(flipX ? left : right, flipY ? bottom : top);
            uv1 = new Vector2(flipX ? left : right, flipY ? top : bottom);
            uv2 = new Vector2(flipX ? right : left, flipY ? top : bottom);
            uv3 = new Vector2(flipX ? right : left, flipY ? bottom : top);
        }

        /// <summary>
        /// Pushes the quad parameters onto the vertex buffer.
        /// </summary>
        /// <param name="q">The quad to push to the buffer.</param>
        public void PushQuadBuffer(Quad q)
        {
            int offset = 0;
            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.TopRight;
            buffer[BufferPos + offset].UV = q.UV0;
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.BottomRight;
            buffer[BufferPos + offset].UV = q.UV1;
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.BottomLeft;
            buffer[BufferPos + offset].UV = q.UV2;
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.TopLeft;
            buffer[BufferPos + offset].UV = q.UV3;

            BufferPos += 4;
            NumIndices += 6;
        }

        public void SetShader(ShaderProgram param)
        {
            shader = param;
            shader.Use();
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix((float)RenderWidth / RenderHeight, 40));
        }

        /// <summary>
        /// Creates a model matrix from the given parameters.
        /// </summary>
        /// <param name="position">The position of the object.</param>
        /// <param name="pivot">The pivot around which the object rotates.</param>
        /// <param name="rotation">The rotation of the object</param>
        /// <param name="size">The size of the object.</param>
        /// <returns>A model matrix.</returns>
        public Matrix4x4 CreateModelMatrix(Vector3 position, Vector2 pivot, float rotation, Vector3 size)
        {
            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateScale(size.X, size.Y, size.Z));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(pivot.X, pivot.Y, 0.0f));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateRotationZ(Utilities.ToRadians(rotation)));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(position.X, -position.Y, position.Z + 0.1f));

            return model;
        }

        /// <summary>
        /// Adds a quad given its sprite and world transform to the quad buffer.
        /// </summary>
        /// <param name="pos">The position of the quad.</param>
        /// <param name="pivot">The pivot around which the quad rotates.</param>
        /// <param name="rot">The rotation of the quad.</param>
        /// <param name="sz">The size of the quad.</param>
        /// <param name="spr">The sprite used to draw the quad.</param>
        /// <param name="flags"></param>
        /// <param name="col">The tint color/</param>
        /// <param name="tl">The top-left tint gradient</param>
        /// <param name="tr">The top-right tint gradient</param>
        /// <param name="br">The bottom-right tint gradient</param>
        /// <param name="bl">The bottom-left tint gradient</param>
        /// <param name="index">The draw index of the quad. A higher index indactes the quad is drawn on top of a quad with a lower index.</param>
        public void DrawSprite(
            Vector3 pos, Vector2 pivot, float rot, Vector3 sz, 
            Models.Sprite spr, Models.Sprite nextSpr, float sprFactor, uint flags, 
            Vector4 col, Vector4 tl, Vector4 bl, Vector4 tr, Vector4 br, int index)
        {
            bool additive = (flags & 1) != 0;
            bool mirrorX = (flags & 1024) != 0;
            bool mirrorY = (flags & 2048) != 0;
            Matrix4x4 mat = CreateModelMatrix(pos, pivot, rot, sz);

            var uv0 = new Vector2(); 
            var uv1 = new Vector2(); 
            var uv2 = new Vector2(); 
            var uv3 = new Vector2(); 

            if (spr != null && nextSpr != null)
            {
                GetUVCoords(
                    new Vector2(
                        (1.0f - sprFactor) * spr.Start.X + sprFactor * nextSpr.Start.X,
                        (1.0f - sprFactor) * spr.Start.Y + sprFactor * nextSpr.Start.Y),
                    
                    new Vector2(
                        (1.0f - sprFactor) * spr.Width + sprFactor * nextSpr.Width,
                        (1.0f - sprFactor) * spr.Height + sprFactor * nextSpr.Height),
                    
                    spr.Texture.Width,
                    spr.Texture.Height,
                    
                    mirrorX,
                    mirrorY,
                    out uv0,
                    out uv1,
                    out uv2,
                    out uv3
                );
            }

            quads.Add(new Quad(mat, uv0, uv1, uv2, uv3, col, tl, tr, bl, br, spr?.Texture, index, additive));
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
            quads.Sort();
            
            foreach (var quad in quads)
            {
                int id = quad.Texture?.GlTex?.ID ?? -1;

                if (id != TextureId || Additive != quad.Additive || NumVertices >= MaxVertices)
                {
                    EndBatch();
                    BeginBatch();

                    quad.Texture?.GlTex?.Bind();

                    Additive = quad.Additive;
                    TextureId = id;
                }

                PushQuadBuffer(quad);
            }

            if (BatchStarted)
                EndBatch();
        }
    }
}
