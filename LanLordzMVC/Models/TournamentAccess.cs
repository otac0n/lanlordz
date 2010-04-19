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
    public class TournamentAccess
    {
        private bool canCreateTeam;
        private bool canJoinTeam;
        private bool canLeaveTeam;
        private bool canDisbandTeam;
        private bool canManageTeam;
        private bool canInputScores;
        private bool canStartNextRound;
        private bool canRollBackRound;
        private bool canLockTeams;
        private bool canUnlockTeams;
        private bool canEditTeams;

        public TournamentAccess(bool canCreateTeam, bool canJoinTeam, bool canLeaveTeam, bool canDisbandTeam, bool canManageTeam, bool canInputScores, bool canStartNextRound, bool canRollBackRound, bool canLockTeams, bool canUnlockTeams, bool canEditTeams)
        {
            this.canCreateTeam = canCreateTeam;
            this.canDisbandTeam = canDisbandTeam;
            this.canInputScores = canInputScores;
            this.canJoinTeam = canJoinTeam;
            this.canLeaveTeam = canLeaveTeam;
            this.canManageTeam = canManageTeam;
            this.canRollBackRound = canRollBackRound;
            this.canStartNextRound = canStartNextRound;
            this.canLockTeams = canLockTeams;
            this.canUnlockTeams = canUnlockTeams;
            this.canEditTeams = canEditTeams;
        }

        public bool CanCreateTeam
        {
            get
            {
                return this.canCreateTeam;
            }
        }

        public bool CanJoinTeam
        {
            get
            {
                return this.canJoinTeam;
            }
        }

        public bool CanLeaveTeam
        {
            get
            {
                return this.canLeaveTeam;
            }
        }

        public bool CanDisbandTeam
        {
            get
            {
                return this.canDisbandTeam;
            }
        }

        public bool CanManageTeam
        {
            get
            {
                return this.canManageTeam;
            }
        }

        public bool CanInputScores
        {
            get
            {
                return this.canInputScores;
            }
        }

        public bool CanStartNextRound
        {
            get
            {
                return this.canStartNextRound;
            }
        }

        public bool CanRollBackRound
        {
            get
            {
                return this.canRollBackRound;
            }
        }

        public bool CanLockTeams
        {
            get
            {
                return this.canLockTeams;
            }
        }

        public bool CanUnlockTeams
        {
            get
            {
                return this.canUnlockTeams;
            }
        }

        public bool CanEditTeams
        {
            get
            {
                return this.canEditTeams;
            }
        }
    }
}
