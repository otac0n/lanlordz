//-----------------------------------------------------------------------
// <copyright file="AddressDetails.cs" company="LAN Lordz, inc.">
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
    using System.Globalization;
    using System.Xml;

    public class AddressDetails
    {
        private int? accuracy;
        private string countryName;
        private string countryNameCode;
        private string administrativeAreaName;
        private string localityName;
        private string thoroughfareName;
        private int? postalCodeNumber;

        internal AddressDetails(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!node.Name.Equals("AddressDetails"))
            {
                throw new ArgumentException("The provided XmlNode was not a valid AddressDetails node.", "node");
            }

            if (node.Attributes["Accuracy"] != null)
            {
                this.accuracy = int.Parse(node.Attributes["Accuracy"].Value, CultureInfo.InvariantCulture);
            }

            XmlNode countryNode = node.SelectSingleNode("add:Country", nsmgr);
            if (countryNode != null)
            {
                if (countryNode.SelectSingleNode("add:CountryName", nsmgr) != null)
                {
                    this.countryName = countryNode.SelectSingleNode("add:CountryName", nsmgr).InnerText;
                }

                if (countryNode.SelectSingleNode("add:CountryNameCode", nsmgr) != null)
                {
                    this.countryNameCode = countryNode.SelectSingleNode("add:CountryNameCode", nsmgr).InnerText;
                }

                XmlNode adminAreaNode = countryNode.SelectSingleNode("add:AdministrativeArea", nsmgr);
                if (adminAreaNode != null)
                {
                    if (adminAreaNode.SelectSingleNode("add:AdministrativeAreaName", nsmgr) != null)
                    {
                        this.administrativeAreaName = adminAreaNode.SelectSingleNode("add:AdministrativeAreaName", nsmgr).InnerText;
                    }

                    XmlNode localityNode = adminAreaNode.SelectSingleNode("add:Locality", nsmgr);
                    if (localityNode != null)
                    {
                        if (localityNode.SelectSingleNode("add:LocalityName", nsmgr) != null)
                        {
                            this.localityName = localityNode.SelectSingleNode("add:LocalityName", nsmgr).InnerText;
                        }

                        if (localityNode.SelectSingleNode("add:Thoroughfare/add:ThoroughfareName", nsmgr) != null)
                        {
                            this.thoroughfareName = localityNode.SelectSingleNode("add:Thoroughfare/add:ThoroughfareName", nsmgr).InnerText;
                        }

                        if (localityNode.SelectSingleNode("add:PostalCode/add:PostalCodeNumber", nsmgr) != null)
                        {
                            this.postalCodeNumber = int.Parse(localityNode.SelectSingleNode("add:PostalCode/add:PostalCodeNumber", nsmgr).InnerText, CultureInfo.CurrentUICulture);
                        }
                    }
                }
            }
        }

        public int? Accuracy
        {
            get
            {
                return this.accuracy;
            }
        }

        public string CountryName
        {
            get
            {
                return this.countryName;
            }
        }

        public string CountryNameCode
        {
            get
            {
                return this.countryNameCode;
            }
        }

        public string AdministrativeAreaName
        {
            get
            {
                return this.administrativeAreaName;
            }
        }

        public string LocalityName
        {
            get
            {
                return this.localityName;
            }
        }

        public string ThoroughfareName
        {
            get
            {
                return this.thoroughfareName;
            }
        }

        public int? PostalCodeNumber
        {
            get
            {
                return this.postalCodeNumber;
            }
        }
    }
}