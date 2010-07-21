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

namespace LanLordz.Models.CachedModels
{
    public class CachedTournament
    {
        public long TournamentID { get; private set; }
        public long EventID { get; private set; }
        public string Title { get; private set; }
        public string Game { get; private set; }
        public string PairingsGenerator { get; private set; }
        public int TeamSize { get; private set; }
        public bool AllowLateEntry { get; private set; }
        public System.Nullable<long> TournamentDirectorUserID { get; private set; }
        public bool IsLocked { get; private set; }
        public string GameInfo { get; private set; }
        public string ServerSettings { get; private set; }
        public string ScoreMode { get; private set; }

        public CachedTournament(Tournament baseTournament)
        {
            if (baseTournament == null)
            {
                throw new ArgumentNullException("baseTournament");
            }

            this.TournamentID = baseTournament.TournamentID;
            this.EventID = baseTournament.EventID;
            this.Title = baseTournament.Title;
            this.Game = baseTournament.Game;
            this.PairingsGenerator = baseTournament.PairingsGenerator;
            this.TeamSize = baseTournament.TeamSize;
            this.AllowLateEntry = baseTournament.AllowLateEntry;
            this.TournamentDirectorUserID = baseTournament.TournamentDirectorUserID;
            this.IsLocked = baseTournament.IsLocked;
            this.GameInfo = baseTournament.GameInfo;
            this.ServerSettings = baseTournament.ServerSettings;
            this.ScoreMode = baseTournament.ScoreMode;
        }
    }
}
