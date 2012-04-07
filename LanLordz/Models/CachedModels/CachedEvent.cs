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

namespace LanLordz.Models.CachedModels
{
    public class CachedEvent
    {
        public CachedEvent(Event baseEvent)
        {
            if (baseEvent == null)
            {
                throw new ArgumentNullException("baseEvent");
            }

            this.EventID = baseEvent.EventID;
            this.VenueID = baseEvent.VenueID;
            this.BeginDateTime = baseEvent.BeginDateTime;
            this.EndDateTime = baseEvent.EndDateTime;
            this.Info = baseEvent.Info;
            this.Seats = baseEvent.Seats;
            this.Title = baseEvent.Title;
        }

        public long EventID { get; private set; }

        public long VenueID { get; private set; }

        public string Title { get; private set; }

        public string Info { get; private set; }

        public System.DateTime BeginDateTime { get; private set; }

        public System.DateTime EndDateTime { get; private set; }

        public long Seats { get; private set; }
    }
}
