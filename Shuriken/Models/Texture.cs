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

        private void CreateTexture(string filename)
        {
            ScratchImage img = TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE);
            if (!TexHelper.Instance.IsCompressed(img.GetImage(0).Format))
            {
                img = img.Compress(DXGI_FORMAT.BC3_UNORM, TEX_COMPRESS_FLAGS.DEFAULT, 0.5f);
            }

            CreateBitmap(img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM));

            img = img.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM).FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL);

            Width = img.GetImage(0).Width;
            Height = img.GetImage(0).Height;

            GlTex = new GLTexture(img.GetImage(0).Pixels, Width, Height);

            img.Dispose();
        }

        private void CreateBitmap(ScratchImage img)
        {
            var bmp = BitmapConverter.FromTextureImage(img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            img.Dispose();
            bmp.Dispose();
        }

        public Texture(string filename)
        {
            FullName = filename;
            Name = Path.GetFileNameWithoutExtension(filename);
            CreateTexture(filename);

            Sprites = new ObservableCollection<int>();
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
