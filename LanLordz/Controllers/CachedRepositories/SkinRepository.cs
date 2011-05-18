//-----------------------------------------------------------------------
// <copyright file="SkinRepository.cs" company="LAN Lordz, inc.">
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
    using System.Xml;
    using LanLordz.Models;
    using LanLordz.SiteTools;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
    using BBCode;

    public class SkinRepository
    {
        private readonly string skinsDirectory;
        private readonly ICacheManager skinCache;
        private readonly ConfigurationRepository config;

        public SkinRepository(string skinsDirectory, ICacheManager skinCache, ConfigurationRepository config)
        {
            if (skinCache == null)
            {
                throw new ArgumentNullException("skinCache");
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.skinCache = skinCache;
            this.skinsDirectory = skinsDirectory;
            this.config = config;
        }

        public BBCodeInterpreter GetBBCodeInterpreter()
        {
            return this.LoadBBCodeInterpreter();
        }

        public void InvalidateBBCodeInterpreter()
        {
            string dataKey = "BBCode";

            lock (this.skinCache)
            {
                this.skinCache.Remove(dataKey);
            }
        }

        public Pager GetDefaultPager()
        {
            return this.LoadDefaultPager();
        }

        public void InvalidateDefaultPager()
        {
            string dataKey = "DefaultPager";

            lock (this.skinCache)
            {
                this.skinCache.Remove(dataKey);
            }
        }

        public Pager GetDefaultForumPager()
        {
            return this.LoadDefaultForumPager();
        }

        public void InvalidateDefaultForumPager()
        {
            string dataKey = "DefaultForumPager";

            lock (this.skinCache)
            {
                this.skinCache.Remove(dataKey);
            }
        }

        public Pager GetDefaultGalleryPager()
        {
            return this.LoadDefaultPager("Gallery");
        }

        public Pager GetDefaultImagePager()
        {
            return this.LoadDefaultPager("Image");
        }

        public Pager GetDefaultThreadPager()
        {
            return this.LoadDefaultPager("Thread");
        }

        public void InvalidateDefaultThreadPager()
        {
            string dataKey = "DefaultThreadPager";

            lock (this.skinCache)
            {
                this.skinCache.Remove(dataKey);
            }
        }

        public IEnumerable<string> GetSkinsList()
        {
            string dataKey = "Skins";

            lock (this.skinCache)
            {
                var value = (IEnumerable<string>)this.skinCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = this.LoadSkinsList();

                    this.skinCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(0, 30, 0)), new SlidingTime(new TimeSpan(0, 10, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        public SiteTheme GetSkin(string skinName)
        {
            string dataKey = "Skins." + skinName;

            lock (this.skinCache)
            {
                var skins = this.GetSkinsList();

                if (string.IsNullOrEmpty(skinName) || !skins.Contains(skinName))
                {
                    skinName = skins.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(skinName))
                {
                    return null;
                }

                var value = (SiteTheme)this.skinCache[skinName];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = this.LoadSkin(skinName);

                    this.skinCache.Add(skinName, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(0, 30, 0)), new SlidingTime(new TimeSpan(0, 10, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private SiteTheme LoadSkin(string skinName)
        {
            string skinDirectory = Path.Combine(this.skinsDirectory, skinName);

            List<string> stylesheets = new List<string>();
            foreach (string path in Directory.GetFiles(skinDirectory, "*.css"))
            {
                stylesheets.Add("~/Skins/" + skinName + "/" + Path.GetFileName(path));
            }

            List<string> scripts = new List<string>();
            foreach (string path in Directory.GetFiles(skinDirectory, "*.js"))
            {
                scripts.Add("~/Skins/" + skinName + "/" + Path.GetFileName(path));
            }

            string selector = string.Empty;
            if (System.IO.File.Exists(Path.Combine(skinDirectory, "PngFixSelector.txt")))
            {
                selector = System.IO.File.ReadAllText(Path.Combine(skinDirectory, "PngFixSelector.txt"));
            }

            string banner = string.Empty;
            if (System.IO.File.Exists(Path.Combine(skinDirectory, "Banner.txt")))
            {
                banner = "~/Skins/" + skinName + "/" + System.IO.File.ReadAllText(Path.Combine(skinDirectory, "Banner.txt"));
            }

            return new SiteTheme
            {
                Name = skinName,
                PngFixSelector = selector,
                Banner = banner,
                Scripts = scripts,
                Stylesheets = stylesheets
            };
        }

        private BBCodeInterpreter LoadBBCodeInterpreter()
        {
            string dataKey = "BBCode";

            lock (this.skinCache)
            {
                var value = (BBCodeInterpreter)this.skinCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    XmlDocument doc = new XmlDocument();
                    using (var reader = new StreamReader(Path.Combine(this.skinsDirectory, "BBCode.xml")))
                    {
                        doc.Load(reader);
                    }

                    value = new BBCodeInterpreter(doc);

                    this.skinCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(3, 0, 0)), new SlidingTime(new TimeSpan(1, 0, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private Pager LoadDefaultPager()
        {
            string dataKey = "DefaultPager";

            lock (this.skinCache)
            {
                var value = (Pager)this.skinCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    XmlDocument doc = new XmlDocument();
                    using (var reader = new StreamReader(Path.Combine(this.skinsDirectory, "DefaultPager.xml")))
                    {
                        doc.Load(reader);
                    }

                    value = new Pager(doc);

                    this.skinCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private Pager LoadDefaultForumPager()
        {
            string dataKey = "DefaultForumPager";

            lock (this.skinCache)
            {
                var value = (Pager)this.skinCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    XmlDocument doc = new XmlDocument();
                    using (var reader = new StreamReader(Path.Combine(this.skinsDirectory, "DefaultForumPager.xml")))
                    {
                        doc.Load(reader);
                    }

                    value = new Pager(doc);

                    this.skinCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private Pager LoadDefaultPager(string pagerName)
        {
            string dataKey = "Default" + pagerName + "Pager";

            lock (this.skinCache)
            {
                var value = (Pager)this.skinCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    XmlDocument doc = new XmlDocument();
                    using (var reader = new StreamReader(Path.Combine(this.skinsDirectory, "Default" + pagerName + "Pager.xml")))
                    {
                        doc.Load(reader);
                    }

                    value = new Pager(doc);

                    this.skinCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private IEnumerable<string> LoadSkinsList()
        {
            List<string> skins = new List<string>();

            if (Directory.Exists(this.skinsDirectory))
            {
                foreach (string dir in Directory.GetDirectories(this.skinsDirectory))
                {
                    skins.Add(Path.GetFileName(dir));
                }
            }

            return skins.AsReadOnly();
        }
    }
}
