//-----------------------------------------------------------------------
// <copyright file="AdminController.cs" company="LAN Lordz, inc.">
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

    public class AdminController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            return !this.UserIsAuthorized() ? this.View("NotAuthorized") : this.View();
        }

        [CompressFilter]
        public ActionResult EditConfig()
        {
            if (!this.UserIsAuthorized())
            {
                return View("NotAuthorized");
            }
            
            // Prevent reading of Cached Data.
            this.Config.Freshen();

            var model = new MasterConfigModel(this)
            {
                AvailableThemes = this.AppManager.GetAvailableThemes(false),
                AvailableTimezones = this.AppManager.GetAvailableTimezones(false),
                Groups = this.AppManager.GetAllRoles().ToList(),
            };

            return View("EditConfig", model);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditConfig(FormCollection values)
        {
            if (!this.UserIsAuthorized())
            {
                return View("NotAuthorized");
            }

            // Prevent saving of cached data.
            this.Config.Freshen();

            try
            {
                UpdateModel(this.Config);
                this.Db.SubmitChanges();
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("Form", ex.Message);

                var model = new MasterConfigModel(this)
                {
                    AvailableThemes = this.AppManager.GetAvailableThemes(false),
                    AvailableTimezones = this.AppManager.GetAvailableTimezones(false),
                    Groups = this.AppManager.GetAllRoles().ToList(),
                };

                return View("EditConfig", model);
            }

            // Makes config immediately available.
            this.Config.Invalidate();

            return RedirectToAction("Index");
        }

        [CompressFilter]
        public ActionResult SendMail()
        {
            if (!this.UserIsAuthorized())
            {
                return View("NotAuthorized");
            }

            var roles = this.AppManager.GetAllRoles().ToList();
            var events = this.Events.GetUpcomingEvents();

            var model = new SendMailModel(this)
            {
                Roles = roles,
                UpcomingEvents = events
            };

            return View("SendMail", model);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken, CompressFilter]
        public ActionResult SendMail(FormCollection values)
        {
            if (!this.UserIsAuthorized())
            {
                return View("NotAuthorized");
            }

            var roleId = long.Parse(values["Role"]);
            var subject = values["Subject"];
            var body = this.FormatPostText(values["Body"]);
            long? eventId = null;

            if (!string.IsNullOrEmpty(values["Event"]))
            {
                eventId = long.Parse(values["Event"]);
            }

            this.AppManager.SendMail(CurrentUser, roleId, subject, body, eventId);

            return View("SendMailSuccess");
        }

        private bool UserIsAuthorized()
        {
            return this.CurrentUser != null && this.Security.IsUserAdministrator(this.CurrentUser);
        }
    }
}
