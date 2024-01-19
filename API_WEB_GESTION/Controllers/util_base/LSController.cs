using API_WEB_GESTION.Controllers.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_GESTION.Controllers.util_base
{
    public class LSController : Controller
    {
        public ActionResult I()
        {
            MODELS.BASE_ERRORS BASE_ERRORS = (MODELS.BASE_ERRORS)Session[VARS.VARS_SESSION_ERR];
            if (BASE_ERRORS == null)
            {
                return RedirectToAction("I", "TO");
            }

            ViewBag.Error_M = BASE_ERRORS.MODULE;
            ViewBag.Error_EX = (BASE_ERRORS.EX.Message == null ? "-" : BASE_ERRORS.EX.Message);
            ViewBag.Error_EX2 = (BASE_ERRORS.EX.InnerException == null ? "-" : BASE_ERRORS.EX.InnerException.ToString());
            ViewBag.Error_EX3 = (BASE_ERRORS.EX.StackTrace == null ? "-" : BASE_ERRORS.EX.StackTrace);
            ViewBag.Error_P = BASE_ERRORS.PARAMS;
            return View();
        }

        public ActionResult ERR_404()
        {
            return View();
        }
    }
}