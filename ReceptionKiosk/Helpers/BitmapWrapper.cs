using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace ReceptionKiosk.Helpers
{
    /// <summary>
    /// Wraps a Bitmap and a BitmapSource in on class
    /// </summary>
    public class BitmapWrapper
    {
        public SoftwareBitmap Bitmap { get; set; }
        public SoftwareBitmapSource BitmapSource { get; set; }

        public BitmapWrapper(SoftwareBitmap bitmap, SoftwareBitmapSource bitmapSource)
        {
            Bitmap = bitmap;
            BitmapSource = bitmapSource;
        }
    }
}
