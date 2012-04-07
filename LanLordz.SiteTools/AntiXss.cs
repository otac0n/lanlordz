//-----------------------------------------------------------------------
// <copyright file="AntiXss.cs" company="LAN Lordz, inc.">
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
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AntiXss
    {
        public static string SanitizeUrl(string url)
        {
            var validUrlSchemes = new Regex("^(http|https|ftp)://");
            var ret = url.TrimStart();

            if (!validUrlSchemes.IsMatch(ret))
            {
                ret = ret.Replace(":", string.Empty);
            }

            if (!ret.Contains(':'))
            {
                ret = "http://" + ret;
            }

            return ret;
        }
    }
}
