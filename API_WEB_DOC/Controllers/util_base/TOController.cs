using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_GESTION.Controllers.util_base
{
    public class TOController : Controller
    {
        // GET: TO
        public ActionResult I()
        {
            return View();
        }

        public ActionResult S_ALIVE()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AJX_KS_ALIVE()
        {
            return new JsonResult { Data = "Success" };
        }

    }
}