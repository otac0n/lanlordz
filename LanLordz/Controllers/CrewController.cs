//-----------------------------------------------------------------------
// <copyright file="CrewController.cs" company="LAN Lordz, inc.">
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
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class CrewController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(this.CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            return View("Index", new BlankModel(this));
        }

        private void DeleteEventImage(long eventImageId)
        {
            var i = (from image in this.Db.EventImages
                    where image.EventImageID == eventImageId
                    select image).Single();
            var eventId = i.EventID;
            this.Db.EventImages.DeleteOnSubmit(i);
            this.Db.SubmitChanges();

            this.Events.InvalidateEventImages(eventId);
        }

        private void AddEventImage(long eventId, byte[] imageData)
        {
            var image = new EventImage
            {
                EventID = eventId,
                Image = imageData,
                ScrapeBusterKey = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLowerInvariant().Substring(0, 10)
            };

            this.Db.EventImages.InsertOnSubmit(image);

            this.Db.SubmitChanges();

            this.Events.InvalidateEventImages(eventId);
        }

        private void CheckUserIn(long eventId, long userId)
        {
            var reg = this.Db.Registrations.SingleOrDefault(r => r.EventID == eventId && r.UserID == userId);
            reg.IsCheckedIn = true;

            this.Db.SubmitChanges();

            this.Events.InvalidateEventRegistrations(eventId);
        }

        private void CancelUserCheckIn(long eventId, long userId)
        {
            var reg = this.Db.Registrations.SingleOrDefault(r => r.EventID == eventId && r.UserID == userId);
            reg.IsCheckedIn = false;

            this.Db.SubmitChanges();

            this.Events.InvalidateEventRegistrations(eventId);
        }

        [CompressFilter]
        public ActionResult UserCheckIn(long? id)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                var cem = new ChooseEventModel(this)
                {
                    Events = this.Events.GetAllEvents()
                };

                return View("ChooseEvent", cem);
            }

            var uci = new UserCheckInModel(this)
            {
                Registrations = this.Events.GetEventRegistrations(id.Value)
            };

            return this.View("UserCheckIn", uci);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult UserCheckIn(long? id, FormCollection values)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                long eventId;
                if (long.TryParse(values["Event"], out eventId))
                {
                    return RedirectToAction("UserCheckIn", new { id = eventId });
                }

                return this.RedirectToAction("UserCheckIn");
            }

            long userId;
            int action;

            if (values.AllKeys.Where(k => k == "Username").Any())
            {
                var user = this.GetUser(values["Username"]);

                if (user == null)
                {
                    throw new ArgumentException("User does not exists.");
                }

                this.Events.RegisterEventUser(id.Value, user.UserID);

                userId = user.UserID;
                action = 1;
            }
            else
            {
                userId = long.Parse(values["User"]);
                action = int.Parse(values["Action"]);
            }

            if (action == 1)
            {
                this.CheckUserIn(id.Value, userId);
            }
            else
            {
                this.CancelUserCheckIn(id.Value, userId);
            }

            return RedirectToAction("UserCheckIn", new { id = id.Value });
        }

        [CompressFilter]
        public ActionResult DoorPrizes(long? id)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                var cem = new ChooseEventModel(this)
                {
                    Events = this.Events.GetAllEvents()
                };

                return View("ChooseEvent", cem);
            }

            var uci = new DoorPrizesModel(this)
            {
                Prizes = this.Db.Prizes.Where(p => p.EventId == id.Value).ToList()
            };

            return this.View("DoorPrizes", uci);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult DoorPrizes(long? id, FormCollection values)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                long eventId = 0;
                if (long.TryParse(values["Event"], out eventId))
                {
                    return RedirectToAction("DoorPrizes", new { id = eventId });
                }
                else
                {
                    return RedirectToAction("DoorPrizes");
                }
            }

            var action = values["Action"];

            if (action == "ADD")
            {
                var prize = new Prize()
                {
                    EventId = id.Value,
                    Name = values["Value"],
                };

                this.Db.Prizes.InsertOnSubmit(prize);

                this.Db.SubmitChanges();
            }
            else if (action == "DEL")
            {
                long prizeId = 0;
                if (!long.TryParse(values["Value"], out prizeId))
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not convert value passed into a PrizeId"
                    });
                }

                var prize = this.Db.Prizes.Where(p => p.PrizeId == prizeId && p.EventId == id.Value).SingleOrDefault();

                if (prize == null)
                {
                    return View("NotFound");
                }

                if (prize.WinnerUserId.HasValue)
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not delete the prize, because it has already been drawn."
                    });
                }

                this.Db.Prizes.DeleteOnSubmit(prize);

                this.Db.SubmitChanges();
            }
            else if (action == "DRW")
            {
                long prizeId = 0;
                if (!long.TryParse(values["Value"], out prizeId))
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not convert value passed into a PrizeId"
                    });
                }

                var prize = this.Db.Prizes.Where(p => p.PrizeId == prizeId && p.EventId == id.Value).SingleOrDefault();

                if (prize == null)
                {
                    return View("NotFound");
                }

                if (prize.WinnerUserId != null)
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not draw the prize, because it has already been drawn."
                    });
                }

                var users = (from r in this.Db.Registrations
                             where r.EventID == id.Value
                             where r.IsCheckedIn
                             select r.UserID).ToList();
                
                var rand = new SecureRandom();

                var winnerUserId = users[rand.Next(users.Count)];

                prize.WinnerUserId = winnerUserId;

                this.Db.SubmitChanges();
            }
            else if (action == "RVK")
            {
                long prizeId = 0;
                if (!long.TryParse(values["Value"], out prizeId))
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not convert value passed into a PrizeId"
                    });
                }

                var prize = this.Db.Prizes.Where(p => p.PrizeId == prizeId && p.EventId == id.Value).SingleOrDefault();

                if (prize == null)
                {
                    return View("NotFound");
                }

                if (!prize.WinnerUserId.HasValue)
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could not revoke the prize, because it has not been drawn."
                    });
                }

                prize.WinnerUserId = null;

                this.Db.SubmitChanges();
            }
            else
            {
                return View("NotAvailable");
            }

            return RedirectToAction("DoorPrizes", new { id = id.Value });
        }

        [CompressFilter]
        public ActionResult EditImages(long? id)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                var cem = new ChooseEventModel(this)
                {
                    Events = this.Events.GetAllEvents()
                };

                return View("ChooseEvent", cem);
            }

            var uci = new EditImagesModel(this)
            {
                Images = this.Events.GetEventImages(id.Value)
            };

            return this.View("EditImages", uci);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditImages(long? id, FormCollection values)
        {
            if (this.CurrentUser == null || !this.Security.IsUserInRole(CurrentUser, this.Config.CrewGroup))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                long eventId = 0;
                if (long.TryParse(values["Event"], out eventId))
                {
                    return RedirectToAction("EditImages", new { id = eventId });
                }
                
                return this.RedirectToAction("EditImages");
            }

            var deletions = from k in values.AllKeys
                            where k.IndexOf("delete-") == 0
                            where values[k] == "on"
                            let split = k.Split("-".ToCharArray())
                            select new
                            {
                                EventImageID = long.Parse(split[1]),
                            };

            foreach (var deletion in deletions)
            {
                this.DeleteEventImage(deletion.EventImageID);
            }

            foreach (var fileKey in Request.Files.AllKeys)
            {
                var file = Request.Files[fileKey];

                if (file == null || file.ContentLength == 0)
                {
                    continue;
                }
                else
                {
                    var newImage = new byte[file.InputStream.Length];
                    file.InputStream.Seek(0, SeekOrigin.Begin);
                    file.InputStream.Read(newImage, 0, newImage.Length);
                    this.AddEventImage(id.Value, newImage);
                }
            }

            return RedirectToAction("EditImages", new { id = id.Value });
        }
    }
}
