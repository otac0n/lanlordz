//-----------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="LAN Lordz, inc.">
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

    public class UserRepository
    {
        private readonly LanLordzDataContext db;
        private readonly ICacheManager dataCache;
        private readonly ConfigurationRepository config;

        public UserRepository(LanLordzDataContext db, ICacheManager dataCache, ConfigurationRepository config)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (dataCache == null)
            {
                throw new InvalidOperationException("dataCache");
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.db = db;
            this.dataCache = dataCache;
            this.config = config;
        }

        public UserInformation GetUserInformation(long userId)
        {
            return this.LoadUserInformation(userId);
        }

        public IEnumerable<UserInformation> GetUserInformation(IEnumerable<long> userIds)
        {
            List<UserInformation> userInfo = new List<UserInformation>();

            foreach (long userId in userIds)
            {
                userInfo.Add(this.LoadUserInformation(userId));
            }

            return userInfo.AsReadOnly();
        }

        public IQueryable<UserInformation> GetUserInformation(IQueryable<User> users)
        {
            return from u in users
                   let attrs = this.db.UserAttributes.Where(ua => ua.UserID == u.UserID)
                   select new UserInformation
                   {
                       UserID = u.UserID,
                       Username = u.Username,
                       Email = u.Email,
                       Gender = u.Gender,
                       ShowEmail = u.ShowEmail,
                       ShowGender = u.ShowGender,
                       CreateDate = u.CreateDate,
                       IsEmailConfirmed = u.IsEmailConfirmed,
                       ReceiveAdminEmail = u.ReceiveAdminEmail,
                       HasAvatar = this.db.UserAvatars.Where(ua => ua.UserID == u.UserID).Any(),
                       Posts = this.db.Posts.Where(p => p.UserID == u.UserID).Count(),
                       EventsRegistered = this.db.Registrations.Where(r => r.UserID == u.UserID).Count(),
                       EventsCheckedIn = this.db.Registrations.Where(r => r.UserID == u.UserID && r.IsCheckedIn).Count(),
                       Biography = attrs.Where(ua => ua.Attribute == "Biography").SingleOrDefault().Value,
                       Interests = attrs.Where(ua => ua.Attribute == "Interests").SingleOrDefault().Value,
                       Location = attrs.Where(ua => ua.Attribute == "Location").SingleOrDefault().Value,
                       Occupation = attrs.Where(ua => ua.Attribute == "Occupation").SingleOrDefault().Value,
                       Website = attrs.Where(ua => ua.Attribute == "Website").SingleOrDefault().Value,
                       Theme = attrs.Where(ua => ua.Attribute == "Theme").SingleOrDefault().Value,
                       TimeZone = attrs.Where(ua => ua.Attribute == "TimeZone").SingleOrDefault().Value,
                       Signature = attrs.Where(ua => ua.Attribute == "Signature").SingleOrDefault().Value,
                   };
        }

        public void InvalidateUserInformation(long userId)
        {
            string dataKey = "UserInformation." + userId;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public IQueryable<User> GetAllUsers()
        {
            return from u in this.db.Users
                   select u;
        }

        private UserInformation LoadUserInformation(long userId)
        {
            string dataKey = "UserInformation." + userId;

            lock (this.dataCache)
            {
                var value = (UserInformation)this.dataCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = this.GetUserInformation(from u in this.db.Users
                                                    where u.UserID == userId
                                                    select u).SingleOrDefault();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(2, 0, 0)), new SlidingTime(new TimeSpan(0, 40, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        public string RememberUser(User user)
        {
            if (user == null)
            {
                return null;
            }

            Guid keyId = Guid.NewGuid();

            AutoLogin al = new AutoLogin
            {
                UserID = user.UserID,
                Key = keyId,
                ExpirationDate = DateTime.UtcNow.AddDays(35)
            };

            this.db.AutoLogins.InsertOnSubmit(al);
            this.db.SubmitChanges();

            return keyId.ToString();
        }

        public void ForgetUser(string key)
        {
            Guid keyId;
            try
            {
                keyId = new Guid(key);
            }
            catch
            {
                return;
            }

            this.db.AutoLogins.DeleteAllOnSubmit(from a in this.db.AutoLogins
                                                 where a.Key == keyId
                                                 select a);
            this.db.SubmitChanges();
        }
    }
}
