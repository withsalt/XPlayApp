using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OnceMi.AspNetCore.FFmpeg.Utils
{
    public class ImageTools
    {
        public static Image ResizeImage(Image image, int width, int height)
        {
            var nw = (float)image.Width;
            var nh = (float)image.Height;
            if (nw > width)
            {
                nh = width * nh / nw;
                nw = width;
            }
            if (nh > height)
            {
                nw = height * nw / nh;
                nh = height;
            }

            var result = new Bitmap(width, height);
            try
            {
                try
                {
                    if (image.HorizontalResolution == 0 || image.VerticalResolution == 0)
                    {
                        result.SetResolution(96, 96);
                    }
                    else
                    {
                        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Set image resolution({image.HorizontalResolution},{image.VerticalResolution}) failed, {ex.Message}", ex);
                }
                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, result.Width, result.Height));
                    graphics.DrawImage(image, new Rectangle(
                      (int)(result.Width - nw) / 2,
                      (int)(result.Height - nh) / 2,
                      (int)nw, (int)nh
                      ));
                }
                return result;
            }
            catch (Exception)
            {
                result.Dispose();
                throw;
            }
        }
    }
}
