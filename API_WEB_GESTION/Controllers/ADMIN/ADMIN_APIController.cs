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
    public class ADMIN_API_R_MODEL
    {
        public API_PROY_CAB API_PROY_CAB { get; set; }
        public API_PROY_DET API_PROY_DET { get; set; }
        public API_ATTR_CONT_TYPE API_ATTR_CONT_TYPE { get; set; }
        public API_ATTR_HTTP API_ATTR_HTTP { get; set; }
    }
    public class ADMIN_API_PARAMS
    {
        public List<Tuple<long>> Items { get; set; }
        public ADMIN_API_PARAMS()
        {
            Items = new List<Tuple<long>>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in Items) { sb.Append(a.Item1 + ", " + "\r\n"); }
            return sb.ToString();
        }
    }
    public class ADMIN_API_PARAMS_RES
    {
        public List<Tuple<long,int>> Items { get; set; }
        public ADMIN_API_PARAMS_RES()
        {
            Items = new List<Tuple<long, int>>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in Items) { sb.Append(a.Item1 + ", " + "\r\n"); }
            return sb.ToString();
        }
    }
    public class ADMIN_APIController : Controller
    {
        private API_CLS API_CLS = new API_CLS();
        private API_ENT API_ENT = new API_ENT();
        public string GLOBAL_SESSION = "SESSION_ADMIN_API";
        public string GLOBAL_SESSION_RES = "SESSION_ADMIN_API_DET";

        public void ViewBags()
        {
        }
        public void ViewBags_RES()
        {
            ViewBag.ATTR_HTML = API_CLS.API_ATTR_HTTP.Where(m => m.Estado == 1).ToList();
            ViewBag.ATTR_CT = API_CLS.API_ATTR_CONT_TYPE.Where(m => m.Estado == 1).ToList();
        }
        public ActionResult I(int filtro_codigo = 0, string filtro_nombre = "", int filtro_estado = 1)
        {
            try
            {

                ViewBags();
                var mo = API_CLS.API_PROY_CAB
                .Where(m =>
                   (filtro_codigo != 0 ? m.ProyID == filtro_codigo : true) &&
                    (filtro_nombre.Trim().Length > 0 ? m.Nombre.Trim().Contains(filtro_nombre.Trim()) : true) &&
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
            Session[GLOBAL_SESSION] = JsonConvert.DeserializeObject<ADMIN_API_PARAMS>(JSON);
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
        public ActionResult N(API_PROY_CAB API_PROY_CAB)
        {
            ViewBag.ERR = "0";
            try
            {
                ViewBags();
                API_PROY_CAB.ProyID = 0;
                API_PROY_CAB.URL = (API_PROY_CAB.URL == null ? "-" : API_PROY_CAB.URL);
                API_PROY_CAB.Descrip = (API_PROY_CAB.Descrip == null ? "-" : API_PROY_CAB.Descrip);
                API_PROY_CAB.FechaDIG = DateTime.Now;
                API_PROY_CAB.UsuarioDIG = "SISTEMA";

                if (ModelState.IsValid)
                {


                    API_CLS.API_PROY_CAB.Add(API_PROY_CAB);
                    API_CLS.SaveChanges();

                    ADMIN_API_PARAMS PARAMS = new ADMIN_API_PARAMS();
                    PARAMS.Items.Add(Tuple.Create(API_PROY_CAB.ProyID));
                    E_AJX(JsonConvert.SerializeObject(PARAMS));
                    return RedirectToAction("E");
                }
                else
                {
                    ViewBag.ERR = "<i class='fas fa-times-circle'></i> DEBE INGRESAR TODOS LOS CAMPOS.";
                    return View(API_PROY_CAB);
                }
                //return View(API_PROY_CAB);
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
                ADMIN_API_PARAMS PARAMS = (ADMIN_API_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);
                return View(API_PROY_CAB);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult E(API_PROY_CAB API_PROY_CAB)
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags();
                API_PROY_CAB.URL = (API_PROY_CAB.URL == null ? "-" : API_PROY_CAB.URL);
                API_PROY_CAB.Descrip = (API_PROY_CAB.Descrip == null ? "" : API_PROY_CAB.Descrip);
                API_PROY_CAB.UsuarioDIG = "SISTEMA";
                if (ModelState.IsValid)
                {
                    API_CLS.Entry(API_PROY_CAB).State = EntityState.Modified;
                    API_CLS.SaveChanges();
                    ViewBag.UPD = "<i class='fas fa-check-circle'></i> SE HA ACTUALIZADO CORRECTAMENTE.";
                    return View(API_PROY_CAB);
                }
                else
                {
                    ViewBag.ERR = "<i class='fas fa-times-circle'></i> DEBE INGRESAR TODOS LOS CAMPOS.";
                    return View(API_PROY_CAB);
                }
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        #region RECURSOS
        public ActionResult R_I(string fitro_R_nombre="", int fitro_R_estado = 1)
        {
            try
            {
                ADMIN_API_PARAMS PARAMS = (ADMIN_API_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);

                List<ADMIN_API_R_MODEL> model = new List<ADMIN_API_R_MODEL>();
                List<API_ATTR_CONT_TYPE> LIST_ATTR_CT = API_CLS.API_ATTR_CONT_TYPE.ToList();
                List<API_ATTR_HTTP> LIST_ATTR_HTTP = API_CLS.API_ATTR_HTTP.ToList();
                
                foreach (var item in 
                API_CLS.API_PROY_DET
                .Where(m => 
                    (m.ProyID == ID) &&
                    (fitro_R_nombre.Trim().Length > 0 ? m.Nombre.Trim().Contains(fitro_R_nombre.Trim()) : true) &&
                    (fitro_R_estado != -1 ? m.Estado == fitro_R_estado : true)
                )
                .ToList())
                {
                    model.Add(new ADMIN_API_R_MODEL()
                    {
                        API_PROY_CAB = API_PROY_CAB,
                        API_PROY_DET = item,
                        API_ATTR_CONT_TYPE = (LIST_ATTR_CT.Where(m => m.CTAtrID == item.CTAtrID).Count() > 0 ? LIST_ATTR_CT.Where(m => m.CTAtrID == item.CTAtrID).FirstOrDefault() : new API_ATTR_CONT_TYPE() { CTAtrID = 0, Nombre = "-" }),
                        API_ATTR_HTTP = (LIST_ATTR_HTTP.Where(m => m.HAtrID == item.HAtrID).Count() > 0 ? LIST_ATTR_HTTP.Where(m => m.HAtrID == item.HAtrID).FirstOrDefault() : new API_ATTR_HTTP() { HAtrID = 0, Nombre = "-" })
                    });
                }

                return PartialView("R_I", model);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        public JsonResult RE_AJX(string JSON)
        {
            Session[GLOBAL_SESSION_RES] = JsonConvert.DeserializeObject<ADMIN_API_PARAMS_RES>(JSON);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult R_N()
        {
            ViewBag.ERR = "0";
            try
            {
                ViewBags_RES();
                ADMIN_API_PARAMS PARAMS = (ADMIN_API_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);
                ViewBag.API_PROY_CAB = API_PROY_CAB;

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
        public ActionResult R_N(API_PROY_DET API_PROY_DET)
        {
            ViewBag.ERR = "0";
            try
            {
                ViewBags_RES();
                ADMIN_API_PARAMS PARAMS = (ADMIN_API_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);
                ViewBag.API_PROY_CAB = API_PROY_CAB;

                int ID2 = 0;
                try {ID2 = API_CLS.API_PROY_DET.Where(m=>m.ProyID==ID).Max(m=>m.DetID)+1;}
                catch { ID2 = 1; }

                API_PROY_DET.ProyID = ID;
                API_PROY_DET.DetID = ID2;
                API_PROY_DET.URL = (API_PROY_DET.URL == null ? "-" : API_PROY_DET.URL);
                API_PROY_DET.Descrip = (API_PROY_DET.Descrip == null ? "-" : API_PROY_DET.Descrip);
                API_PROY_DET.Request = (API_PROY_DET.Request == null ? " " : API_PROY_DET.Request);
                API_PROY_DET.Response = (API_PROY_DET.Response == null ? " " : API_PROY_DET.Response);
                API_PROY_DET.HAtrID = 0;
                API_PROY_DET.CTAtrID = 0;
                API_PROY_DET.FechaDIG = DateTime.Now;
                API_PROY_DET.UsuarioDIG = "SISTEMA";

                if (ModelState.IsValid)
                {
                    API_CLS.API_PROY_DET.Add(API_PROY_DET);
                    API_CLS.SaveChanges();

                    ADMIN_API_PARAMS_RES PARAMS_RES = new ADMIN_API_PARAMS_RES();
                    PARAMS_RES.Items.Add(Tuple.Create(API_PROY_DET.ProyID, API_PROY_DET.DetID));
                    E_AJX(JsonConvert.SerializeObject(PARAMS));
                    return RedirectToAction("R_E");
                }
                else
                {
                    ViewBag.ERR = "<i class='fas fa-times-circle'></i> DEBE INGRESAR TODOS LOS CAMPOS.";
                    return View(API_PROY_DET);
                }

                //return View(API_PROY_DET);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        public ActionResult R_E()
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags_RES();
                ADMIN_API_PARAMS_RES PARAMS_RES = (ADMIN_API_PARAMS_RES)Session[GLOBAL_SESSION_RES];
                long ID = PARAMS_RES.Items.ElementAt(0).Item1;
                int ID2 = PARAMS_RES.Items.ElementAt(0).Item2;

                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);
                ViewBag.API_PROY_CAB = API_PROY_CAB;

                API_PROY_DET API_PROY_DET = API_CLS.API_PROY_DET.Find(ID,ID2);
                HELPERS.TrimModelProperties(typeof(API_PROY_DET), API_PROY_DET);
                return View(API_PROY_DET);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult R_E(API_PROY_DET API_PROY_DET)
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags_RES();
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(API_PROY_DET.ProyID);
                HELPERS.TrimModelProperties(typeof(API_PROY_CAB), API_PROY_CAB);
                ViewBag.API_PROY_CAB = API_PROY_CAB;

                API_PROY_DET.UsuarioDIG = (API_PROY_DET.UsuarioDIG == null ? "-" : API_PROY_DET.UsuarioDIG);
                API_PROY_DET.URL = (API_PROY_DET.URL == null ? "-" : API_PROY_DET.URL);
                API_PROY_DET.Descrip = (API_PROY_DET.Descrip == null ? "-" : API_PROY_DET.Descrip);
                API_PROY_DET.Request = (API_PROY_DET.Request == null ? " " : API_PROY_DET.Request);
                API_PROY_DET.Response = (API_PROY_DET.Response == null ? " " : API_PROY_DET.Response);

                if (ModelState.IsValid)
                {
                    API_CLS.Entry(API_PROY_DET).State = EntityState.Modified;
                    API_CLS.SaveChanges();
                    ViewBag.UPD = "<i class='fas fa-check-circle'></i> SE HA ACTUALIZADO CORRECTAMENTE.";
                    return View(API_PROY_DET);
                }
                else
                {
                    ViewBag.ERR = "<i class='fas fa-times-circle'></i> DEBE INGRESAR TODOS LOS CAMPOS.";
                    return View(API_PROY_DET);
                }
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        #endregion
    }
}