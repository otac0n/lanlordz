using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LanLordz.Controllers
{
    public abstract class AdminControllerBase : LanLordzBaseController
    {
        protected override bool UserIsAuthorized()
        {
            return this.CurrentUser != null && this.Security.IsUserAdministrator(this.CurrentUser);
        }
    }
}