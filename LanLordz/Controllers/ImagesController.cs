//-----------------------------------------------------------------------
// <copyright file="ImagesController.cs" company="LAN Lordz, inc.">
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
    using System.Drawing;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.Models.CachedModels;
    using LanLordz.SiteTools;
    using Microsoft.SqlServer.Types;

    public class ImagesController : LanLordzBaseController
    {
        private EventImage GetEventImage(long eventImageId)
        {
            return this.Db.EventImages.Where(i => i.EventImageID == eventImageId).SingleOrDefault();
        }

        public ActionResult ViewAvatar(long id)
        {
            UserAvatar avatar = this.Users.GetUserAvatar(id);

            if (avatar != null && true)
            {
                byte[] imageData = avatar.Avatar.ToArray();

                string eTag = CalculateMd5(imageData);

                if (this.Request.Headers["If-None-Match"] == eTag)
                {
                    return this.NotModified(
                        HttpCacheability.Public,
                        DateTime.Now.AddMinutes(2)
                    );
                }

                string mimeType = ImageUtilities.GetImageMimeType(imageData);

                return this.Image(
                    imageData,
                    mimeType,
                    HttpCacheability.Public,
                    DateTime.Now.AddMinutes(2),
                    eTag
                );
            }
            else
            {
                return this.HttpError(404, this.View("NotFound"));
            }
        }

        public ActionResult Image(long id, int? thumbnail, string hash)
        {
            EventImage image = this.GetEventImage(id);

            if (image != null)
            {
                if (CalculateScrapeBuster(image.ScrapeBusterKey, thumbnail).Equals(hash))
                {
                    byte[] imageData = image.Image.ToArray();

                    if (thumbnail.HasValue)
                    {
                        imageData = ImageUtilities.ResizeToFit(imageData, new Size(thumbnail.Value, thumbnail.Value), true);
                    }
                    else
                    {
                        imageData = ImageUtilities.ResizeToFit(imageData, new Size(800, 600), false);
                    }

                    string eTag = CalculateMd5(imageData);

                    if (this.Request.Headers["If-None-Match"] == eTag)
                    {
                        return this.NotModified(
                            HttpCacheability.Public,
                            DateTime.Now.AddMonths(12)
                        );
                    }

                    string mimeType = ImageUtilities.GetImageMimeType(imageData);

                    return this.Image(
                        imageData,
                        mimeType,
                        HttpCacheability.Public,
                        DateTime.Now.AddMonths(12),
                        eTag
                    );
                }
                else
                {
                    return this.HttpError(403, this.View("NotAvailable"));
                }
            }
            else
            {
                return this.HttpError(404, this.View("NotFound"));
            }
        }
        
        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult TagImage(long id, FormCollection values)
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            var x1 = Math.Min(1.0, Math.Max(0.0, double.Parse(values["x1"])));
            var y1 = Math.Min(1.0, Math.Max(0.0, double.Parse(values["y1"])));
            var x2 = Math.Min(1.0, Math.Max(0.0, double.Parse(values["x2"])));
            var y2 = Math.Min(1.0, Math.Max(0.0, double.Parse(values["y2"])));

            var user = this.GetUser(values["username"]);

            if (user == null)
            {
                throw new ArgumentException("User does not exists.");
            }

            SqlGeometryBuilder b = new SqlGeometryBuilder();
            b.SetSrid(0);
            b.BeginGeometry(OpenGisGeometryType.Polygon);
            b.BeginFigure(x1, y1);
            b.AddLine(x2, y1);
            b.AddLine(x2, y2);
            b.AddLine(x1, y2);
            b.AddLine(x1, y1);
            b.EndFigure();
            b.EndGeometry();

            var geometry = b.ConstructedGeometry;

            this.Db.EventImageUserTags.InsertOnSubmit(new EventImageUserTag
            {
                EventImageId = id,
                CreatorUserId = this.CurrentUser.UserID,
                TaggedUserUserId = user.UserID,
                RegionGeometry = geometry,
            });

            this.Db.SubmitChanges();

            return Redirect(this.Request.UrlReferrer.ToString());
        }

        private IQueryable<EventImage> FilterImages(IQueryable<EventImage> images, long? eventId, long? userId)
        {
            if (userId.HasValue)
            {
                // TODO: Once we start tagging users, add filtering based on user.
                images = from i in images
                         where i.UserTags.Where(ut => ut.TaggedUserUserId == userId.Value).Any()
                         select i;

            }

            if (eventId.HasValue)
            {
                images = from i in images
                         where i.EventID == eventId.Value
                         select i;

            }

            return from i in images
                   orderby i.EventImageID
                   orderby i.EventID
                   select i;
        }

        public ActionResult Gallery(long? @event, long? user, int? page)
        {
            int pageSize = 50;

            CachedEvent viewEvent = null;
            if (@event.HasValue)
            {
                viewEvent = this.Events.GetEvent(@event.Value);

                if (viewEvent == null)
                {
                    return this.HttpError(404, this.View("NotFound"));
                }
            }

            UserInformation viewUser = null;
            if (user.HasValue)
            {
                viewUser = this.Users.GetUserInformation(user.Value);

                if (viewUser == null)
                {
                    return this.HttpError(404, this.View("NotFound"));
                }
            }

            var images = FilterImages(this.Db.EventImages, @event, user);

            int imageCount = images.Count();
            int pages = Pager.PageCount(imageCount, pageSize);
            page = Pager.ClampPage(page, pages);

            var viewImages = images.Skip((page.Value - 1) * pageSize).Take(pageSize).ToList();

            return this.View(new ImageGalleryModel
            {
                Event = viewEvent,
                User = viewUser,
                Images = viewImages,
                PageInfo = new PaginationInformation
                {
                    Pager = this.Skins.GetDefaultGalleryPager(),
                    CurrentPage = page,
                    Items = imageCount,
                    ItemsPerPage = pageSize,
                    ControllerName = "Images",
                    ActionName = "Gallery",
                    PageAttribute = "page",
                    RouteValues = new System.Web.Routing.RouteValueDictionary(new { @event = @event, user = user })
                }
            });
        }

        public ActionResult ViewImage(int id, long? @event, long? user)
        {
            CachedEvent viewEvent = null;
            if (@event.HasValue)
            {
                viewEvent = this.Events.GetEvent(@event.Value);

                if (viewEvent == null)
                {
                    return this.HttpError(404, this.View("NotFound"));
                }
            }

            UserInformation viewUser = null;
            if (user.HasValue)
            {
                viewUser = this.Users.GetUserInformation(user.Value);

                if (viewUser == null)
                {
                    return this.HttpError(404, this.View("NotFound"));
                }
            }

            var images = FilterImages(this.Db.EventImages, @event, user);
            var imageCount = images.Count();

            if (imageCount == 0)
            {
                return this.HttpError(404, this.View("NotFound"));
            }

            var viewImages = images.ToList();

            id = Math.Max(1, Math.Min(imageCount, id));
            var viewImage = viewImages.Skip(id - 1).Take(1).SingleOrDefault();

            var tags = (from t in this.Db.EventImageUserTags
                        where t.EventImageId == viewImage.EventImageID
                        select new UserTag
                        {
                            UserId = t.TaggedUserUserId,
                            Username = t.TaggedUser.Username,
                            TagRegion = t.RegionGeometry
                        }).ToList();

            return this.View(new ViewImageModel
            {
                Event = viewEvent,
                User = viewUser,
                Image = viewImage,
                Images = viewImages,
                UserTags = tags,
                AllowTagging = this.Security.IsUserAdministrator(this.CurrentUser)
            });
        }
    }
}
