using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_WEB_GESTION.Controllers.util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_GESTION.Controllers.ADMIN
{
    public class ADMIN_API_ERRORS_PARAMS
    {
        public List<Tuple<long>> Items { get; set; }
        public ADMIN_API_ERRORS_PARAMS()
        {
            Items = new List<Tuple<long>>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in Items){sb.Append(a.Item1 + ", " + "\r\n");}
            return sb.ToString();
        }
    }
    public class ADMIN_API_ERRORSController : Controller
    {
        private API_CLS API_CLS = new API_CLS();
        private API_ENT API_ENT = new API_ENT();
        public string GLOBAL_SESSION = "SESSION_ADMIN_API_ERRORS";

        public void ViewBags()
        {
        }
        public ActionResult I(int filtro_codigo=0,int filtro_tipo=-1,string filtro_nombre="",int filtro_estado=1)
        {
            try
            {

                ViewBags();
                var mo = API_CLS.API_RESPONSE_ERRORS
                .Where(m=>
                    (filtro_codigo != 0 ? m.ErrID == filtro_codigo : true) &&
                    (filtro_tipo!=-1? m.IndTipo==(filtro_tipo==1?true:false) :true) &&
                    (filtro_nombre.Trim().Length>0 ? m.Nombre.Trim().Contains(filtro_nombre.Trim()) :true) &&
                    (filtro_estado != -1 ? m.Estado == filtro_estado : true) 
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

        public JsonResult E_AJX(string JSON)
        {
            Session[GLOBAL_SESSION] = JsonConvert.DeserializeObject<ADMIN_API_ERRORS_PARAMS>(JSON);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult N()
        {
            ViewBag.ERR = "0";
            try
            {
                ViewBags();
                return View();
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        [HttpPost]
        public ActionResult N(API_RESPONSE_ERRORS API_RESPONSE_ERRORS)
        {
            ViewBag.ERR = "0";
            try
            {
                ViewBags();
             
                if (ModelState.IsValid)
                {
                    if (API_CLS.API_RESPONSE_ERRORS.Where(m => m.ErrID == API_RESPONSE_ERRORS.ErrID).Count() > 0) {
                        ViewBag.ERR = "<i class='fas fa-times-circle'></i> YA EXISTE UN ERROR CON EL CÓDIGO <b>" + API_RESPONSE_ERRORS.ErrID + "</b>";
                        return View(API_RESPONSE_ERRORS);
                    }

                    API_CLS.API_RESPONSE_ERRORS.Add(API_RESPONSE_ERRORS);
                    API_CLS.SaveChanges();

                    ADMIN_API_ERRORS_PARAMS PARAMS = new ADMIN_API_ERRORS_PARAMS();
                    PARAMS.Items.Add(Tuple.Create(API_RESPONSE_ERRORS.ErrID));
                    E_AJX(JsonConvert.SerializeObject(PARAMS));
                    return RedirectToAction("E");
                }
                return View(API_RESPONSE_ERRORS);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        public ActionResult E()
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags();
                ADMIN_API_ERRORS_PARAMS PARAMS = (ADMIN_API_ERRORS_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                API_RESPONSE_ERRORS API_RESPONSE_ERRORS = API_CLS.API_RESPONSE_ERRORS.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_RESPONSE_ERRORS), API_RESPONSE_ERRORS);
                return View(API_RESPONSE_ERRORS);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        [HttpPost]
        public ActionResult E(API_RESPONSE_ERRORS API_RESPONSE_ERRORS)
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags();
                if (ModelState.IsValid)
                {
                    API_CLS.Entry(API_RESPONSE_ERRORS).State = EntityState.Modified;
                    API_CLS.SaveChanges();
                    ViewBag.UPD = "<i class='fas fa-check-circle'></i> SE HA ACTUALIZADO CORRECTAMENTE.";
                    return View(API_RESPONSE_ERRORS);
                }
                else
                {
                    ViewBag.ERR = "<i class='fas fa-times-circle'></i> DEBE INGRESAR TODOS LOS CAMPOS.";
                    return View(API_RESPONSE_ERRORS);
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