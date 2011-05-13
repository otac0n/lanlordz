//-----------------------------------------------------------------------
// <copyright file="ForumsController.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class ForumsController : LanLordzBaseController
    {
        #region Information Attach Methods
        private IQueryable<ThreadInformation> GetThreadInformation(IQueryable<Thread> threads, User user)
        {
            var userposts = from p in this.Db.Posts
                            join u in this.Db.Users on p.UserID equals u.UserID
                            select new
                            {
                                p.PostID,
                                p.UserID,
                                p.CreateDate,
                                u.Username
                            };


            var threadposts = from t in this.Db.Threads
                              select new
                              {
                                  t.ThreadID,
                                  FirstPostID = (from p in t.Posts
                                                 where p.IsDeleted == false
                                                 orderby p.CreateDate
                                                 select (long?)p.PostID).FirstOrDefault(),
                                  LastPostID = (from p in t.Posts
                                                where p.IsDeleted == false
                                                orderby p.CreateDate descending
                                                select (long?)p.PostID).FirstOrDefault(),
                              };

            var results = from t in threads
                          join tp in threadposts on t.ThreadID equals tp.ThreadID
                          join fp in userposts on tp.FirstPostID equals fp.PostID into joinedFirstPost
                          join lp in userposts on tp.LastPostID equals lp.PostID into joinedLastPost
                          from firstPost in joinedFirstPost.DefaultIfEmpty()
                          from lastPost in joinedLastPost.DefaultIfEmpty()
                          select new
                          {
                              Thread = t,
                              Level = t.ThreadLevel.Name,
                              PostCount = t.Posts.Where(p => p.IsDeleted == false).Count(),
                              FirstPost = firstPost,
                              LastPost = lastPost,
                          };

            if (user == null)
            {
                return from r in results
                       select new ThreadInformation
                       {
                           Thread = r.Thread,
                           Level = r.Level,
                           Posts = r.PostCount,
                           Read = true,
                           ReadCount = 0,
                           FirstUnreadID = r.FirstPost == null ? (long?)null : r.FirstPost.UserID,
                           FirstPostUserID = r.FirstPost == null ? (long?)null : r.FirstPost.UserID,
                           FirstPostUsername = r.FirstPost == null ? null : r.FirstPost.Username,
                           LastPostUserID = r.LastPost == null ? (long?)null : r.LastPost.UserID,
                           LastPostUsername = r.LastPost == null ? null : r.LastPost.Username,
                           LastPostDate = r.LastPost == null ? r.Thread.CreateDate : r.LastPost.CreateDate,
                       };
            }
            else
            {
                return from r in results
                       let dateRead = (from tr in r.Thread.ThreadReads
                                       where tr.UserID == user.UserID
                                       select (DateTime?)tr.DateRead).SingleOrDefault()
                       let readPosts = from p in r.Thread.Posts
                                       where p.IsDeleted == false
                                       where dateRead.HasValue && dateRead > p.CreateDate
                                       select p
                       let unreadPosts = from p in r.Thread.Posts
                                         where p.IsDeleted == false
                                         where !dateRead.HasValue || dateRead <= p.CreateDate
                                         select p
                       select new ThreadInformation
                       {
                           Thread = r.Thread,
                           Level = r.Level,
                           Posts = r.PostCount,
                           Read = !unreadPosts.Any(),
                           ReadCount = readPosts.Count(),
                           FirstUnreadID = (from p in unreadPosts
                                            where p.IsDeleted == false
                                            orderby p.CreateDate descending
                                            select (long?)p.PostID).FirstOrDefault(),
                           FirstPostUserID = r.FirstPost == null ? (long?)null : r.FirstPost.UserID,
                           FirstPostUsername = r.FirstPost == null ? null : r.FirstPost.Username,
                           LastPostUserID = r.LastPost == null ? (long?)null : r.LastPost.UserID,
                           LastPostUsername = r.LastPost == null ? null : r.LastPost.Username,
                           LastPostDate = r.LastPost == null ? r.Thread.CreateDate : r.LastPost.CreateDate,
                       };
            }
        }

        private ThreadInformation GetThreadInformation(Thread thread, User user)
        {
            long userId = 0;
            if (user != null)
            {
                userId = user.UserID;
            }

            var r = from p in this.Db.Posts
                    where p.IsDeleted == false
                    group p by p.ThreadID into g
                    where g.Key == thread.ThreadID
                    let firstPostUser = g.OrderBy(p => p.CreateDate).FirstOrDefault().User
                    let lastPost = g.OrderByDescending(p => p.CreateDate).FirstOrDefault()
                    let lastPostUser = lastPost.User
                    select new ThreadInformation
                    {
                        Thread = thread,
                        Read = userId == 0 ? true : this.Db.ThreadReads.Where(thr => thr.ThreadID == thread.ThreadID && thr.UserID == userId).Any(),
                        Level = thread.ThreadLevel.Name,
                        Posts = g.Count(),
                        FirstPostUserID = firstPostUser.UserID,
                        FirstPostUsername = firstPostUser.Username,
                        LastPostUserID = lastPostUser.UserID,
                        LastPostUsername = lastPostUser.Username,
                        LastPostDate = lastPost.CreateDate
                    };

            return r.SingleOrDefault();
        }

        private IEnumerable<PostInformation> GetPostsInformation(Thread t)
        {
            return (from p in this.Db.Posts
                    where !p.IsDeleted
                    where p.ThreadID == t.ThreadID
                    orderby p.CreateDate
                    let author = p.User
                    let authorPostCount = this.Db.Posts.Where(pc => pc.UserID == p.UserID).Count()
                    let userTitle = this.Db.Titles.Where(ti => ti.UserID.HasValue && ti.UserID == p.UserID).FirstOrDefault()
                    let groupTitle = this.Db.Titles.Where(ti => ti.RoleID.HasValue && this.Db.UsersRoles.Where(ur => ur.RoleID == ti.RoleID && ur.UserID == p.UserID).Any()).FirstOrDefault()
                    let postsTitle = this.Db.Titles.Where(ti => ti.PostCountThreshold.HasValue && authorPostCount >= ti.PostCountThreshold).OrderByDescending(ti => ti.PostCountThreshold).FirstOrDefault()
                    select new PostInformation
                    {
                        Post = p,
                        UserID = author.UserID,
                        Username = author.Username,
                        UserEmail = author.Email,
                        UserPosts = authorPostCount,
                        UserTitle = userTitle != null ? userTitle.TitleText : null,
                        GroupTitle = groupTitle != null ? groupTitle.TitleText : null,
                        PostCountTitle = postsTitle != null ? postsTitle.TitleText : null,
                        UserJoinedDate = author.CreateDate,
                        UserSignature = this.Db.UserAttributes.Where(a => a.UserID == p.UserID && a.Attribute == "Signature").SingleOrDefault().Value,
                        UserHasAvatar = this.Db.UserAvatars.Where(ua => ua.UserID == author.UserID).Any()
                    }).ToList();
        }
        #endregion

        private void MarkForumThreadsRead(long forumId, long userId)
        {
            // TODO: Implement this function
            throw new NotImplementedException();
        }

        private IQueryable<Thread> GetAllUnreadThreads(long userId)
        {
            var threads = from t in this.Db.Threads
                          where !t.IsDeleted
                          where !this.Db.ThreadReads.Where(tr => tr.UserID == userId && tr.ThreadID == t.ThreadID && tr.DateRead > t.Posts.Where(p => p.IsDeleted == false).Max(p => p.CreateDate)).Any()
                          select t;

            return threads;
        }

        [CompressFilter]
        public ActionResult Index()
        {
            ForumsIndexModel fmi = new ForumsIndexModel(this);
            fmi.ForumGroups = this.Forums.GetViewableForumGroups(CurrentUser);
            fmi.Forums = this.Forums.GetViewableForums(CurrentUser, fmi.ForumGroups);

            return View("Index", fmi);
        }

        [CompressFilter]
        public ActionResult CreatePost(long? forum, long? thread, long? replyTo)
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            Forum f = null;
            Thread t = null;
            Post p = null;

            if (replyTo.HasValue)
            {
                p = this.Forums.GetPost(replyTo.Value);

                if (p == null  || p.IsDeleted)
                {
                    return View("NotFound");
                }

                t = p.Thread;
                f = t.Forum;
            }

            if (thread.HasValue)
            {
                if (f != null)
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "You may not reply to a thread and another post at the same time."
                    });
                }

                t = this.Forums.GetThread(thread.Value);

                if (t == null || t.IsDeleted)
                {
                    return View("NotFound");
                }

                f = t.Forum;
            }

            if (forum.HasValue)
            {
                if (f != null)
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "You may not create a new thread in reply to another item."
                    });
                }

                f = this.Forums.GetForum(forum.Value);

                if (f == null)
                {
                    return View("NotFound");
                }
            }

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && t != null && t.IsLocked))
            {
                return View("NotAuthorized");
            }

            string title = "";
            string text = "";

            if (p != null)
            {
                title = "Re: " + p.Title;
                text = "[quote=" + p.User.Username + "]" + p.Text + "[/quote]";
            }

            var cpm = new CreatePostModel(this)
            {
                Title = title,
                Text = text,
                Forum = f,
                Thread = t,
                Post = p,
                AvailableThreadLevels = this.Db.ThreadLevels.ToList(),
                UserAccess = access
            };
            return View("CreatePost", cpm);
        }

        [CompressFilter]
        public ActionResult ViewForum(long? id, int? page)
        {
            int pageSize = 25;

            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            Forum f = this.Forums.GetForum(id.Value);

            if (f == null)
            {
                return View("NotFound");
            }

            ForumAccess a = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!a.CanView)
            {
                return View("NotAuthorized");
            }

            var threadSource = this.Forums.GetForumViewableThreads(f.ForumID);

            var threadCount = threadSource.Count();
            var threads = GetThreadInformation(threadSource, CurrentUser);
            int pages = Pager.PageCount(threadCount, pageSize);
            page = Pager.ClampPage(page, pages);

            var threadsList = new ThreadList(this)
            {
                Threads = threads.OrderByDescending(t => t.LastPostDate).OrderByDescending(t => t.Thread.Level).Skip((page.Value - 1) * pageSize).Take(pageSize).ToList()
            };

            var fd = new ForumDetailsModel(this)
            {
                Forum = f,
                UserAccess = a,
                PageInfo = new PaginationInformation
                {
                    Pager = this.Skins.GetDefaultForumPager(),
                    ControllerName = "Forums",
                    ActionName = "ViewForum",
                    PageAttribute = "page",
                    RouteValues = new System.Web.Routing.RouteValueDictionary(new { id = id }),
                    ItemsPerPage = pageSize,
                    Items = threadCount,
                    CurrentPage = page
                },
                Threads = threadsList
            };

            return View("ViewForum", fd);
        }

        [CompressFilter]
        public ActionResult ViewThread(long? id, int? page)
        {
            int pageSize = 25;

            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var t = this.Forums.GetThread(id.Value);

            if (t == null || t.IsDeleted)
            {
                return View("NotFound");
            }

            Forum f = t.Forum;
            ForumAccess a = this.Security.GetUserForumAccess(CurrentUser, f);
            if (!a.CanRead)
            {
                return View("NotAuthorized");
            }

            lock (LanLordzApplication.Locker)
            {
                t = this.Forums.GetThread(id.Value);
                t.Views = t.Views + 1;
                this.Db.SubmitChanges();
            }

            if (CurrentUser != null)
            {
                ThreadRead tr = (from thr in this.Db.ThreadReads
                                 where thr.ThreadID == id && thr.UserID == CurrentUser.UserID
                                 select thr).SingleOrDefault();
                if (tr == null)
                {
                    tr = new ThreadRead
                    {
                        ThreadID = t.ThreadID,
                        UserID = CurrentUser.UserID,
                        DateRead = DateTime.UtcNow,
                    };

                    this.Db.ThreadReads.InsertOnSubmit(tr);
                }
                else
                {
                    tr.DateRead = DateTime.UtcNow;
                }
                this.Db.SubmitChanges();
            }

            var posts = this.GetPostsInformation(t);
            int postCount = posts.Count();
            int pages = Pager.PageCount(postCount, pageSize);

            page = Pager.ClampPage(page, pages);

            posts = posts.OrderBy(p => p.Post.CreateDate).Skip((page.Value - 1) * pageSize).Take(pageSize);

            ForumThreadModel tm = new ForumThreadModel(this)
            {
                Thread = t,
                Posts = posts.ToList(),
                UserAccess = a,
                PageInfo = new PaginationInformation
                {
                    Pager = this.Skins.GetDefaultThreadPager(),
                    CurrentPage = page,
                    Items = postCount,
                    ItemsPerPage = pageSize,
                    ControllerName = "Forums",
                    ActionName = "ViewThread",
                    PageAttribute = "page",
                    RouteValues = new System.Web.Routing.RouteValueDictionary(new { id = id })
                }
            };

            return View("ViewThread", tm);
        }

        [CompressFilter]
        public ActionResult ViewUnread()
        {
            if (this.CurrentUser == null)
            {
                return RedirectToAction("Index");
            }

            var tl = new ThreadList(this)
            {
                Threads = GetUnreadThreads(CurrentUser).OrderByDescending(t => t.LastPostDate)
            };

            return View("ViewUnread", tl);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Post(FormCollection values)
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            long? replyTo = Converter.Convert<long?>(values["ReplyTo"]);
            long? thread = Converter.Convert<long?>(values["Thread"]);
            long? forum = Converter.Convert<long?>(values["Forum"]);

            Forum f = null;
            Thread t = null;
            Post p = null;

            if (replyTo.HasValue)
            {
                p = this.Forums.GetPost(replyTo.Value);

                if (p == null || p.IsDeleted)
                {
                    return View("NotFound");
                }

                t = p.Thread;
                f = t.Forum;
            }

            if (thread.HasValue)
            {
                if (f != null)
                {
                    return View("Error");
                }

                t = this.Forums.GetThread(thread.Value);

                if (t == null || t.IsDeleted)
                {
                    return View("NotFound");
                }

                f = t.Forum;
            }

            if (forum.HasValue)
            {
                if (f != null)
                {
                    return View("Error");
                }

                f = this.Forums.GetForum(forum.Value);

                if (f == null)
                {
                    return View("NotFound");
                }
            }

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && t != null && t.IsLocked))
            {
                return View("NotAuthorized");
            }

            string title = Converter.Convert<string>(values["Title"]);
            string text = Converter.Convert<string>(values["Text"]);

            DateTime now = DateTime.UtcNow;

            if (t == null)
            {
                t = new Thread
                {
                    CreateDate = now,
                    Title = title,
                    Views = 0,
                    Level = 0,
                    IsLocked = false,
                    Forum = f,
                };

                t.Forum = f;
                this.Db.Threads.InsertOnSubmit(t);
            }

            if (access.CanModerate)
            {
                string locked = Converter.Convert<string>(values["Locked"]);
                long? level = Converter.Convert<long?>(values["Level"]);

                if (string.Equals(locked, "on"))
                {
                    t.IsLocked = true;
                }
                else
                {
                    t.IsLocked = false;
                }

                if (level.HasValue)
                {
                    t.Level = level.Value;
                }
            }

            User cu = CurrentUser;

            Post newPost = new Post
            {
                CreateDate = now,
                ModifyDate = now,
                UserID = cu.UserID,
                ModifyUserID = cu.UserID,
                Text = text,
                Title = title,
                Thread = t,
                ResponseToPostID = null
            };

            // TODO: Validate the post

            this.Db.Posts.InsertOnSubmit(newPost);
            this.Db.SubmitChanges();

            int pageSize = 25;

            return Redirect(
                Url.Action("ViewThread", new
                {
                    id = t.ThreadID,
                    page = Pager.PageCount(t.Posts.Where(post => post.IsDeleted == false).Count(), pageSize)
                }) + "#" + newPost.PostID);
        }

        private IEnumerable<ThreadInformation> GetUnreadThreads(User user)
        {
            if (user == null)
            {
                return null;
            }

            var threads = GetAllUnreadThreads(user.UserID);

            var threadInfo = GetThreadInformation(threads, user);

            return GetViewableThreads(threadInfo, user);
        }

        private IEnumerable<ThreadInformation> GetViewableThreads(IEnumerable<ThreadInformation> threadInfo, User user)
        {
            List<long> forums = new List<long>(from t in threadInfo
                                               group t by t.Thread.ForumID into g
                                               select g.Key);

            // Check the access of each forum.
            List<long> inaccessableForums = new List<long>();
            foreach (var forum in forums)
            {
                Forum f = this.Db.Forums.Where(fo => fo.ForumID == forum).Single();
                if (!this.Security.GetUserForumGroupAccess(user, f.ForumGroup) || !this.Security.GetUserForumAccess(user, f).CanView)
                {
                    inaccessableForums.Add(forum);
                }
            }

            return new List<ThreadInformation>(from t in threadInfo
                                               where !inaccessableForums.Where(fo => fo == t.Thread.ForumID).Any()
                                               select t);
        }

        #region Mark All Read
        private void MarkAllThreadsRead(long userId, DateTime asOfDate)
        {
            // TODO: Make Faster.
            var threads = GetAllUnreadThreads(userId);

            foreach (var thread in threads)
            {
                ThreadRead tr = this.Db.ThreadReads.Where(thr => thr.UserID == userId && thr.ThreadID == thread.ThreadID).SingleOrDefault();

                if (tr == null)
                {
                    tr = new ThreadRead
                    {
                        ThreadID = thread.ThreadID,
                        UserID = userId,
                        DateRead = asOfDate
                    };

                    this.Db.ThreadReads.InsertOnSubmit(tr);
                }

                tr.DateRead = (tr.DateRead > asOfDate ? tr.DateRead : asOfDate);
            }

            this.Db.SubmitChanges();
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult MarkAllRead(FormCollection values)
        {
            if (this.CurrentUser != null)
            {
                DateTime asOf = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(values["ReadDate"]))
                {
                    if (DateTime.TryParse(values["ReadDate"], out asOf))
                    {
                        if (asOf >= DateTime.UtcNow)
                        {
                            asOf = DateTime.UtcNow;
                        }
                    }
                }

                this.MarkAllThreadsRead(this.CurrentUser.UserID, asOf);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Search
        [CompressFilter]
        public ActionResult Search()
        {
            // TODO:  Must update the search view to handle a blank search.
            return View("Search", new BlankModel(this));
        }

        private IEnumerable<ThreadInformation> GetSearchResults(User user, string terms, bool isInclusive)
        {
            string[] splitTerms = terms.Split(" \r\n\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            List<string> termsList = new List<string>();
            foreach (string t in splitTerms)
            {
                if (t.Length < 3)
                {
                    throw new Exception("The search term \"" + t + "\" is too short. All search terms must be at least 3 characters long.");
                }

                string term = t.ToUpperInvariant();

                if (term.Length > 15)
                {
                    term = term.Substring(0, 15);
                }

                if (!termsList.Contains(term))
                {
                    termsList.Add(term);
                }

                if (termsList.Count >= 6)
                {
                    break;
                }
            }

            if (termsList.Count == 0)
                return null;

            var threads = new List<ThreadInformation>(GetThreadInformation(GetSearchTermResults(termsList[0]), user));

            for (int i = 1; i < termsList.Count; i++)
            {
                var newResults = new List<ThreadInformation>(GetThreadInformation(GetSearchTermResults(termsList[i]), user));

                if (isInclusive)
                {
                    foreach (ThreadInformation newRow in newResults)
                    {
                        long threadId = newRow.Thread.ThreadID;
                        if (!threads.Any(t => t.Thread.ThreadID == threadId))
                        {
                            threads.Add(newRow);
                        }
                    }
                }
                else
                {
                    List<ThreadInformation> newThreads = new List<ThreadInformation>();

                    foreach (ThreadInformation newRow in newResults)
                    {
                        long threadId = newRow.Thread.ThreadID;
                        if (threads.Any(t => t.Thread.ThreadID == threadId))
                        {
                            newThreads.Add(newRow);
                        }
                    }

                    threads = newThreads;
                }
            }

            return GetViewableThreads(threads, user);
        }

        private IQueryable<Thread> GetSearchTermResults(string term)
        {
            return from thread in this.Db.Threads
                   where thread.Title.Contains(term) || thread.Posts.Any(post => post.Title.Contains(term) || post.Text.Contains(term))
                   select thread;
        }
        #endregion

        [CompressFilter]
        public ActionResult DeletePost(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            Post p = this.Forums.GetPost(id.Value);

            if (p == null || p.IsDeleted)
            {
                return View("NotFound");
            }

            Thread t = p.Thread;

            long firstPostId = t.Posts.Where(post => post.IsDeleted == false).OrderBy(post => post.CreateDate).FirstOrDefault().PostID;
            long lastPostId = t.Posts.Where(post => post.IsDeleted == false).OrderByDescending(post => post.CreateDate).FirstOrDefault().PostID;
            
            Forum f = t.Forum; 

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && p.PostID != lastPostId))
            {
                return View("NotAuthorized");
            }

            var del = new DeletePostModel(this)
            {
                Post = p,
                UserAccess = access,
                FirstPost = (p.PostID == firstPostId),
            };

            return View("DeletePost", del);
        }

        [CompressFilter]
        public ActionResult EditPost(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            Post p = this.Forums.GetPost(id.Value);

            if (p == null || p.IsDeleted)
            {
                return View("NotFound");
            }

            Thread t = p.Thread;
            Forum f = t.Forum;

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && (t.IsLocked || p.UserID != CurrentUser.UserID)))
            {
                return View("NotAuthorized");
            }

            var epm = new EditPostModel(this)
            {
                Forum = f,
                Thread = t,
                Post = p,
                AvailableThreadLevels = this.Db.ThreadLevels.ToList(),
                UserAccess = access
            };

            return View("EditPost", epm);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditPost(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            Post p = this.Forums.GetPost(id.Value);

            if (p == null || p.IsDeleted)
            {
                return View("NotFound");
            }

            Thread t = p.Thread;
            Forum f = t.Forum;
            bool firstPost = t.Posts.Where(post => post.IsDeleted == false).OrderBy(post => post.CreateDate).First().PostID == id;

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && (t.IsLocked || p.UserID != CurrentUser.UserID)))
            {
                return View("NotAuthorized");
            }

            // TODO: Validate the post

            p.Title = values["Title"];
            p.Text = values["Text"];
            p.ModifyDate = DateTime.UtcNow;
            p.ModifyUserID = CurrentUser.UserID;

            if (firstPost)
            {
                t.Title = p.Title;
            }

            if (access.CanModerate)
            {
                string locked = Converter.Convert<string>(values["Locked"]);
                long? level = Converter.Convert<long?>(values["Level"]);

                if (string.Equals(locked, "on"))
                {
                    t.IsLocked = true;
                }
                else
                {
                    t.IsLocked = false;
                }

                if (level.HasValue)
                {
                    t.Level = level.Value;
                }
            }

            this.Db.SubmitChanges();

            return RedirectToAction("ViewThread", new { id = t.ThreadID });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult DeletePost(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            Post p = this.Forums.GetPost(id.Value);

            if (p == null || p.IsDeleted)
            {
                return View("NotFound");
            }

            Thread t = p.Thread;

            long firstPostId = t.Posts.Where(post => post.IsDeleted == false).OrderBy(post => post.CreateDate).FirstOrDefault().PostID;
            long lastPostId = t.Posts.Where(post => post.IsDeleted == false).OrderByDescending(post => post.CreateDate).FirstOrDefault().PostID;

            Forum f = t.Forum;

            ForumAccess access = this.Security.GetUserForumAccess(CurrentUser, f);

            if (!access.CanPost || (!access.CanModerate && p.PostID != lastPostId))
            {
                return View("NotAuthorized");
            }

            bool canDeleteThread = access.CanModerate || p.PostID == firstPostId;
            bool deleteThread = canDeleteThread && p.PostID == firstPostId;

            if (access.CanModerate)
            {
                string del = Converter.Convert<string>(values["DeleteThread"]);

                if (string.Equals(del, "on"))
                {
                    deleteThread = true;
                }
                else
                {
                    deleteThread = false;
                }
            }

            if (deleteThread)
            {
                t.IsDeleted = true;
                this.Db.SubmitChanges();

                return RedirectToAction("ViewForum", new { id = f.ForumID });
            }
            else
            {
                p.IsDeleted = true;
                this.Db.SubmitChanges();

                return RedirectToAction("ViewThread", new { id = t.ThreadID });
            }
        }

        [CompressFilter]
        public ActionResult Search(string searchTerms)
        {
            var threads = new ThreadList(this)
            {
                Threads = this.GetSearchResults(this.CurrentUser, searchTerms, true).OrderByDescending(t => t.LastPostDate)
            };

            return View("Search", threads);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Search(FormCollection values)
        {
            if (!string.IsNullOrEmpty(values["SearchTerms"]))
            {
                return Search(values["SearchTerms"]);
            }
            else
            {
                return Search();
            }
        }
    }
}
