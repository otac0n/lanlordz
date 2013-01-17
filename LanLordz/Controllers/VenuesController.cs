using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LanLordz.Models;
using System.Data.Common;
using System.Data;

namespace LanLordz.Controllers
{
    public class VenuesController : AdminControllerBase
    {
        public ActionResult Index()
        {
            var venues = this.Db.Venues.ToList();
            return View(venues);
        }

        public ActionResult Create()
        {
            return View("Edit");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Venue venue)
        {
            if (!this.ModelState.IsValid)
            {
                return View("Edit");
            }

            try
            {
                this.Db.Venues.InsertOnSubmit(venue);
                this.Db.SubmitChanges();
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
                return View("EditSponsor");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var venue = this.Db.Venues.SingleOrDefault(v => v.VenueID == id);

            if (venue == null)
            {
                return View("NotFound");
            }

            return View(venue);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(long id, FormCollection form)
        {
            var venue = this.Db.Venues.SingleOrDefault(v => v.VenueID == id);

            if (venue == null)
            {
                return View("NotFound");
            }

            if (!this.TryUpdateModel(venue) || !this.ModelState.IsValid)
            {
                return View(venue);
            }

            try
            {
                this.Db.SubmitChanges();
            }
            catch (DataException ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
                return View(venue);
            }

            return RedirectToAction("Index");
        }
    }
}
