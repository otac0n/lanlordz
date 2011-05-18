//-----------------------------------------------------------------------
// <copyright file=".cs" company="LAN Lordz, inc.">
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

using System;
using System.Web;
using System.IO;
using System.Net.Mail;
using System.Text;
using LanLordz.SiteTools;
using LanLordz.Controllers;

namespace LanLordz.Models
{
    public class LanLordzApplicationManager
    {
        private LanLordzDataContext db;

        private LanLordzBaseController controller;

        public LanLordzBaseController Controller
        {
            get
            {
                return this.controller;
            }
        }

        public LanLordzApplicationManager(LanLordzBaseController controller, LanLordzDataContext db)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            this.controller = controller;
            this.db = db;
        }

        private LanLordzDataContext DB
        {
            get
            {
                return this.db;
            }
        }

        public void SendConfirmationEmail(string email, HttpRequestBase request, HttpResponseBase response)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");
            MailAddress from = new MailAddress("admin@example.com");
            try
            {
                from = new MailAddress(this.controller.Config.AdminEmail);
            }
            catch
            {
            }

            MailAddress to;
            try
            {
                to = new MailAddress(email);
            }
            catch
            {
                return;
            }

            MailMessage message = new MailMessage(from, to);

            String body = this.controller.Config.ConfirmEmailText;

            String subject = this.controller.Config.ConfirmEmailSubject;

            EmailConfirm confirm = new EmailConfirm
            {
                Email = email,
                Key = Guid.NewGuid()
            };
            this.DB.EmailConfirms.InsertOnSubmit(confirm);
            this.DB.SubmitChanges();

            String Link = request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Authority + response.ApplyAppPathModifier("~/Account/Confirm?key=" + confirm.Key);

            try
            {
                message.Subject = subject;
                message.Body = String.Format(body, Link);
                message.IsBodyHtml = true;
            }
            catch (FormatException)
            {
                return;
            }

            try
            {
                SmtpClient client = new SmtpClient(this.controller.Config.SmtpHost, this.controller.Config.SmtpPort);
                client.Send(message);
            }
            catch
            {
            }
        }

        public void SendMail(User fromUser, long toGroupId, string subject, string body, long? invitationEventId)
        {
            SmtpClient client = new SmtpClient(this.controller.Config.SmtpHost, this.controller.Config.SmtpPort);

            MailAddress from = new MailAddress(fromUser.Email);

            string attachmentData = null;

            if (invitationEventId.HasValue)
            {
                var evt = this.Controller.Events.GetEvent(invitationEventId.Value);
                var vnu = this.Controller.Events.GetVenue(evt.VenueID);

                string address = this.controller.Config.AdminEmail;

                try
                {
                    address = (new MailAddress(this.controller.Config.AdminEmail, this.controller.Config.SiteName)).ToString();
                }
                catch
                {
                }

                Appointment apt = new Appointment
                {
                    StartTime = this.Controller.ConvertDateTime(evt.BeginDateTime, this.controller.Config.DefaultTimeZoneInfo),
                    EndTime = this.Controller.ConvertDateTime(evt.EndDateTime, this.controller.Config.DefaultTimeZoneInfo),
                    Title = evt.Title,
                    Description = evt.Info,
                    Location = string.IsNullOrEmpty(vnu.Address) ? vnu.Name : vnu.Address,
                    Organizer = address
                };

                attachmentData = apt.AsICalendar();
            }

            foreach (User user in this.Controller.Security.GetUsersInRole(toGroupId))
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

                MailMessage message = new MailMessage(from, to);
                message.IsBodyHtml = true;
                message.Subject = subject;
                message.Body = body;

                if (!string.IsNullOrEmpty(attachmentData))
                {
                    Attachment attachment = new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(attachmentData)), "Invitation.ics", "text/calendar");
                    message.Attachments.Add(attachment);
                }

                client.Send(message);
            }
        }
    }
}
