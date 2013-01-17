using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LanLordz.Models;
using System.Data.Common;

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

        [HttpPost]
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
    }
}
