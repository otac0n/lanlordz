//-----------------------------------------------------------------------
// <copyright file="BaseView.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Views
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using LanLordz.Controllers;
    using LanLordz.Models;
    using LanLordz.Controllers.CachedRepositories;
    using System.Web;

    public static class HtmlUserExtensions
    {
        public static LanLordzBaseController GetController(this HtmlHelper html)
        {
            return html.ViewContext.Controller as LanLordzBaseController;
        }

        public static ConfigurationRepository GetConfig(this HtmlHelper html)
        {
            var controller = html.GetController();
            return controller.Config;
        }

        public static MvcHtmlString LocalContent(this HtmlHelper html, string virtualPath)
        {
            var controller = html.GetController();
            return new MvcHtmlString(controller.ApplyCoralCdn(VirtualPathUtility.ToAbsolute(virtualPath)));
        }

        public static MvcHtmlString BBCode(this HtmlHelper html, string postText)
        {
            var controller = html.GetController();
            return new MvcHtmlString(controller.FormatPostText(postText));
        }

        public static MvcHtmlString UserLink(this HtmlHelper html, User user)
        {
            return html.UserLink(user.UserID, user.Username, user.Username);
        }

        public static MvcHtmlString UserLink(this HtmlHelper html, UserInformation user)
        {
            return html.UserLink(user.UserID, user.Username, user.Username);
        }

        public static MvcHtmlString UserLink(this HtmlHelper html, long userId, string username)
        {
            return html.UserLink(userId, username, username);
        }

        public static MvcHtmlString UserLink(this HtmlHelper html, long userId, string username, string displayName)
        {
            var controller = html.ViewContext.Controller as LanLordzBaseController;

            var routeValues = new
            {
                id = userId,
                title = controller.CreateUrlTitle(username),  // TODO: Remove this dependency on the controller.
            };

            return html.ActionLink(displayName, "ViewProfile", "Account", routeValues, null);
        }

        public static MvcHtmlString UserAvatar<TModel>(this HtmlHelper<TModel> html, UserInformation user)
        {
            return html.UserAvatar(user.UserID, user.Username, user.HasAvatar, user.Email);
        }

        public static MvcHtmlString UserAvatar<TModel>(this HtmlHelper<TModel> html, long userId, string username, bool userHasAvatar, string email)
        {
            var url = ((BaseView<TModel>)html.ViewContext.View).Url;

            string imgUrl;

            if (userHasAvatar)
            {
                imgUrl = url.Action("ViewAvatar", "Images", new { id = userId });
            }
            else
            {
                imgUrl = "http://www.gravatar.com/avatar/" + HashString(email) + ".jpg?d=wavatar";
            }

            return new MvcHtmlString("<img alt=\"" + html.AttributeEncode(username + "'s Avatar") + "\" src=\"" + html.AttributeEncode(imgUrl) + "\" />");
        }

        private static string HashString(string Value)
        {
            byte[] data = Encoding.UTF8.GetBytes(Value);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                data = md5.ComputeHash(data);
            }

            string ret = "";
            for (int i = 0; i < data.Length; i++)
            {
                ret += data[i].ToString("x2").ToLowerInvariant();
            }

            return ret;
        }
    }
}
