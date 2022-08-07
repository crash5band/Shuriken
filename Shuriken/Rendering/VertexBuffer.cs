using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Runtime.CompilerServices;

namespace Shuriken.Rendering
{
    public class VertexBuffer
    {
        private uint vao;
        private uint vbo;
        private uint ebo;
        private uint[] indices;

        private Vertex[] buffer;

        public readonly int MaxVertices = 10000;
        public int MaxQuads => MaxVertices / 4;
        public int MaxIndices => MaxQuads * 6;

        public int NumVertices { get; private set; }
        public int NumIndices { get; private set; }
        public int BufferPos { get; private set; }

        private void AllocateBuffer()
        {
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
        }

        public void Initialize()
        {
            AllocateBuffer();

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
        /// Resets the vertex buffer's pointer
        /// </summary>
        public void ResetBufferPos()
        {
            BufferPos = 0;
        }

        /// <summary>
        /// Uploads the vertex buffer's contents to the GPU
        /// </summary>
        public void Upload()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, BufferPos * Unsafe.SizeOf<Vertex>(), buffer);
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
            NumVertices += 4;
            NumIndices += 6;
        }

        /// <summary>
        /// Draws the vertex buffer's contents
        /// </summary>
        public void Flush()
        {
            GL.DrawElements(PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);
            NumVertices = 0;
            NumIndices = 0;
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
        }

        public VertexBuffer()
        {
            vao = vbo = ebo = 0;
            BufferPos = 0;

            Initialize();
        }
    }
}
