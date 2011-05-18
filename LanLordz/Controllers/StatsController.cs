//-----------------------------------------------------------------------
// <copyright file="StatsController.cs" company="LAN Lordz, inc.">
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
    using System.Linq;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class StatsController : LanLordzBaseController
    {
        private SiteStatisticsModel GetSiteStats()
        {
            var previousEvents = from e in this.Db.Events
                                 where e.EndDateTime < DateTime.UtcNow
                                 orderby e.EndDateTime descending
                                 select e;

            var largestEvents = from e in this.Db.Events
                                orderby e.Registrations.Where(r => r.IsCheckedIn).Count() descending
                                select e;

            var usersByCreateDate = from u in this.Db.Users
                                    orderby u.CreateDate descending
                                    select u.UserID;
            long recentUserId = usersByCreateDate.FirstOrDefault();

            var usersByPostCount = from u in this.Db.Users
                                   orderby u.Posts.Count() descending
                                   select u.UserID;
            long activeUserId = usersByPostCount.FirstOrDefault();


            return new SiteStatisticsModel
            {
                AllEventsStats = this.AppManager.GetEventsStats(previousEvents),
                RecentEventsStats = this.AppManager.GetEventsStats(previousEvents.Take(5)),
                BiggestEvent = largestEvents.FirstOrDefault(),
                BiggestEventStats = this.AppManager.GetEventsStats(largestEvents.Take(1)),
                TotalThreads = this.Db.Threads.Count(),
                TotalPosts = this.Db.Posts.Count(),
                MostActivePoster = this.AppManager.GetUserInformation(activeUserId, false),
                NewestUser = this.AppManager.GetUserInformation(recentUserId, false)
            };
        }

        [CompressFilter]
        public ActionResult Index()
        {
            var stats = this.GetSiteStats();
            return View("Index", stats);
        }

        [CompressFilter, ActionName("Events")]
        public ActionResult EventsHistory(int? page, string sortBy, string dir)
        {
            int pageSize = 15;

            var events = this.AppManager.GetEventStats();
            var eventsCount = events.Count();
            var pages = Pager.PageCount(eventsCount, pageSize);

            page = Pager.ClampPage(page, pages);

            events = events.OrderByDescending(t => t.Event.BeginDateTime).Skip((page.Value - 1) * pageSize).Take(pageSize);

            return View("Events", new StatsEventsModel
            {
                Events = events,
                PageInfo = new PaginationInformation
                {
                    Pager = this.Skins.GetDefaultPager(),
                    ControllerName = "Stats",
                    ActionName = "Events",
                    CurrentPage = page,
                    Items = eventsCount,
                    ItemsPerPage = pageSize,
                    PageAttribute = "page",
                    RouteValues = null
                }
            });
        }

        [CompressFilter, ActionName("Users")]
        public ActionResult UsersStats(int? page, string sortBy, string dir)
        {
            var pageSize = 15;

            var allUsers = this.Users.GetAllUsers();

            var usersCount = allUsers.Count();
            var pages = Pager.PageCount(usersCount, pageSize);
            page = Pager.ClampPage(page, pages);

            var users = this.Users.GetUserInformation(allUsers);

            users = users.OrderByDescending(u => u.EventsCheckedIn).Skip((page.Value - 1) * pageSize).Take(pageSize);

            return View("Users", new StatsUsersModel
            {
                Users = users,
                PageInfo = new PaginationInformation
                {
                    Pager = this.Skins.GetDefaultPager(),
                    ControllerName = "Stats",
                    ActionName = "Users",
                    CurrentPage = page,
                    Items = usersCount,
                    ItemsPerPage = pageSize,
                    PageAttribute = "page",
                    RouteValues = null
                }
            });
        }
    }
}
