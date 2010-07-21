//-----------------------------------------------------------------------
// <copyright file=".cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Views
{
    public abstract class BaseView<TModel> : SparkView<TModel> where TModel : ControllerResponse
    {
        protected string HashString(string Value)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(Value);
            data = md5.ComputeHash(data);

            string ret = "";
            for (int i = 0; i < data.Length; i++)
            {
                ret += data[i].ToString("x2").ToLowerInvariant();
            }

            return ret;
        }

        protected string FormatPostText(string postText)
        {
            return this.Model.AppManager.Controller.FormatPostText(postText);
        }

        protected static string CalculateScrapeBuster(string scrapeBusterKey, object additionalData)
        {
            return LanLordzApplicationManager.CalculateScrapeBuster(scrapeBusterKey, additionalData);
        }

        protected MvcHtmlString UserLink(User user)
        {
            return this.UserLink(user.UserID, user.Username, user.Username);
        }

        protected MvcHtmlString UserLink(UserInformation user)
        {
            return this.UserLink(user.UserID, user.Username, user.Username);
        }

        protected MvcHtmlString UserLink(long userId, string username)
        {
            return this.UserLink(userId, username, username);
        }

        protected MvcHtmlString UserLink(long userId, string username, string displayName)
        {
            var routeValues = new
            {
                id = userId,
                title = Model.Controller.CreateUrlTitle(username)
            };

            return Html.ActionLink(displayName, "ViewProfile", "Account", routeValues, null);
        }

        protected string UserAvatar(long userId, string username, bool userHasAvatar, string email)
        {
            if (userHasAvatar)
            {
                return "<img alt=\"" +
                    Html.AttributeEncode(username + "'s Avatar") +
                    "\" src=\"" +
                    Html.AttributeEncode(Url.Action("ViewAvatar", "Images", new
                    {
                        id = userId
                    })) +
                    "\" />";
            }
            else
            {
                return "<img alt=\"" +
                    Html.AttributeEncode(username + "'s Avatar") +
                    "\" src=\"" +
                    "http://www.gravatar.com/avatar/" + HashString(email) + ".jpg?d=wavatar" +
                    "\" />";
            }
        }
    }
}
