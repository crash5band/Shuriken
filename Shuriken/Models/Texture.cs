using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AmicitiaLibrary.Graphics.DDS;
using Shuriken.Converters;
using Shuriken.ViewModels;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Shuriken.Models
{
    public class Texture
    {
        public uint ID { get; private set; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public BitmapSource ImageSource { get; private set; }
        private System.Drawing.Bitmap bitmap;

        public ObservableCollection<Sprite> Sprites { get; set; }
        
        private void CreateGLTexture()
        {
            uint id = 0;
            GL.GenTextures(1, out id);

            ID = id;

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Load the image
            Image<Rgba32> image = BitmapConverter.ToImageSharpImage<Rgba32>(bitmap);

            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }

        public Texture(string filename)
        {
            Name = Path.GetFileNameWithoutExtension(filename);
            bitmap = DDSCodec.DecompressImage(filename);
            ImageSource = BitmapConverter.Bitmap2BitmapImage(bitmap);
            Width = ImageSource.PixelWidth;
            Height = ImageSource.PixelHeight;
            Sprites = new ObservableCollection<Sprite>();

            CreateGLTexture();
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
