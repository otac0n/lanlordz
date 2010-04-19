//-----------------------------------------------------------------------
// <copyright file="CaptchaHelper.cs" company="LAN Lordz, inc.">
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
    using System.IO;
    using System.Web.Mvc;
    using System.Web.UI;
    using Recaptcha;

    public static class CaptchaHelper
    {
        public static string Captcha(this HtmlHelper helper, string publicKey, string privateKey, string id, string theme)
        {
            var captcha = new RecaptchaControl
            {
                ID = id,
                PublicKey = publicKey,
                PrivateKey = privateKey,
                Theme = theme
            };

            var htmlWriter = new HtmlTextWriter(new StringWriter());

            captcha.RenderControl(htmlWriter);

            return htmlWriter.InnerWriter.ToString();
        }

        public static string Captcha(this HtmlHelper helper, string publicKey, string privateKey)
        {
            return Captcha(helper, publicKey, privateKey, "recaptcha", "blackglass");
        }

        public static bool IsCaptchaValid(this Controller controller, string privateKey)
        {
            string challenge = controller.HttpContext.Request.Form["recaptcha_challenge_field"];
            string response = controller.HttpContext.Request.Form["recaptcha_response_field"];

            if (string.IsNullOrEmpty(challenge) || string.IsNullOrEmpty(response))
            {
                return false;
            }

            var validator = new RecaptchaValidator
            {
                PrivateKey = privateKey,
                Challenge = challenge,
                Response = response,
                RemoteIP = controller.HttpContext.Request.UserHostAddress,
            };

            return validator.Validate().IsValid;
        }
    }
}