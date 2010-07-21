//-----------------------------------------------------------------------
// <copyright file="SecurityRepository.cs" company="LAN Lordz, inc.">
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

    public class SecurityRepository
    {
        private readonly LanLordzDataContext db;
        private readonly ICacheManager dataCache;
        private readonly ConfigurationRepository config;

        public SecurityRepository(LanLordzDataContext db, ICacheManager dataCache, ConfigurationRepository config)
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

        public bool IsUserInRole(User user, string roleName)
        {
            if (user == null)
            {
                return false;
            }

            var userRoles = this.LoadUserRoles(user.UserID);

            return userRoles.Where(r => r.Name == roleName).Any();
        }

        public bool IsUserInRole(User user, long roleId)
        {
            if (user == null)
            {
                return false;
            }

            var userRoles = this.LoadUserRoles(user.UserID);

            return userRoles.Where(ur => ur.RoleID == roleId).Any();
        }

        public bool IsUserAdministrator(User user)
        {
            if (user == null)
            {
                return false;
            }

            var userRoles = this.LoadUserRoles(user.UserID);

            return userRoles.Where(ur => ur.IsAdministrator).Any();
        }

        public IEnumerable<Role> LoadUserRoles(long userId)
        {
            string dataKey = "UserRoles." + userId;

            lock (this.dataCache)
            {
                var value = (IEnumerable<Role>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from ur in this.db.UsersRoles
                             where ur.UserID == userId
                             select ur.Role).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(3, 0, 0)), new SlidingTime(new TimeSpan(1, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        public IEnumerable<AccessControlItem> LoadForumACL(long forumId, string accessType)
        {
            string dataKey = "ForumsAccesses." + forumId + "-" + accessType;

            lock (this.dataCache)
            {
                var value = (IEnumerable<AccessControlItem>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from forumAccess in this.db.ForumsAccesses
                             join aci in this.db.AccessControlItems on forumAccess.AccessControlListID equals aci.AccessControlListID
                             where forumAccess.ForumID == forumId && forumAccess.AccessType.Equals(accessType)
                             orderby aci.Order
                             select aci).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(3, 0, 0)), new SlidingTime(new TimeSpan(1, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        public void InvalidateForumACL(long forumId, string accessType)
        {
            string dataKey = "ForumsAccesses." + forumId + "-" + accessType;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public IEnumerable<AccessControlItem> LoadForumGroupACL(long forumGroupId, string accessType)
        {
            string dataKey = "ForumsGroupAccesses." + forumGroupId + "-" + accessType;

            lock (this.dataCache)
            {
                var value = (IEnumerable<AccessControlItem>)this.dataCache[dataKey];

                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from forumGroupAccess in this.db.ForumGroupsAccesses
                             join aci in this.db.AccessControlItems on forumGroupAccess.AccessControlListID equals aci.AccessControlListID
                             where forumGroupAccess.ForumGroupID == forumGroupId && forumGroupAccess.AccessType.Equals(accessType)
                             orderby aci.Order
                             select aci).ToList();

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

        public void InvalidateForumGroupACL(long forumGroupId, string accessType)
        {
            string dataKey = "ForumsGroupAccesses." + forumGroupId + "-" + accessType;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public bool GetUserForumGroupAccess(User user, ForumGroup forumGroup)
        {
            var acl = this.LoadForumGroupACL(forumGroup.ForumGroupID, "VIEW");

            return this.GetUserAccess(user, acl);
        }

        public ForumAccess GetUserForumAccess(User user, Forum forum)
        {
            bool grup = this.GetUserForumGroupAccess(user, forum.ForumGroup);
            bool view = grup && this.GetUserForumAccess(user, forum, "VIEW");
            bool read = view && this.GetUserForumAccess(user, forum, "READ");
            bool post = read && this.GetUserForumAccess(user, forum, "POST");
            bool modr = post && this.GetUserForumAccess(user, forum, "MODR");

            return new ForumAccess(view, read, post, modr);
        }

        public bool GetUserAccess(User user, IEnumerable<AccessControlItem> acl)
        {
            // Implicit * A 1 (Special, Administrator, Allow)
            if (user != null && this.IsUserAdministrator(user))
            {
                return true;
            }

            foreach (AccessControlItem item in acl)
            {
                switch (item.Type)
                {
                    // Special Codes
                    case '*':
                        switch (item.For)
                        {
                            // Administrators
                            case "A":
                                if (user != null && this.IsUserAdministrator(user))
                                {
                                    return item.Allow;
                                }

                                break;

                            // Authenticated Users ("Members")
                            case "*":
                                if (user != null)
                                {
                                    return item.Allow;
                                }

                                break;

                            // Anyone
                            case "?":
                                return item.Allow;
                        }

                        break;
                    case 'G':
                        if (user != null && this.IsUserInRole(user, item.For))
                        {
                            return item.Allow;
                        }

                        break;
                    case 'U':
                        if (user != null && user.Username.Equals(item.For, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return item.Allow;
                        }

                        break;
                }
            }

            // Implicit * ? 0 (Special, Any User, Deny)
            return false;
        }

        private bool GetUserForumAccess(User user, Forum forum, string accessType)
        {
            var acl = this.LoadForumACL(forum.ForumID, accessType);

            return this.GetUserAccess(user, acl);
        }
    }
}
