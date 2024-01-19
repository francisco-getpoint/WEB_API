using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_WEB_GESTION.Controllers.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_GESTION.Controllers
{
    public class HController : Controller
    {
        API_CLS API_CLS = new API_CLS();
        API_ENT API_ENT = new API_ENT();

      
        public ActionResult I()
        {
            try
            {               
                API_PROF_USERS API_PROF_USERS = (API_PROF_USERS)Session[VARS.VARS_SESSION];
                return View(API_PROF_USERS);
            }
            catch (Exception ex) {
                return RedirectToAction("I", "TO");
            }

        }
        public ActionResult USER_EDIT()
        {
            try
            {
                API_PROF_USERS API_PROF_USERS = (API_PROF_USERS)Session[VARS.VARS_SESSION];
                return View(API_PROF_USERS);
            }
            catch (Exception ex)
            {
                return RedirectToAction("I", "TO");
            }
        }

        public JsonResult AJX_USER_EDIT_INFO(
      string  USERNAME
       ,string Nombre1
       ,string Nombre2
       ,string Apellido1
       ,string Apellido2
       ,string Mail1
        )
        {
            try {

                API_PROF_USERS API_PROF_USERS = API_CLS.API_PROF_USERS.Find(USERNAME);
                API_PROF_USERS.Nombre1 = Nombre1;
                API_PROF_USERS.Nombre2 = Nombre2;
                API_PROF_USERS.Apellido1 = Apellido1;
                API_PROF_USERS.Apellido2 = Apellido2;
                API_PROF_USERS.Mail1 = Mail1;
                API_CLS.Entry(API_PROF_USERS).State = System.Data.Entity.EntityState.Modified;
                API_CLS.SaveChanges();
                Session[VARS.VARS_SESSION] = API_PROF_USERS;
                return Json(new { Success = true}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) {
                return Json(new { Success = false,Error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AJX_USER_EDIT_INFO_PW(string c, string c2, string c3)
        {
            try
            {
                API_PROF_USERS API_PROF_USERS = (API_PROF_USERS)Session[VARS.VARS_SESSION];
                string PW_ACTUAL = API_ENT.SP_SEL_API_PROF_USERS_LOGIN_LEE_PW(CONFIGS.APP_KEY_PHRASE, API_PROF_USERS.USERNAME).ElementAt(0);

                if (PW_ACTUAL.Trim() != c.Trim())
                {
                    throw new Exception("CONTRASEÑA ACTUAL NO COINCIDE");
                }

                if (PW_ACTUAL == c3)
                {
                    throw new Exception("CONTRASEÑA NUEVA DEBE SER DISTINTA QUE LA ACTUAL");
                }

                API_PROF_USERS.PW = API_ENT.SP_GEN_API_PROF_USERS_LOGIN_NEW_PW(CONFIGS.APP_KEY_PHRASE, c3).ElementAt(0);
                API_CLS.Entry(API_PROF_USERS).State = System.Data.Entity.EntityState.Modified;
                API_CLS.SaveChanges();

                Session[VARS.VARS_SESSION] = API_PROF_USERS;
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        
    }
}