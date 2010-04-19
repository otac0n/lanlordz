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
using System.Text.RegularExpressions;

namespace LanLordz.Models
{
    public partial class User
    {
        public bool IsValid
        {
            get
            {
                return (this.GetRuleViolations().Count() == 0);
            }
        }

        Regex validEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
        Regex whiteSpace = new Regex(@"\s");

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.Username))
                yield return new RuleViolation("Usernames cannot be blank.", "Username");

            if (!string.IsNullOrEmpty(this.Username) && this.Username.Length > 50)
                yield return new RuleViolation("Usernames must be less than fifty characters.", "Username");

            if (this.Username != null && whiteSpace.IsMatch(this.Username))
                yield return new RuleViolation("Usernames may not contain any whitespace.", "Username");

            if (this.Username != null && (this.Username.Contains("]") || this.Username.Contains("[")))
                yield return new RuleViolation("Usernames may not contain square brackets. Brackets will interfere with BBCode.");
            
            if (string.IsNullOrEmpty(this.PasswordHash))
                yield return new RuleViolation("Passwords must be specified.", "PasswordHash");
            
            if (string.IsNullOrEmpty(this.Email) || !validEmail.IsMatch(this.Email))
                yield return new RuleViolation("The email address you entered is invalid.", "Email");
            
            if (!string.IsNullOrEmpty(this.Email) && this.Email.Length > 50)
                yield return new RuleViolation("Email addresses must be less than fifty characters.", "Email");
            
            if (string.IsNullOrEmpty(this.SecurityQuestion) || this.SecurityQuestion.Length < 3)
                yield return new RuleViolation("Security questions must be at least three characters.", "SecurityQuestion");

            if (!string.IsNullOrEmpty(this.SecurityQuestion) && this.SecurityQuestion.Length > 100)
                yield return new RuleViolation("Security questions must be less than one hundred characters.", "SecurityQuestion");

            if (string.IsNullOrEmpty(this.SecurityAnswer) || this.SecurityAnswer.Length < 3)
                yield return new RuleViolation("Security answers must be at least three characters.", "SecurityAnswer");
            
            if (!string.IsNullOrEmpty(this.SecurityAnswer) && this.SecurityAnswer.Length > 50)
                yield return new RuleViolation("Security answers must be less than fifty characters.", "SecurityAnswer");
            
            if (!this.Gender.Equals('M') && !this.Gender.Equals('F'))
                yield return new RuleViolation("Genders must be 'M' or 'F'.", "Gender");
            
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
