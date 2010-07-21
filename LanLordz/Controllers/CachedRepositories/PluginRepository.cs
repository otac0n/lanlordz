//-----------------------------------------------------------------------
// <copyright file="PluginRepository.cs" company="LAN Lordz, inc.">
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
    using System.IO;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
    using Tournaments;
    using Tournaments.Plugins;

    public class PluginRepository
    {
        private readonly ICacheManager pluginCache;
        private readonly string pluginsDirectory;
        private readonly ConfigurationRepository config;

        public PluginRepository(string pluginsDirectory, ICacheManager pluginCache, ConfigurationRepository config)
        {
            if (pluginCache == null)
            {
                throw new ArgumentNullException("pluginCache");
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.pluginCache = pluginCache;
            this.pluginsDirectory = pluginsDirectory;
            this.config = config;
        }

        public IEnumerable<IPluginFactory> LoadPluginFactories()
        {
            lock (this.pluginCache)
            {
                var value = (IEnumerable<IPluginFactory>)this.pluginCache[this.pluginsDirectory];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: PluginFactories");
                    value = LoadPlugins(this.pluginsDirectory);
                    this.pluginCache.Add(this.pluginsDirectory, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(1, 0, 0)), new SlidingTime(new TimeSpan(0, 20, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: PluginFactories");
                }

                return (IEnumerable<IPluginFactory>)value;
            }
        }

        public IPairingsGeneratorFactory GetPairingsGeneratorFactory(string pluginName)
        {
            var allPlugins = this.LoadPluginFactories();

            var pg = from p in allPlugins
                     where p is IPairingsGeneratorFactory
                     where p.Name == pluginName
                     select p;

            return (IPairingsGeneratorFactory)pg.SingleOrDefault();
        }

        public IPairingsGenerator GetPairingsGenerator(string pluginName)
        {
            var pgf = this.GetPairingsGeneratorFactory(pluginName);

            if (pgf == null)
            {
                return null;
            }
            else
            {
                return pgf.Create();
            }
        }

        internal IList<string> GetPairingsGeneratorList()
        {
            List<string> pairingsGenerators = new List<string>();

            var apf = this.LoadPluginFactories();

            foreach (var p in apf.Where(pf => pf is IPairingsGeneratorFactory))
            {
                pairingsGenerators.Add(p.Name);
            }

            return pairingsGenerators;
        }

        private static IEnumerable<IPluginFactory> LoadPlugins(string pluginsDirectory)
        {
            List<IPluginFactory> factories = new List<IPluginFactory>();

            if (Directory.Exists(pluginsDirectory))
            {
                foreach (string path in Directory.GetFiles(pluginsDirectory, "*.dll"))
                {
                    try
                    {
                        var newFactories = PluginLoader.LoadPlugins(path);

                        if (newFactories != null)
                        {
                            factories.AddRange(newFactories);
                        }
                    }
                    catch (LoadPluginsFailureException ex)
                    {
                        Debug.WriteLine(ex);
                        Debug.WriteLineIf(ex.InnerException != null, ex.InnerException);
                        continue;
                    }
                }
            }

            return factories.AsReadOnly();
        }
    }
}
