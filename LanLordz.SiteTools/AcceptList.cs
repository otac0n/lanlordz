//-----------------------------------------------------------------------
// <copyright file="AcceptList.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.SiteTools
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AcceptList : IEnumerable<string>
    {
        private readonly Regex parser = new Regex(@"(?<name>[^;,\r\n]+)(?:;q=(?<value>[\d.]+))?", RegexOptions.Compiled);
        private readonly IEnumerable<string> encodings;

        public AcceptList(string acceptHeaderValue, IEnumerable<string> supportedEncodings)
        {
            var accepts = new List<KeyValuePair<string, float>>();

            if (!string.IsNullOrEmpty(acceptHeaderValue))
            {
                var matches = this.parser.Matches(acceptHeaderValue);

                var values = from Match v in matches
                             where v.Success
                             select new
                             {
                                 Name = v.Groups["name"].Value,
                                 Value = v.Groups["value"].Value
                             };

                foreach (var value in values)
                {
                    if (value.Name == "*")
                    {
                        foreach (var encoding in supportedEncodings)
                        {
                            if (!accepts.Where(a => a.Key.ToUpperInvariant() == encoding.ToUpperInvariant()).Any())
                            {
                                accepts.Add(new KeyValuePair<string, float>(encoding, 1.0f));
                            }
                        }

                        continue;
                    }

                    var desired = 1.0f;
                    if (!string.IsNullOrEmpty(value.Value))
                    {
                        float.TryParse(value.Value, out desired);
                    }

                    if (desired == 0.0f)
                    {
                        continue;
                    }

                    accepts.Add(new KeyValuePair<string, float>(value.Name, desired));
                }
            }

            this.encodings = from a in accepts
                             where supportedEncodings.Where(se => se.ToUpperInvariant() == a.Key.ToUpperInvariant()).Any() || a.Key.ToUpperInvariant() == "IDENTITY"
                             orderby a.Value descending
                             select a.Key;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return this.encodings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.encodings).GetEnumerator();
        }
    }
}
