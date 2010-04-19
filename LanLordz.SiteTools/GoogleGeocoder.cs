//-----------------------------------------------------------------------
// <copyright file="GoogleGeocoder.cs" company="LAN Lordz, inc.">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Xml;

    public class GoogleGeocoder
    {
        private string apiKey;

        public GoogleGeocoder(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public Placemark Geocode(string location)
        {
            string url = "http://maps.google.com/maps/geo?key=" + Uri.EscapeUriString(this.apiKey) + "&sensor=false&output=xml&oe=utf8&q=" + Uri.EscapeUriString(location);

            string data;

            using (WebClient client = new WebClient())
            {
                using (Stream response = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(response))
                    {
                        data = reader.ReadToEnd();
                    }
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("kml", "http://earth.google.com/kml/2.0");
            nsmgr.AddNamespace("add", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");

            List<Placemark> placemarks = new List<Placemark>();
            foreach (XmlNode node in doc.SelectNodes("/kml:kml/kml:Response/kml:Placemark", nsmgr))
            {
                placemarks.Add(new Placemark(node, nsmgr));
            }

            if (placemarks.Count == 0)
            {
                return null;
            }
            else
            {
                return placemarks[0];
            }
        }
    }
}