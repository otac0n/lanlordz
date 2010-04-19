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
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;

    partial class Tournament
    {
        public bool IsSinglePlayer
        {
            get
            {
                return (this.TeamSize == 1);
            }
        }

        public bool IsValid
        {
            get
            {
                return (this.GetRuleViolations().Count() == 0);
            }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.Title))
                yield return new RuleViolation("Titles of tournaments must not be empty.", "Title");

            if (this.EventID == 0)
                yield return new RuleViolation("Events of tournaments must not be empty.", "EventID");

            if (this.TeamSize <= 0)
                yield return new RuleViolation("Team Size must be greater than zero.", "TeamSize");

            yield break;
        }

        partial void OnValidate(ChangeAction action)
        {
            if (!this.IsValid)
            {
                throw new ApplicationException("Rule violations prevent saving.");
            }
        }
    }
}
