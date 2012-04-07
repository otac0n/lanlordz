//-----------------------------------------------------------------------
// <copyright file="NotModifiedResult.cs" company="LAN Lordz, inc.">
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
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    public static class NotModifiedResultHelper
    {
        public static NotModifiedResult NotModified(this Controller controller)
        {
            return new NotModifiedResult();
        }

        public static NotModifiedResult NotModified(this Controller controller, HttpCacheability cacheability, DateTime expires)
        {
            return new NotModifiedResult
            {
                Cacheability = cacheability,
                Expires = expires
            };
        }
    }

    public class NotModifiedResult : ActionResult
    {
        public HttpCacheability Cacheability { get; set; }

        public DateTime? Expires { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
            context.HttpContext.Response.SuppressContent = true;

            if (this.Expires.HasValue)
            {
                context.HttpContext.Response.Cache.SetCacheability(this.Cacheability);
                context.HttpContext.Response.Cache.SetExpires(this.Expires.Value);
            }
        }
    }
}