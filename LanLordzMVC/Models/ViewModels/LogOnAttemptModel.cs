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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LanLordz.Controllers.CachedRepositories;
using LanLordz.Controllers;

namespace LanLordz.Models
{
    public class LogOnAttemptModel : ControllerResponse
    {
        public LogOnAttemptModel(LanLordzBaseController controller)
            : base(controller)
        {
        }

        public string ReturnUrl
        {
            get;
            set;
        }
        public string Username
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }
        public bool RememberMe
        {
            get;
            set;
        }
        public bool RequireCaptcha
        {
            get;
            set;
        }
    }
}