using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_WEB_DOC.Controllers.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_DOC.Controllers
{
    public class ERR_INFOController : Controller
    {
        private API_CLS API_CLS = new API_CLS();
        private API_ENT API_ENT = new API_ENT();
        public ActionResult I(int filtro_codigo = 0, int filtro_tipo = -1, string filtro_nombre = "")
        {
            try
            {
                var mo = API_CLS.API_RESPONSE_ERRORS
                .Where(m =>
                    (filtro_codigo != 0 ? m.ErrID == filtro_codigo : true) &&
                    (filtro_tipo != -1 ? m.IndTipo == (filtro_tipo == 1 ? true : false) : true) &&
                    (filtro_nombre.Trim().Length > 0 ? m.Nombre.Trim().Contains(filtro_nombre.Trim()) : true) &&
                   m.Estado == 1
                )
                .ToList();

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_I_P", mo.ToList());
                }
                else
                {
                    return View(mo.ToList());
                }
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
    }
}