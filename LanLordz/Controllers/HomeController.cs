//-----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="LAN Lordz, inc.">
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
    using System.Linq;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class HomeController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult PngFix()
        {
            string pngFix = System.IO.File.ReadAllText(Server.MapPath("~/Skins/PngFix.htc"));
            string blankImage = Response.ApplyAppPathModifier("~/Skins/blank.gif");
            pngFix = pngFix.Replace("##BLANKIMAGE##", blankImage);

            return Content(pngFix, "text/x-component");
        }

        [CompressFilter]
        public ActionResult Index()
        {
            return View();
        }

        [CompressFilter]
        public ActionResult ViewCrew()
        {
            return View(new CrewDetailsModel
            {
                Members = this.Users.GetUserInformation(from u in this.Db.Users
                                                        join ur in this.Db.UsersRoles on u.UserID equals ur.UserID
                                                        where ur.RoleID == this.Config.CrewGroup
                                                        orderby u.CreateDate
                                                        select u).ToList()
            });
        }

        [CompressFilter]
        public ActionResult ViewSponsors()
        {
            return View(new ViewSponsorsModel
            {
                Sponsors = this.Db.Sponsors.ToList()
            });
        }

        [CompressFilter]
        public ActionResult ViewNews()
        {
            return View(new ViewNewsModel
            {
                Posts = (from p in this.Db.Posts
                         where p.ThreadID == this.Config.NewsThread
                         orderby p.CreateDate descending
                         select p).Take(10).ToList()
            });
        }
    }
}
