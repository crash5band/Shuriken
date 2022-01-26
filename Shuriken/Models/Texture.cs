using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Shuriken.Converters;
using OpenTK.Graphics.OpenGL;
using DirectXTexNet;

namespace Shuriken.Models
{
    public class Texture
    {
        public uint ID { get; private set; }
        public string Name { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public BitmapSource ImageSource { get; private set; }

        public ObservableCollection<Sprite> Sprites { get; set; }

        private void CreateTexture(string filename)
        {
            uint id = 0;
            GL.GenTextures(1, out id);

            ID = id;

            ScratchImage img = TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE);
            ScratchImage bimg = img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);
            img = img.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM).FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL);

            Width = img.GetImage(0).Width;
            Height = img.GetImage(0).Height;

            var bmp = BitmapConverter.FromTextureImage(bimg, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenAlphaSgis);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.GetImage(0).Pixels);
            
            img.Dispose();
            bimg.Dispose();
            bmp.Dispose();
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }

        public Texture(string filename)
        {
            Name = Path.GetFileNameWithoutExtension(filename);
            CreateTexture(filename);

            Sprites = new ObservableCollection<Sprite>();
        }

        public Texture()
        {
            Name = "";
            Width = 0;
            Height = 0;
            ImageSource = null;
            Sprites = new ObservableCollection<Sprite>();
        }
    }
}
