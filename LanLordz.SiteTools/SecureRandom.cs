//-----------------------------------------------------------------------
// <copyright file="SecureRandom.cs" company="LAN Lordz, inc.">
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
    using System;
    using System.Security.Cryptography;

    public class SecureRandom
    {
        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public int Next()
        {
            return (int)(this.NextUlong() % int.MaxValue);
        }

        public int Next(int maxValue)
        {
            if (maxValue <= 0)
            {
                throw new ArgumentOutOfRangeException("maxValue");
            }

            var chop = ulong.MaxValue - (ulong.MaxValue % (ulong)maxValue);

            ulong rand;

            do
            {
                rand = this.NextUlong();
            }
            while (rand >= chop);

            return (int)(rand % (ulong)maxValue);
        }

        private ulong NextUlong()
        {
            var data = new byte[8];
            this.rng.GetBytes(data);
            return BitConverter.ToUInt64(data, 0);
        }
    }
}