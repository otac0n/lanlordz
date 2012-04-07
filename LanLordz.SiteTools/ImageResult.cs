//-----------------------------------------------------------------------
// <copyright file="ImageResult.cs" company="LAN Lordz, inc.">
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
    using System.Web;
    using System.Web.Mvc;

    public static class ImageResultHelper
    {
        public static ImageResult Image(this Controller controller, byte[] imageData, string mimeType)
        {
            return new ImageResult
            {
                ImageData = imageData,
                MimeType = mimeType
            };
        }

        public static ImageResult Image(this Controller controller, byte[] imageData, string mimeType, HttpCacheability cacheability, DateTime expires, string eTag)
        {
            return new ImageResult
            {
                ImageData = imageData,
                MimeType = mimeType,
                Cacheability = cacheability,
                Expires = expires,
                ETag = eTag
            };
        }
    }

    public class ImageResult : ActionResult
    {
        public ImageResult()
        {
        }

        public byte[] ImageData { get; set; }

        public string MimeType { get; set; }

        public HttpCacheability Cacheability { get; set; }

        public string ETag { get; set; }

        public DateTime? Expires { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (this.ImageData == null)
            {
                throw new ArgumentNullException("ImageData");
            }

            if (string.IsNullOrEmpty(this.MimeType))
            {
                throw new ArgumentNullException("MimeType");
            }

            context.HttpContext.Response.ContentType = this.MimeType;

            if (!string.IsNullOrEmpty(this.ETag))
            {
                context.HttpContext.Response.Cache.SetETag(this.ETag);
            }

            if (this.Expires.HasValue)
            {
                context.HttpContext.Response.Cache.SetCacheability(this.Cacheability);
                context.HttpContext.Response.Cache.SetExpires(this.Expires.Value);
            }

            context.HttpContext.Response.OutputStream.Write(this.ImageData, 0, this.ImageData.Length);
        }
    }
}