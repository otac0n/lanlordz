//-----------------------------------------------------------------------
// <copyright file="BaseView`1.cs" company="LAN Lordz, inc.">
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

using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc.Html;
using LanLordz.Models;
using Spark.Web.Mvc;
using System.Web.Mvc;
using LanLordz.Controllers;

namespace LanLordz.Views
{
    public abstract class BaseView<TModel> : SparkView<TModel>
    {
        public LanLordzBaseController Controller
        {
            get
            {
                return ViewBag.Controller;
            }
        }

        protected string Theme
        {
            get
            {
                var controller = this.Controller;

                var theme = string.Empty;
                if (this.Controller.CurrentUser != null)
                {
                    theme = controller.GetUserInformation(controller.CurrentUser.UserID, false).Theme;
                }

                if (string.IsNullOrEmpty(theme))
                {
                    theme = controller.Config.DefaultTheme;
                }

                return theme;
            }
        }

        protected string FormatPostText(string postText)
        {
            return this.Controller.FormatPostText(postText);
        }

        protected static string CalculateScrapeBuster(string scrapeBusterKey, object additionalData)
        {
            return LanLordzBaseController.CalculateScrapeBuster(scrapeBusterKey, additionalData);
        }
    }
}
