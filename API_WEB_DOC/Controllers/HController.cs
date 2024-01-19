using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_WEB_DOC.Controllers.util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_DOC.Controllers
{
    public class HController : Controller
    {

        API_CLS API_CLS = new API_CLS();
        API_ENT API_ENT = new API_ENT();

        public ActionResult I()
        {
            try
            {
                MODELS.BASE_LOGIN BASE_LOGIN = (MODELS.BASE_LOGIN)Session[VARS.VARS_SESSION];
                return View(BASE_LOGIN);
            }
            catch (Exception ex)
            {
                return RedirectToAction("I", "TO");
            }

        }

        public List<API_PROY_CAB> LIST_API_PROY_CAB(string u)
        {
            List<API_PROY_CAB> model = new List<API_PROY_CAB>();
            List<API_PROY_CAB> API_PROY_CAB = API_CLS.API_PROY_CAB.Where(m => m.Estado == 1).ToList();

            foreach (var item in API_CLS.API_USUARIOS_PROY.Where(m => m.Username == u).ToList())
            {
                if (API_PROY_CAB.Where(m => m.ProyID == item.ProyID).Count() > 0)
                {
                    model.Add(API_PROY_CAB.Where(m => m.ProyID == item.ProyID).OrderBy(t => t.Descrip).FirstOrDefault());
                }
            }
            return model;
        }
        public List<API_PROY_DET> LIST_API_PROY_DET(string u)
        {
            List<API_PROY_DET> model = new List<API_PROY_DET>();
            List<API_PROY_DET> API_PROY_DET = API_CLS.API_PROY_DET.Where(m => m.Estado == 1).ToList();

            foreach (var item in API_CLS.API_USUARIOS_PROY_DET.Where(m => m.Username == u).ToList())
            {
                if (API_PROY_DET.Where(m => m.ProyID == item.ProyID && m.DetID == item.DetID).Count() > 0)
                {
                    model.Add(API_PROY_DET.Where(m => m.ProyID == item.ProyID && m.DetID == item.DetID).FirstOrDefault());
                }
            }
            return model.OrderBy( o => o.Nombre).ToList();
        }

        public JsonResult AJX_R(string JSON)
        {
            RController R = new RController();
            Session[R.GLOBAL_SESSION] = JsonConvert.DeserializeObject<R_PARAMS>(JSON);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}