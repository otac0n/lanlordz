//-----------------------------------------------------------------------
// <copyright file="Placemark.cs" company="LAN Lordz, inc.">
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

    public class Placemark
    {
        private string address = null;
        private AddressDetails addressDetails = null;
        private PlacemarkPoint point = null;

        internal Placemark(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!node.Name.Equals("Placemark"))
            {
                throw new ArgumentException("The provided XmlNode was not a valid Placemark node.", "node");
            }

            XmlNode addressNode = node.SelectSingleNode("kml:address", nsmgr);
            if (addressNode != null)
            {
                this.address = addressNode.InnerText;
            }

            XmlNode detailsNode = node.SelectSingleNode("add:AddressDetails", nsmgr);
            if (detailsNode != null)
            {
                this.addressDetails = new AddressDetails(detailsNode, nsmgr);
            }

            XmlNode pointNode = node.SelectSingleNode("kml:Point", nsmgr);
            if (pointNode != null)
            {
                this.point = new PlacemarkPoint(pointNode, nsmgr);
            }
        }

        public string Address
        {
            get
            {
                return this.address;
            }
        }

        public AddressDetails AddressDetails
        {
            get
            {
                return this.addressDetails;
            }
        }

        public PlacemarkPoint Point
        {
            get
            {
                return this.point;
            }
        }
    }
}
