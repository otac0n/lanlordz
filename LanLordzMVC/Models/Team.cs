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
using System.Data.Linq;

namespace LanLordz.Models
{
    public partial class Team
    {
        public bool IsValid
        {
            get
            {
                return (this.GetRuleViolations().Count() == 0);
            }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.TeamName))
                yield return new RuleViolation("Team names must be at least one characters.", "TeamName");

            if (string.IsNullOrEmpty(this.TeamTagFormat) || this.TeamTagFormat.Length < 3 )
                yield return new RuleViolation("Team tags must be at least three characters.", "TeamTagFormat");

            bool formatValid = false;

            string magic = Guid.NewGuid().ToString("P");
            string result = "";

            if (!string.IsNullOrEmpty(this.TeamTagFormat))
            {
                try
                {
                    result = string.Format(this.TeamTagFormat, magic);

                    formatValid = true;
                }
                catch(FormatException)
                {
                }

                if (!formatValid)
                {
                    yield return new RuleViolation("The team tag format was invalid.  Use '{{' or '}}' for a single curly brace.", "TeamTagFormat");
                }
                else
                {
                    if (!result.Contains(magic))
                    {
                        yield return new RuleViolation("Team tags must contain '{0}' as a place holder for the username.", "TeamTagFormat");
                    }
                    else if (result.IndexOf(magic) != result.LastIndexOf(magic))
                    {
                        yield return new RuleViolation("Team tags may only contain one instance of '{0}'.", "TeamTagFormat");
                    }
                }
            }

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
