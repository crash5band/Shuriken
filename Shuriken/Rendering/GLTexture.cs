using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Shuriken.Rendering
{
    internal class GLTexture
    {
        private int id = 0;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public GLTexture()
        {

        }

        public GLTexture(IntPtr pixels, int width, int height)
        {
            GL.GenTextures(1, out id);

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenAlphaSgis);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, id);
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref id);
        }
    }
}
