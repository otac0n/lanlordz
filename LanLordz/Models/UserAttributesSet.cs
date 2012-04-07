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

using System.Collections.Generic;
using System.Linq;

namespace LanLordz.Models
{
    public class UserAttributesSet
    {
        public string Location { get; set; }

        public string Website { get; set; }

        public string Occupation { get; set; }

        public string Interests { get; set; }

        public string Signature { get; set; }

        public string Biography { get; set; }

        public string Theme { get; set; }

        public string TimeZone { get; set; }

        public bool IsValid
        {
            get
            {
                return (this.GetRuleViolations().Count() == 0);
            }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.Location != null && this.Location.Length > 50)
                yield return new RuleViolation("Your location must be less than 50 characters.", "Location");

            if (this.Website != null && this.Website.Length > 50)
                yield return new RuleViolation("Your website must be less than 50 characters.", "Website");

            if (this.Occupation != null && this.Occupation.Length > 50)
                yield return new RuleViolation("Your occupation must be less than 50 characters.", "Occupation");

            if (this.Interests != null && this.Interests.Length > 50)
                yield return new RuleViolation("Your interests must be less than 50 characters.", "Interests");

            if (this.Signature != null && this.Signature.Length > 500)
                yield return new RuleViolation("Your signature must be less than 500 characters.", "Signature");

            if (this.Biography != null && this.Biography.Length > 1500)
                yield return new RuleViolation("Your biography must be less than 1500 characters.", "Biography");

            yield break;
        }
    }
}
