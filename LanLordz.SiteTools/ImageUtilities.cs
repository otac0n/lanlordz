//-----------------------------------------------------------------------
// <copyright file="ImageUtilities.cs" company="LAN Lordz, inc.">
//  Copyright © 2010 LAN Lordz, inc.
//
//  This file is part of The LAN Lordz LAN Party System.
//
//  The LAN Lordz LAN Party System is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  The LAN Lordz LAN Party System is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with The LAN Lordz LAN Party System.  If not, see [http://www.gnu.org/licenses/].
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

namespace LanLordz.SiteTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;

    public static class ImageUtilities
    {
        public static byte[] ResizeToFit(byte[] imageData, Size size, bool expand)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        if (size.Width == img.Width &&
                            size.Height == img.Height)
                        {
                            return imageData;
                        }

                        var width = (float)size.Width;
                        var height = (float)size.Height;

                        var xRatio = width / img.Width;
                        var yRatio = height / img.Height;

                        var ratio = Math.Min(xRatio, yRatio);

                        // If we are not expanding the image to fit and the resulting ratio indicates expansion, do not transform the image.
                        if (!expand && ratio >= 1.0f)
                        {
                            return imageData;
                        }

                        var newSize = new Size(Math.Min(size.Width, (int)Math.Round(img.Width * ratio, MidpointRounding.AwayFromZero)), Math.Min(size.Height, (int)Math.Round(img.Height * ratio, MidpointRounding.AwayFromZero)));

                        // Invialidate internally stored thumbnails.
                        img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                        img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

                        var newImage = new Bitmap(newSize.Width, newSize.Height);

                        using (var g = Graphics.FromImage(newImage))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(img, new Rectangle(new Point(0, 0), newSize));
                        }

                        using (MemoryStream outStream = new MemoryStream(1024))
                        {
                            newImage.Save(outStream, ImageFormat.Jpeg);

                            return outStream.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return imageData;
            }
        }

        public static string GetImageMimeType(byte[] imageData)
        {
            String mimeType = "image/unknown";

            try
            {
                Guid id;

                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        id = img.RawFormat.Guid;
                    }
                }

                if (id == ImageFormat.Png.Guid)
                {
                    mimeType = "image/png";
                }
                else if (id == ImageFormat.Bmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Emf.Guid)
                {
                    mimeType = "image/x-emf";
                }
                else if (id == ImageFormat.Exif.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.Gif.Guid)
                {
                    mimeType = "image/gif";
                }
                else if (id == ImageFormat.Icon.Guid)
                {
                    mimeType = "image/ico";
                }
                else if (id == ImageFormat.Jpeg.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.MemoryBmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Tiff.Guid)
                {
                    mimeType = "image/tiff";
                }
                else if (id == ImageFormat.Wmf.Guid)
                {
                    mimeType = "image/wmf";
                }
            }
            catch
            {
            }

            return mimeType;
        }
    }
}
