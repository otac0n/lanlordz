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
    using System.ServiceModel.Syndication;
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;

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
        public ActionResult ViewNews(string format = "html")
        {
            var posts = (from p in this.Db.Posts
                         where p.ThreadID == this.Config.NewsThread
                         where p.IsDeleted == false
                         orderby p.CreateDate descending
                         select p).Take(10).ToList();

            if (format == "atom")
            {
                var items = new List<SyndicationItem>();

                foreach (var p in posts)
                {
                    var u = p.User;
                    var content = new TextSyndicationContent(
                        this.FormatPostText(p.Text),
                        TextSyndicationContentKind.Html);

                    var item = new SyndicationItem(
                         title: p.Title,
                         content: content,
                         itemAlternateLink: new Uri(this.Request.Url, Url.Action("ViewNews")),
                         id: p.PostID.ToString(),
                         lastUpdatedTime: p.ModifyDate);

                    var person = new SyndicationPerson(
                        u.Email,
                        u.Username,
                        Url.Action("ViewProfile", "Account", new { id = u.UserID, title = this.CreateUrlTitle(u.Username) }));

                    item.Authors.Add(person);

                    items.Add(item);
                }

                return this.Atom(this.Config.SiteName + " News", items);
            }
            else
            {
                return View(new ViewNewsModel
                {
                    Posts = posts
                });
            }
        }

        [CompressFilter]
        public ActionResult ContactUs()
        {
            var model = new ContactUsModel();

            if (this.CurrentUser != null)
            {
                model.Email = this.CurrentUser.Email;
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult ContactUs(ContactUsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admins = (from u in this.Db.Users
                          where u.UsersRoles.Any(ur => ur.Role.IsAdministrator)
                          select u).ToList();

            if (admins.Count == 0)
            {
                this.ModelState.AddModelError("", "There are no administrators configured for this website.  An email cannot be sent.");
                return View(model);
            }

            var message = new MailMessage
            {
                From = new MailAddress(model.Email),
                Body = model.Message,
                IsBodyHtml = false,
            };

            foreach (var admin in admins)
            {
                message.To.Add(admin.Email);
            }

            try
            {
                using (var smpt = new SmtpClient(this.Config.SmtpHost, this.Config.SmtpPort))
                {
                    smpt.Send(message);
                }
            }
            catch (SmtpException ex)
            {
                this.ModelState.AddModelError("", ex.GetBaseException().Message);
                return View(model);
            }

            return View("ContactUsSuccess");
        }
    }
}
