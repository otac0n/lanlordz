//-----------------------------------------------------------------------
// <copyright file="Appointment.cs" company="LAN Lordz, inc.">
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
    using System.Text;

    public class Appointment
    {
        public string Title
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public string Organizer
        {
            get;
            set;
        }

        public string Location
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string AsICalendar()
        {
            StringBuilder o = new StringBuilder();

            o.Append("BEGIN:VCALENDAR\r\n");
            o.Append("VERSION:2.0\r\n");
            o.Append("PRODID:-//LAN Lordz, Inc.//LanLordz.SiteTools//EN\r\n");

            o.Append("BEGIN:VEVENT\r\n");

            o.Append("DTSTART:" + FormatDateTime(this.StartTime) + "\r\n");
            o.Append("DTEND:" + FormatDateTime(this.EndTime) + "\r\n");

            if (!string.IsNullOrEmpty(this.Organizer))
            {
                o.Append("ORGANIZER:" + FormatText(this.Organizer) + "\r\n");
            }

            if (!string.IsNullOrEmpty(this.Location))
            {
                o.Append("LOCATION:" + FormatText(this.Location) + "\r\n");
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                o.Append("DESCRIPTION:" + FormatText(this.Description) + "\r\n");
            }

            if (!string.IsNullOrEmpty(this.Title))
            {
                o.Append("SUMMARY:" + FormatText(this.Title) + "\r\n");
            }

            o.Append("END:VEVENT\r\n");

            o.Append("END:VCALENDAR");

            return o.ToString();
        }

        private static string FormatText(string text)
        {
            var formatted = text.Replace(@"\", @"\\");
            formatted = formatted.Replace(",", @"\,");
            formatted = formatted.Replace(";", @"\;");
            formatted = formatted.Replace("\r\n;", @"\n");
            formatted = formatted.Replace("\n;", @"\n");
            formatted = formatted.Replace("\r;", @"\n");
            return formatted;
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("s").Replace("-", string.Empty).Replace(":", string.Empty);
        }
    }
}
