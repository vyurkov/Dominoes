using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DominoClient.DominoesService;
using Image = System.Windows.Controls.Image;

namespace DominoClient
{
    class BoneButton:Button
    {
        public int _width
        {
            get { return 40; }
        }
        public int _height
        {
            get { return 80; }
        }
        private const string _spriteMapPath = "Images\\bone-sprite.gif";
        public Bone _Bone { get; set; }
        
        public BoneButton(Bone bone, RoutedEventHandler handler, bool isOpponent = false)
        {
            if (isOpponent) this.IsEnabled = false;
            this.Width = _width;
            this.Height = _height;
            this.Margin = new Thickness(2);
            _Bone = bone;
            SetBackground(bone);
            this.Style = (Style)FindResource("BoneMouseOverButton");
            this.Click += handler;
        }

        private void SetBackground(Bone bn)
        {
            const int width = 40;
            const int height = 40;
            var x1 = bn.FirstValue * 40;
            var y1 = 0;
            var x2 = bn.SecondValue * 40;
            var y2 = 0;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(_spriteMapPath, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            try
            {
                int bytesPerPix = bi.Format.BitsPerPixel / 8;
                int stride = bi.PixelWidth * bytesPerPix;

                var pixelBuffer = new byte[bi.PixelHeight * stride];

                //WriteableBitmap wb = new WriteableBitmap(width * 2, height, bi.DpiX, bi.DpiY, bi.Format, bi.Palette);
                WriteableBitmap wb = new WriteableBitmap(width, height * 2, bi.DpiX, bi.DpiY, bi.Format, bi.Palette);

                bi.CopyPixels(new Int32Rect(Convert.ToInt32(x1), Convert.ToInt32(y1),
                        Convert.ToInt32(width), Convert.ToInt32(height)),
                    pixelBuffer, stride, 0);
                var firstImage = BitmapImage.Create(Convert.ToInt32(width), Convert.ToInt32(height),
                    bi.DpiX, bi.DpiY, bi.Format, bi.Palette, pixelBuffer, stride);

                bi.CopyPixels(new Int32Rect(Convert.ToInt32(x2), Convert.ToInt32(y2),
                        Convert.ToInt32(width), Convert.ToInt32(height)),
                    pixelBuffer, stride, 0);
                var secondImage = BitmapImage.Create(Convert.ToInt32(width), Convert.ToInt32(height),
                    bi.DpiX, bi.DpiY, bi.Format, bi.Palette, pixelBuffer, stride);

                var frsStride = (firstImage.PixelWidth * firstImage.Format.BitsPerPixel + 7) / 8;
                var scndStride = (secondImage.PixelWidth * secondImage.Format.BitsPerPixel + 7) / 8;
                var size = firstImage.PixelHeight * frsStride;
                size = Math.Max(size, secondImage.PixelHeight * scndStride);
                var buffer = new byte[size];

                firstImage.CopyPixels(buffer, frsStride, 0);
                wb.WritePixels(
                    new Int32Rect(0, 0, firstImage.PixelWidth, firstImage.PixelHeight),
                    buffer, frsStride, 0);

                secondImage.CopyPixels(buffer, scndStride, 0);
                //wb.WritePixels(
                //    new Int32Rect(firstImage.PixelWidth, 0, secondImage.PixelWidth, secondImage.PixelHeight),
                //    buffer, scndStride, 0);
                wb.WritePixels(
                    new Int32Rect(0, firstImage.PixelHeight, secondImage.PixelWidth, secondImage.PixelHeight),
                    buffer, scndStride, 0);

                var brush = new ImageBrush();
                brush.ImageSource = wb;
                this.Background = brush;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
        }
    }
}
