using CMC.Presentation.Application.DTOs.Questions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace CMC.Presentation.Application.Helpers
{
    public static class ImageProcessing
    {

        public static byte[] GetImageBytes(IFormFile formFile)
        {
            using (Stream stream = formFile.OpenReadStream())
            {
                using (Image imgOriginal = Image.FromStream(stream, true, true))
                {
                    using (Image imgActual = ScaleImage(imgOriginal))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            imgActual.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            byte[] xByte = ms.ToArray();
                            return xByte;
                        }
                    }
                }
            }
        }

        //public static Image ScaleImage(Image originalImage)
        //{
        //    // Define your scaling logic here
        //    // Example:
        //    int newWidth = 400; // Set your desired width
        //    int newHeight = 400; // Set your desired height

        //    Bitmap scaledImage = new Bitmap(newWidth, newHeight);
        //    using (Graphics graphics = Graphics.FromImage(scaledImage))
        //    {
        //        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
        //    }

        //    return scaledImage;
        //}
        public static Image ScaleImage(Image originalImage)
        {
            int newWidth = 400; // Set your desired width
            int newHeight = 400; // Set your desired height

            Bitmap scaledImage = new Bitmap(newWidth, newHeight);
            scaledImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(scaledImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                graphics.Clear(Color.Transparent); // Set background to transparent

                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return scaledImage;
        }

    }
}
