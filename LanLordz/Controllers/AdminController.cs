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
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
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

            this.SendMail(CurrentUser, roleId, subject, body, eventId);

            return View("SendMailSuccess");
        }

        [CompressFilter]
        public ActionResult EditGroups()
        {
            return View(this.Security.GetAllRoles());
        }

        [CompressFilter]
        public ActionResult EditGroup(long id)
        {
            var group = this.Security.GetRoleById(id);

            if (group == null)
            {
                return View("NotFound");
            }

            return View(group);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult AddGroupMember(long id, FormCollection values)
        {
            var group = this.Security.GetRoleById(id);

            if (group == null)
            {
                return View("NotFound");
            }

            var user = this.Users.GetUserByUsername(values["Username"]);
            if (user != null)
            {
                this.Security.AddUserToRole(user, group);
            }

            return RedirectToAction("EditGroup", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult DeleteGroupMember(long id, FormCollection values)
        {
            var group = this.Security.GetRoleById(id);

            if (group == null)
            {
                return View("NotFound");
            }

            long userId;
            if (long.TryParse(values["UserId"], out userId))
            {
                var user = this.Users.GetUserById(userId);
                if (user != null)
                {
                    this.Security.RemoveUserFromRole(user, group);
                }
            }

            return RedirectToAction("EditGroup", new { id });
        }

        public ActionResult EditTitles()
        {
            var model = this.Db.Titles.ToList();
            ViewBag.Roles = this.Security.GetAllRoles();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult AddTitle(FormCollection values)
        {
            var text = values["TitleText"];
            if (!string.IsNullOrWhiteSpace(text))
            {
                Title title = null;

                var type = values["Type"];
                switch (type)
                {
                    case "user":
                        var user = this.Users.GetUserByUsername(values["Username"]);
                        if (user != null)
                        {
                            title = new Title { User = user };
                        }
                        break;

                    case "role":
                        long roleId;
                        if (long.TryParse(values["RoleId"], out roleId))
                        {
                            title = new Title { RoleID = roleId };
                        }
                        break;

                    case "count":
                        int count;
                        if (int.TryParse(values["PostCountThreshold"], out count))
                        {
                            title = new Title { PostCountThreshold = count };
                        }
                        break;

                    default:
                        break;
                }

                if (title != null)
                {
                    title.TitleText = text;
                    this.Db.Titles.InsertOnSubmit(title);
                    this.Db.SubmitChanges();
                }
            }

            return RedirectToAction("EditTitles");
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult DeleteTitle(FormCollection values)
        {
            long titleId;
            if (long.TryParse(values["TitleId"], out titleId))
            {
                this.Db.Titles.DeleteAllOnSubmit(from t in this.Db.Titles
                                                 where t.TitleID == titleId
                                                 select t);
                this.Db.SubmitChanges();
            }

            return RedirectToAction("EditTitles");
        }

        private void SendMail(User fromUser, long toGroupId, string subject, string body, long? invitationEventId)
        {
            using (var client = new SmtpClient(this.Config.SmtpHost, this.Config.SmtpPort))
            {
                MailAddress from = new MailAddress(fromUser.Email);

                string attachmentData = null;

                if (invitationEventId.HasValue)
                {
                    var evt = this.Events.GetEvent(invitationEventId.Value);
                    var vnu = this.Events.GetVenue(evt.VenueID);

                    string address = this.Config.AdminEmail;

                    try
                    {
                        address = (new MailAddress(this.Config.AdminEmail, this.Config.SiteName)).ToString();
                    }
                    catch
                    {
                    }

                    Appointment apt = new Appointment
                    {
                        StartTime = this.ConvertDateTime(evt.BeginDateTime, this.Config.DefaultTimeZoneInfo),
                        EndTime = this.ConvertDateTime(evt.EndDateTime, this.Config.DefaultTimeZoneInfo),
                        Title = evt.Title,
                        Description = evt.Info,
                        Location = string.IsNullOrEmpty(vnu.Address) ? vnu.Name : vnu.Address,
                        Organizer = address
                    };

                    attachmentData = apt.AsICalendar();
                }

                foreach (User user in this.Security.GetUsersInRole(toGroupId))
                {
                    if (!user.ReceiveAdminEmail)
                    {
                        continue;
                    }

                    MailAddress to;

                    try
                    {
                        to = new MailAddress(user.Email);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    using (var message = new MailMessage(from, to))
                    {
                        message.IsBodyHtml = true;
                        message.Subject = subject;
                        message.Body = body;

                        if (!string.IsNullOrEmpty(attachmentData))
                        {
                            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(attachmentData)))
                            {
                                Attachment attachment = new Attachment(contentStream, "Invitation.ics", "text/calendar");
                                message.Attachments.Add(attachment);
                            }
                        }

                        client.Send(message);
                    }
                }
            }
        }
    }
}
