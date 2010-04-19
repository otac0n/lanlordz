//-----------------------------------------------------------------------
// <copyright file="PlacemarkPoint.cs" company="LAN Lordz, inc.">
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
    using System.Xml;

    public class PlacemarkPoint
    {
        private decimal latitude;
        private decimal longitude;
        private decimal altitude;

        internal PlacemarkPoint(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!node.Name.Equals("Point"))
            {
                throw new ArgumentException("The provided XmlNode was not a valid Point node.", "node");
            }

            if (node.SelectSingleNode("kml:coordinates", nsmgr) != null)
            {
                string[] coordinates = node.SelectSingleNode("kml:coordinates", nsmgr).InnerText.Split(",".ToCharArray());
                this.latitude = decimal.Parse(coordinates[0]);
                this.longitude = decimal.Parse(coordinates[1]);
                this.altitude = decimal.Parse(coordinates[2]);
            }
        }

        public decimal Latitude
        {
            get
            {
                return this.latitude;
            }
        }

        public decimal Longitude
        {
            get
            {
                return this.longitude;
            }
        }

        public decimal Altitude
        {
            get
            {
                return this.altitude;
            }
        }
    }
}
