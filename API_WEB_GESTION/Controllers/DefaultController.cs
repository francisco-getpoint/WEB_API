using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_WEB_GESTION.Controllers.util;

namespace API_WEB_GESTION.Controllers
{
    public class DefaultController : Controller
    {
        API_CLS API_CLS = new API_CLS();
        API_ENT API_ENT = new API_ENT();

        private void ViewBags()
        {
        }
        public ActionResult I()
        {
            ViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult I(string u, string p)
        {
            try
            {
                ViewBags();
                ViewBag.ERR = "0";

                if
                (API_CLS.API_PROF_USERS
                .Where(m =>
                m.USERNAME.Trim().ToUpper() == u
                ).Count() == 0)
                {
                    ViewBag.ERR = "NO SE HA ENCONTRADO USUARIO <b>" + u + "<b>";
                    return View();
                }

                if
                (API_CLS.API_PROF_USERS
                .Where(m =>
                m.USERNAME.Trim().ToUpper() == u &&
                m.Estado == 1
                ).Count() == 0)
                {
                    ViewBag.ERR = "USUARIO <b>" + u + "<b> DESHABILITADO";
                    return View();
                }

                List<SP_VAL_API_PROF_USERS_LOGIN_Result> respuesta = API_ENT.SP_VAL_API_PROF_USERS_LOGIN(
                CONFIGS.APP_KEY_PHRASE
                , u, p).ToList();

                if (respuesta.Count == 0)
                {
                    ViewBag.ERR = "ERROR AL INICIAR SESIÓN";
                    return View();
                }
                else
                {
                    if (respuesta.ElementAt(0).RESPUESTA == 1)
                    {
                        var USERNAME = respuesta.ElementAt(0).USERNAME;
                        Session[VARS.VARS_SESSION] = API_CLS.API_PROF_USERS.Where(m => m.USERNAME == USERNAME).FirstOrDefault();
                        Session.Timeout = 99999;
                        return RedirectToAction("I", "H");
                    }
                    else
                    {
                        ViewBag.ERR = "USUARIO / CONTRASEÑA INCORRECTOS";
                        return View();
                    }
                }
                //return View();
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new  MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }


        public ActionResult LO()
        {
            ViewBag.Error = "0";
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("I", "Default");
        }
    }
}