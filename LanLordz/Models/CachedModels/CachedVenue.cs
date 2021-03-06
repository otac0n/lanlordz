﻿//-----------------------------------------------------------------------
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

namespace LanLordz.Models.CachedModels
{
    public class CachedVenue
    {
        public CachedVenue(Venue baseVenue)
        {
            if (baseVenue == null)
            {
                throw new ArgumentNullException("baseEvent");
            }

            this.VenueID = baseVenue.VenueID;
            this.Name = baseVenue.Name;
            this.Address = baseVenue.Address;
            this.Latitude = baseVenue.Latitude;
            this.Longitude = baseVenue.Longitude;
        }

        public long VenueID { get; private set; }

        public string Name { get; private set; }

        public string Address { get; private set; }

        public System.Nullable<decimal> Latitude { get; private set; }

        public System.Nullable<decimal> Longitude { get; private set; }
    }
}
