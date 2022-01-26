using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using DirectXTexNet;


namespace Shuriken.Converters
{
    public static class BitmapConverter
    {
        public static BitmapSource FromBitmap(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return i;
        }

        public static Bitmap FromTextureImage(ScratchImage img, PixelFormat format)
        {
            return new Bitmap(img.GetImage(0).Width, img.GetImage(0).Height,
                (int)img.GetImage(0).RowPitch, format, img.GetImage(0).Pixels);
        }
    }

}
