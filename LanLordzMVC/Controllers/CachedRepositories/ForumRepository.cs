//-----------------------------------------------------------------------
// <copyright file="ForumRepository.cs" company="LAN Lordz, inc.">
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
    using System.Linq;
    using LanLordz.Models;
    using Microsoft.Practices.EnterpriseLibrary.Caching;

    public class ForumRepository
    {
        private readonly LanLordzDataContext db;
        private readonly SecurityRepository security;
        private ICacheManager dataCache;

        public ForumRepository(LanLordzDataContext db, ICacheManager dataCache, SecurityRepository security)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (dataCache == null)
            {
                throw new ArgumentNullException("dataCache");
            }

            if (security == null)
            {
                throw new ArgumentNullException("security");
            }

            this.db = db;
            this.dataCache = dataCache;
            this.security = security;
        }

        public IEnumerable<ForumGroup> GetViewableForumGroups(User user)
        {
            List<ForumGroup> groups = new List<ForumGroup>();

            foreach (ForumGroup forumGroup in this.db.ForumGroups)
            {
                if (this.security.GetUserForumGroupAccess(user, forumGroup))
                {
                    groups.Add(forumGroup);
                }
            }

            return groups;
        }

        public IEnumerable<ForumInformation> GetViewableForums(User user, IEnumerable<ForumGroup> groups)
        {
            long userId = 0;
            if (user != null)
            {
                userId = user.UserID;
            }

            List<ForumInformation> info = new List<ForumInformation>();

            List<long> groupIds = new List<long>(from fg in groups
                                                 select fg.ForumGroupID);

            var forums = from f in this.db.Forums
                         let lastPost = (from p in this.db.Posts
                                         where p.Thread.ForumID == f.ForumID
                                         where !p.IsDeleted
                                         select p).OrderByDescending(p => p.CreateDate).FirstOrDefault()
                         let lastPostUser = lastPost == null ? null : lastPost.User
                         where groupIds.Contains(f.ForumGroupID)
                         select new ForumInformation
                         {
                             Forum = f,
                             Threads = (from t in this.db.Threads
                                        where t.IsDeleted == false
                                        where t.ForumID == f.ForumID
                                        select t.ThreadID).Count(),
                             Posts = (from t in this.db.Threads
                                      where t.IsDeleted == false
                                      join p in this.db.Posts
                                      on t.ThreadID equals p.ThreadID
                                      where p.IsDeleted == false
                                      where t.ForumID == f.ForumID
                                      select p.ThreadID).Count(),
                             LastPostDate = lastPost == null ? null : (DateTime?)lastPost.CreateDate,
                             LastPostUserID = lastPost == null ? null : (long?)lastPostUser.UserID,
                             LastPostUsername = lastPost == null ? null : lastPostUser.Username,
                             ThreadsRead = user == null ? f.Threads.Where(t => !t.IsDeleted).Count() : (from t in this.db.Threads
                                                                                                        where t.IsDeleted == false
                                                                                                        join r in this.db.ThreadReads on t.ThreadID equals r.ThreadID
                                                                                                        where t.ForumID == f.ForumID && r.UserID == userId && r.DateRead > t.Posts.Where(post => post.IsDeleted == false).Max(p => p.CreateDate)
                                                                                                        select r).Count()
                         };

            List<ForumInformation> allForums = new List<ForumInformation>(forums);

            foreach (ForumInformation forum in allForums)
            {
                if (this.security.GetUserForumAccess(user, forum.Forum).CanView)
                {
                    info.Add(forum);
                }
            }

            return info;
        }

        public Forum GetForum(long forumId)
        {
            return this.db.Forums.SingleOrDefault(f => f.ForumID == forumId);
        }

        public Thread GetThread(long threadId)
        {
            return this.db.Threads.SingleOrDefault(t => t.ThreadID == threadId);
        }

        public Post GetPost(long postId)
        {
            return this.db.Posts.SingleOrDefault(p => p.PostID == postId);
        }

        public IQueryable<Thread> GetForumViewableThreads(long forumId)
        {
            return from t in this.db.Threads
                   where t.ForumID == forumId
                   where !t.IsDeleted
                   select t;
        }
    }
}
