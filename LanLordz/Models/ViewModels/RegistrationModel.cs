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
using System.Text.RegularExpressions;
using LanLordz.Controllers.CachedRepositories;
using LanLordz.Controllers;

namespace LanLordz.Models
{
    public class RegistrationModel : ControllerResponse
    {
        public RegistrationModel(LanLordzBaseController controller)
            : base(controller)
        {
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string Email { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public char Gender { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowGender { get; set; }

        Regex validEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        Regex whiteSpace = new Regex(@"\s");

        public bool IsValid
        {
            get
            {
                return (this.GetRuleViolations().Count() == 0);
            }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.Username))
                yield return new RuleViolation("Your username must not be blank.", "Username");

            if (!string.IsNullOrEmpty(this.Username) && whiteSpace.IsMatch(this.Username))
                yield return new RuleViolation("Your username may not contain any whitespace.", "Username");

            if (!string.IsNullOrEmpty(this.Username) && (this.Username.Contains("]") || this.Username.Contains("[")))
                yield return new RuleViolation("Your username may not contain square brackets. Brackets will interfere with BBCode.", "Username");

            if (string.IsNullOrEmpty(this.Password))
                yield return new RuleViolation("A password must be specified.", "Password");

            if (!string.Equals(this.Password, this.PasswordConfirm))
                yield return new RuleViolation("The passwords you entered do not match.", "Password");

            if (string.IsNullOrEmpty(this.Email) || !validEmail.IsMatch(this.Email))
                yield return new RuleViolation("The email you entered is invalid.", "Email");

            if (string.IsNullOrEmpty(this.SecurityQuestion) || this.SecurityQuestion.Length < 3)
                yield return new RuleViolation("Your security question must be at least three characters.", "SecurityQuestion");

            if (string.IsNullOrEmpty(this.SecurityAnswer) || this.SecurityAnswer.Length < 3)
                yield return new RuleViolation("Your security answer must be at least three characters.", "SecurityAnswer");

            if (!this.Gender.Equals('M') && !this.Gender.Equals('F'))
                yield return new RuleViolation("You must specify a gender.", "Gender");

            yield break;
        }

    }
}
