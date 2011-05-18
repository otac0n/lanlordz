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

namespace LanLordz.Models
{
    using System.Collections.Generic;
    using LanLordz.Controllers;
    using LanLordz.Models.CachedModels;

    public class CreateTournamentModel
    {
        public IEnumerable<CachedEvent> Events
        {
            get;
            set;
        }

        public IDictionary<string, string> ScoreModes
        {
            get;
            set;
        }

        public IList<string> PairingsGenerators
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public long Event
        {
            get;
            set;
        }

        public int TeamSize
        {
            get;
            set;
        }

        public string Game
        {
            get;
            set;
        }

        public string GameInfo
        {
            get;
            set;
        }

        public string ServerSettings
        {
            get;
            set;
        }

        public string ScoreMode
        {
            get;
            set;
        }

        public string PairingsGenerator
        {
            get;
            set;
        }

        public string TDUsername
        {
            get;
            set;
        }
    }
}
