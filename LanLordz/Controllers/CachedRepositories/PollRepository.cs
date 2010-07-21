//-----------------------------------------------------------------------
// <copyright file="PollRepository.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Controllers.CachedRepositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using LanLordz.Models;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

    public class PollRepository
    {
        private readonly LanLordzDataContext db;
        private readonly ICacheManager dataCache;
        private readonly ConfigurationRepository config;

        public PollRepository(LanLordzDataContext db, ICacheManager dataCache, ConfigurationRepository config)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (dataCache == null)
            {
                throw new ArgumentNullException("dataCache");
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.db = db;
            this.dataCache = dataCache;
            this.config = config;
        }

        public IEnumerable<CachedPoll> GetAllPolls()
        {
            return this.LoadPolls();
        }

        private IEnumerable<CachedPoll> LoadPolls()
        {
            string dataKey = "Polls";

            lock (this.dataCache)
            {
                var value = (IEnumerable<CachedPoll>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from p in this.db.Polls
                             select new CachedPoll(p)).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }
    }
}
