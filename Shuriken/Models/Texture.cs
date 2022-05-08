using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Shuriken.Converters;
using Shuriken.Rendering;
using DirectXTexNet;
using System.ComponentModel;

namespace Shuriken.Models
{
    public class Texture
    {
        public string Name { get; }
        public string FullName { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BitmapSource ImageSource { get; private set; }
        internal GLTexture GlTex { get; private set; }
        public ObservableCollection<int> Sprites { get; set; }

        private void CreateTexture(ScratchImage img)
        {
            if (TexHelper.Instance.IsCompressed(img.GetMetadata().Format))
                img = img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);

            else if (img.GetMetadata().Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                img = img.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

            Width = img.GetImage(0).Width;
            Height = img.GetImage(0).Height;

            GlTex = new GLTexture(img.FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL).GetImage(0).Pixels, Width, Height);

            CreateBitmap(img);

            img.Dispose();
        }

        private unsafe void CreateTexture(byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((IntPtr)pBytes, bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTexture(string filename)
        {
            CreateTexture(TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage img)
        {
            var bmp = BitmapConverter.FromTextureImage(img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            img.Dispose();
            bmp.Dispose();
        }

        public Texture(string filename) : this()
        {
            FullName = filename;
            Name = Path.GetFileNameWithoutExtension(filename);
            CreateTexture(filename);
        }

        public Texture(string name, byte[] bytes) : this()
        {
            FullName = name;
            Name = name;
            CreateTexture(bytes);
        }            

        public Texture()
        {
            Name = FullName = "";
            Width = Height = 0;
            ImageSource = null;
            GlTex = null;

            Sprites = new ObservableCollection<int>();
        }
    }
}
