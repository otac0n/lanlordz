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

namespace LanLordz.Models
{
    public class UserInformation
    {
        public long UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public char Gender { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool ReceiveAdminEmail { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowGender { get; set; }
        public bool HasAvatar { get; set; }
        public int Posts { get; set; }
        public int EventsRegistered { get; set; }
        public int EventsCheckedIn { get; set; }

        public string Biography { get; set; }

        public string Location
        {
            get;
            set;
        }

        public string Occupation
        {
            get;
            set;
        }

        public string Interests
        {
            get;
            set;
        }

        public string Website
        {
            get;
            set;
        }

        public string Theme
        {
            get;
            set;
        }

        public string TimeZone
        {
            get;
            set;
        }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return string.IsNullOrEmpty(this.TimeZone) ? null : TimeZoneInfo.FindSystemTimeZoneById(this.TimeZone);
            }
        }

        public string Signature
        {
            get;
            set;
        }
    }
}
