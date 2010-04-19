//-----------------------------------------------------------------------
// <copyright file="HttpErrorResult.cs" company="LAN Lordz, inc.">
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
    using System.Web.Mvc;

    public static class HttpErrorResultHelper
    {
        public static HttpErrorResult HttpError(this Controller controller, int statusCode)
        {
            return new HttpErrorResult
            {
                StatusCode = statusCode
            };
        }

        public static HttpErrorResult HttpError(this Controller controller, int statusCode, ActionResult chainedAction)
        {
            return new HttpErrorResult
            {
                StatusCode = statusCode,
                ChainedAction = chainedAction,
            };
        }
    }

    public sealed class HttpErrorResult : ActionResult
    {
        public int StatusCode
        {
            get;
            set;
        }

        public ActionResult ChainedAction
        {
            get;
            set;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            context.HttpContext.Response.StatusCode = this.StatusCode;

            if (this.ChainedAction != null)
            {
                this.ChainedAction.ExecuteResult(context);
            }
        }
    }
}