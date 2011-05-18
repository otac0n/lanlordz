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
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!this.UserIsAuthorized())
            {
                filterContext.Result = this.View("NotAuthorized");
            }
        }

        private bool UserIsAuthorized()
        {
            return this.CurrentUser != null && this.Security.IsUserAdministrator(this.CurrentUser);
        }

        [CompressFilter]
        public ActionResult Index()
        {
            return this.View();
        }

        [CompressFilter]
        public ActionResult EditConfig()
        {
            this.Config.Freshen();

            var model = new MasterConfigModel
            {
                AvailableThemes = this.GetAvailableThemes(false),
                AvailableTimezones = this.GetAvailableTimezones(false),
                Groups = this.Db.Roles.ToList(),
            };

            return View("EditConfig", model);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditConfig(FormCollection values)
        {
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

                var model = new MasterConfigModel
                {
                    AvailableThemes = this.GetAvailableThemes(false),
                    AvailableTimezones = this.GetAvailableTimezones(false),
                    Groups = this.Db.Roles.ToList(),
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
            var roles = this.Db.Roles.ToList();
            var events = this.Events.GetUpcomingEvents();

            var model = new SendMailModel
            {
                Roles = roles,
                UpcomingEvents = events
            };

            return View("SendMail", model);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult SendMail(FormCollection values)
        {
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
    }
}
