using System;
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

        uint vao;
        uint vbo;
        uint ebo;
        uint[] indices;
        public Dictionary<string, ShaderProgram> shaderDictionary;

        Vertex[] buffer;
        Vector2[] uvCoords;
        Vector4[] vPos;
        List<Quad> quads;

        Dictionary<uint, string> shaderAttribs;

        public readonly int MaxVertices = 10000;
        public int MaxQuads => MaxVertices / 4;
        public int MaxIndices => MaxQuads * 6;

        public int NumVertices { get; private set; }
        public int NumQuads { get; private set; }
        public int NumIndices { get; private set; }
        public int BufferPos { get; private set; }
        public uint TexID { get; set; }
        public bool BatchStarted { get; private set; }
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

        public Renderer(int width, int height)
        {
            shaderDictionary = new Dictionary<string, ShaderProgram>();

            ShaderProgram basicShader = new ShaderProgram("basic", Path.Combine(shadersDir, "basic.vert"), Path.Combine(shadersDir, "basic.frag"));
            shaderDictionary.Add(basicShader.Name, basicShader);

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
            quads = new List<Quad>();
            Init();

            RenderWidth = width;
            RenderHeight = height;
        }
        private void Init()
        {
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

            uvCoords = new Vector2[4];
            uvCoords[0] = new Vector2(1.0f, 1.0f);
            uvCoords[1] = new Vector2(1.0f, 0.0f);
            uvCoords[2] = new Vector2(0.0f, 0.0f);
            uvCoords[3] = new Vector2(0.0f, 1.0f);

            // top-right, bottom-right, top-left, bottom-left
            vPos = new Vector4[4];
            vPos[0] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
            vPos[1] = new Vector4(0.5f, -0.5f, 0.0f, 1.0f);
            vPos[2] = new Vector4(-0.5f, -0.5f, 0.0f, 1.0f);
            vPos[3] = new Vector4(-0.5f, 0.5f, 0.0f, 1.0f);


        }

        private void ResetRenderStats()
        {
            NumQuads = 0;
            NumIndices = 0;
            NumVertices = 0;
        }

        public void BeginBatch()
        {
            BufferPos = 0;
            BatchStarted = true;

            ResetRenderStats();
        }

        public void EndBatch()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, BufferPos * 4 * 10, buffer);

            Flush();
            TexID = 0;
            BatchStarted = false;
        }

        public void Flush()
        {
            GL.DrawElements(PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);
        }

        public Vector2[] GetUVCoords(Vector2 sprStart, Vector2 sprSize, float texWidth, float texHeight, bool mirrorX, bool mirrorY)
        {
            // Order: top-right, bottom-right, bottom-left, top-left
            Vector2[] uvCoords = new Vector2[4];
            Vector2 start = new Vector2(sprStart.X, sprStart.Y);
            Vector2 end = start + new Vector2(sprSize.X, sprSize.Y);

            float right = end.X / texWidth;
            float left = start.X / texWidth;
            float top = 1 - (start.Y / texHeight);
            float bottom = 1 - (end.Y / texHeight);

            uvCoords[0] = new Vector2(mirrorX ? left : right, mirrorY ? bottom : top);
            uvCoords[1] = new Vector2(mirrorX ? left : right, mirrorY ? top : bottom);
            uvCoords[2] = new Vector2(mirrorX ? right : left, mirrorY ? top : bottom);
            uvCoords[3] = new Vector2(mirrorX ? right : left, mirrorY ? bottom : top);

            return uvCoords;
        }

        public void PushQuadBuffer(Quad q)
        {

            int offset = 0;
            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.TopRight;
            buffer[BufferPos + offset].UV = q.UVCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.BottomRight;
            buffer[BufferPos + offset].UV = q.UVCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.BottomLeft;
            buffer[BufferPos + offset].UV = q.UVCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], q.M);
            buffer[BufferPos + offset].Color = q.Color * q.TopLeft;
            buffer[BufferPos + offset].UV = q.UVCoords[offset];

            BufferPos += 4;
            ++NumQuads;
            NumIndices += 6;
        }

        public void ConfigureShader(ShaderProgram shader)
        {
            shader.Use();
            shader.SetMatrix4("projection", OpenTK.Mathematics.Matrix4.CreateOrthographicOffCenter(0.0f, RenderWidth, -RenderHeight, 0.0f, -100.0f, 100.0f));
        }

        public Matrix4x4 CreateModelMatrix(Vector2 position, int drawIndex, Vector2 pivot, float rotation, Vector2 size)
        {
            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateScale(size.X, size.Y, 1.0f));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(pivot.X, pivot.Y, 0.0f));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateRotationZ(Utilities.ToRadians(rotation)));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f));

            return model;
        }

        public void DrawSprite(Vector2 pos, Vector2 pivot, float rot, Vector2 sz, Shuriken.Models.Sprite spr, uint flags, Vector4 col, Vector4 tl, Vector4 tr, Vector4 br, Vector4 bl, int index)
        {
            bool mirrorX = (flags & 1024) != 0;
            bool mirrorY = (flags & 2048) != 0;
            Matrix4x4 mat = CreateModelMatrix(new Vector2(pos.X, pos.Y), index, new Vector2(pivot.X, pivot.Y), rot, new Vector2(sz.X, sz.Y));
            Vector2[] uvCoords = GetUVCoords(new Vector2(spr.Start.X, spr.Start.Y), new Vector2(spr.Width, spr.Height), spr.Texture.Width, spr.Texture.Height, mirrorX, mirrorY);
            quads.Add(new Quad(mat, uvCoords, col, tl, tr, bl, br, spr, index));
            //PushQuadBuffer(mat, GetUVCoords(new Vector2(spr.Start.X, spr.Start.Y), new Vector2(spr.Dimensions.X, spr.Dimensions.Y), spr.Texture.Width, spr.Texture.Height, mirrorX, mirrorY), col, tr, br, bl, tl);
        }

        public void Start()
        {
            quads.Clear();

            GL.ActiveTexture(TextureUnit.Texture0);
            BeginBatch();
        }

        public void End()
        {
            quads.Sort();
            foreach (var quad in quads)
            {
                if (quad.Sprite.Texture.ID != TexID)
                {
                    EndBatch();
                    BeginBatch();
                    quad.Sprite.Texture.Use();
                    TexID = quad.Sprite.Texture.ID;
                }

                PushQuadBuffer(quad);
            }

            EndBatch();
        }
    }
}
