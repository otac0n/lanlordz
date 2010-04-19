//-----------------------------------------------------------------------
// <copyright file="ConfigurationRepository.cs" company="LAN Lordz, inc.">
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
    using System.Globalization;
    using System.Linq;
    using LanLordz.Models;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

    public class ConfigurationRepository : IDisposable
    {
        private const string ConfigKey = "Configuration";
        private readonly ICacheManager dataCache;

        private LanLordzDataContext db;

        private Dictionary<string, Configuration> config;
        private bool isFromCache = true;
        private bool isInCache;

        private TimeZoneInfo defaultTimeZoneInfo;

        public ConfigurationRepository(LanLordzDataContext db, ICacheManager dataCache)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            this.dataCache = dataCache;
            this.db = db;

            this.LoadConfiguration();
        }

        public string SiteName
        {
            get
            {
                string res = this.GetConfigProperty("SiteName");

                if (String.IsNullOrEmpty(res))
                {
                    res = "YOUR SITE NAME";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("SiteName", value);
            }
        }

        public TimeZoneInfo DefaultTimeZoneInfo
        {
            get
            {
                if (this.defaultTimeZoneInfo == null)
                {
                    this.defaultTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.DefaultTimeZone);
                }

                return this.defaultTimeZoneInfo;
            }
        }

        public string DefaultTimeZone
        {
            get
            {
                return this.GetConfigProperty("DefaultTimeZone");
            }

            set
            {
                this.SetConfigProperty("DefaultTimeZone", value);
                this.defaultTimeZoneInfo = null;
            }
        }

        public string SiteTagline
        {
            get
            {
                return this.GetConfigProperty("SiteTagline");
            }

            set
            {
                this.SetConfigProperty("SiteTagline", value);
            }
        }

        public string SiteCopyright
        {
            get
            {
                string res = this.GetConfigProperty("SiteCopyright");

                if (String.IsNullOrEmpty(res))
                {
                    res = "© " + DateTime.Now.Year.ToString() + " " + this.SiteName;
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("SiteCopyright", value);
            }
        }

        public string DefaultTheme
        {
            get
            {
                return this.GetConfigProperty("DefaultTheme");
            }

            set
            {
                this.SetConfigProperty("DefaultTheme", value);
            }
        }

        public bool UseGoogleAnalytics
        {
            get
            {
                bool res = false;

                return bool.TryParse(this.GetConfigProperty("UseGoogleAnalytics"), out res) ? res : false;
            }

            set
            {
                this.SetConfigProperty("UseGoogleAnalytics", value.ToString());
            }
        }

        public bool UseRecaptcha
        {
            get
            {
                bool res = false;

                return bool.TryParse(this.GetConfigProperty("UseRecaptcha"), out res) ? res : false;
            }

            set
            {
                this.SetConfigProperty("UseRecaptcha", value.ToString());
            }
        }

        public bool UseCoralCdn
        {
            get
            {
                bool res = false;

                return bool.TryParse(this.GetConfigProperty("UseCoralCDN"), out res) ? res : false;
            }

            set
            {
                this.SetConfigProperty("UseCoralCDN", value.ToString());
            }
        }

        public bool ConfirmEmail
        {
            get
            {
                bool res = false;

                return bool.TryParse(this.GetConfigProperty("ConfirmEmail"), out res) ? res : false;
            }

            set
            {
                this.SetConfigProperty("ConfirmEmail", value.ToString());
            }
        }

        public bool ShowVisitors
        {
            get
            {
                bool res = true;

                return bool.TryParse(this.GetConfigProperty("ShowVisitors"), out res) ? res : true;
            }

            set
            {
                this.SetConfigProperty("ShowVisitors", value.ToString());
            }
        }

        public string AdminEmail
        {
            get
            {
                return this.GetConfigProperty("AdminEmail");
            }

            set
            {
                this.SetConfigProperty("AdminEmail", value);
            }
        }

        public string ConfirmEmailText
        {
            get
            {
                string res = this.GetConfigProperty("ConfirmEmailText");

                if (String.IsNullOrEmpty(res))
                {
                    res = "Please visit <a href=\"{0}\">{0}</a> to confirm your email address.";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("ConfirmEmailText", value);
            }
        }

        public string ConfirmEmailSubject
        {
            get
            {
                string res = this.GetConfigProperty("ConfirmEmailSubject");

                if (String.IsNullOrEmpty(res))
                {
                    res = "Confirm your email address.";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("ConfirmEmailSubject", value);
            }
        }

        public string SecurityEmailText
        {
            get
            {
                string res = this.GetConfigProperty("SecurityEmailText");

                if (String.IsNullOrEmpty(res))
                {
                    res = "You or someone else has requested that your security details be changed.  If you requested this, use the following key to change your password or security questions.  If you did not request this, simply delete this email and no action will be taken.<br /><br />Key: {0}";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("SecurityEmailText", value);
            }
        }

        public string SecurityEmailSubject
        {
            get
            {
                string res = this.GetConfigProperty("SecurityEmailSubject");

                if (String.IsNullOrEmpty(res))
                {
                    res = "Your security change key has been created.";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("SecurityEmailSubject", value);
            }
        }

        public string SmtpHost
        {
            get
            {
                string res = this.GetConfigProperty("SmtpHost");

                if (String.IsNullOrEmpty(res))
                {
                    res = Environment.MachineName;
                }

                if (String.IsNullOrEmpty(res))
                {
                    res = "localhost";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("SmtpHost", value);
            }
        }

        public int SmtpPort
        {
            get
            {
                int res = 25;

                return int.TryParse(this.GetConfigProperty("SmtpPort"), NumberStyles.Integer, CultureInfo.InvariantCulture, out res) ? res : 25;
            }

            set
            {
                this.SetConfigProperty("SmtpPort", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool LoginAfterRegister
        {
            get
            {
                bool res = true;

                return bool.TryParse(this.GetConfigProperty("LoginAfterRegister"), out res) ? res : true;
            }

            set
            {
                this.SetConfigProperty("LoginAfterRegister", value.ToString());
            }
        }

        public string MainPageHtml
        {
            get
            {
                string res = this.GetConfigProperty("MainPageHtml");

                if (string.IsNullOrEmpty(res))
                {
                    res = "You can put <i>Any</i> text here you would like. (HTML OK!)";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("MainPageHtml", value);
            }
        }

        public string GoogleMapsKey
        {
            get
            {
                return this.GetConfigProperty("GoogleMapsKey");
            }

            set
            {
                this.SetConfigProperty("GoogleMapsKey", value);
            }
        }

        public string GoogleAnalyticsAccount
        {
            get
            {
                string res = this.GetConfigProperty("GoogleAnalyticsAccount");

                if (string.IsNullOrEmpty(res))
                {
                    res = "UA-0000000-0";
                }

                return res;
            }

            set
            {
                this.SetConfigProperty("GoogleAnalyticsAccount", value);
            }
        }

        public string RecaptchaPublicKey
        {
            get
            {
                return this.GetConfigProperty("RecaptchaPublicKey");
            }

            set
            {
                this.SetConfigProperty("RecaptchaPublicKey", value);
            }
        }

        public string RecaptchaPrivateKey
        {
            get
            {
                return this.GetConfigProperty("RecaptchaPrivateKey");
            }

            set
            {
                this.SetConfigProperty("RecaptchaPrivateKey", value);
            }
        }

        public long NewsThread
        {
            get
            {
                long res = 0;

                return long.TryParse(this.GetConfigProperty("NewsThread"), NumberStyles.Integer, CultureInfo.InvariantCulture, out res) ? res : 0;
            }

            set
            {
                this.SetConfigProperty("NewsThread", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long CrewGroup
        {
            get
            {
                long res = 0;

                return long.TryParse(this.GetConfigProperty("CrewGroup"), NumberStyles.Integer, CultureInfo.InvariantCulture, out res) ? res : 1;
            }

            set
            {
                this.SetConfigProperty("CrewGroup", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long Visitors
        {
            get
            {
                long res = 0;

                return long.TryParse(this.GetConfigProperty("Visitors"), NumberStyles.Integer, CultureInfo.InvariantCulture, out res) ? res : 0;
            }

            set
            {
                this.SetConfigProperty("Visitors", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void Freshen()
        {
            this.LoadPersistantConfiguration();
        }

        public void Invalidate()
        {
            lock (this.dataCache)
            {
                this.dataCache.Remove(ConfigKey);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.config != null)
                {
                    this.config = null;
                }

                if (this.db != null)
                {
                    this.db = null;
                }
            }
        }

        private void LoadConfiguration()
        {
            if (this.config == null)
            {
                var value = this.LoadFromCache();

                if (value != null)
                {
                    Trace.WriteLine("Cache Hit: " + ConfigKey);

                    this.isFromCache = true;
                    this.config = value;
                }
                else
                {
                    Trace.WriteLine("Cache Miss: " + ConfigKey);

                    value = this.LoadFromDB();

                    this.isFromCache = false;
                    this.config = value;

                    this.isInCache = true;
                    this.dataCache.Add(ConfigKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                }
            }
        }

        private void LoadPersistantConfiguration()
        {
            if (this.isFromCache)
            {
                this.isFromCache = false;
                this.config = this.LoadFromDB();
            }
            else if (this.isInCache)
            {
                Trace.WriteLine("Cache Invalid: " + ConfigKey);

                lock (this.dataCache)
                {
                    this.dataCache.Remove(ConfigKey);
                }

                this.isInCache = false;
            }
        }

        private Dictionary<string, Configuration> LoadFromCache()
        {
            if (this.dataCache != null)
            {
                lock (this.dataCache)
                {
                    return (Dictionary<string, Configuration>)this.dataCache[ConfigKey];
                }
            }
            else
            {
                return null;
            }
        }

        private Dictionary<string, Configuration> LoadFromDB()
        {
            return (from c in this.db.Configurations
                    select c).ToDictionary(k => k.Property.ToUpperInvariant());
        }

        private string GetConfigProperty(string property)
        {
            this.LoadConfiguration();

            var propertyKey = property.ToUpperInvariant();

            if (!this.config.ContainsKey(propertyKey))
            {
                return string.Empty;
            }
            else
            {
                return this.config[propertyKey].Value ?? string.Empty;
            }
        }

        private void SetConfigProperty(string property, string value)
        {
            this.LoadPersistantConfiguration();

            var propertyKey = property.ToUpperInvariant();

            if (!this.config.ContainsKey(propertyKey))
            {
                var conf = new Configuration()
                {
                    Property = property,
                    Value = value
                };

                this.db.Configurations.InsertOnSubmit(conf);

                this.config.Add(propertyKey, conf);
            }
            else
            {
                this.config[propertyKey].Value = value;
            }
        }
    }
}
