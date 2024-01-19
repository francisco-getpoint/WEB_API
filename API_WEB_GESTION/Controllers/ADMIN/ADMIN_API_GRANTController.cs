using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_LIB.Model.GET_POINT.GP_CLS;
using API_LIB.Model.GET_POINT.GP_ENT;
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
    public class ADMIN_API_GRANT_MODEL_LIST_E
    {
        public List<API_PROY_CAB> API_PROY_CAB = new List<API_PROY_CAB>();
        public List<API_PROY_DET> API_PROY_DET = new List<API_PROY_DET>();
        public string SESSION_PAN_EXP { get; set; }
    }
    public partial class ADMIN_API_GRANT_MODEL_LIST_E_API
    {
        public List<API_PROY_CAB> API_PROY_CAB_IN = new List<API_PROY_CAB>();
        public List<API_PROY_CAB> API_PROY_CAB_NI = new List<API_PROY_CAB>();

        public ADMIN_API_GRANT_MODEL_LIST_E_API
        (
            List<API_PROY_CAB> API_PROY_CAB_IN
            , List<API_PROY_CAB> API_PROY_CAB_NI
        )
        {
            this.API_PROY_CAB_IN = API_PROY_CAB_IN;
            this.API_PROY_CAB_NI = API_PROY_CAB_NI;
        }
    }
    public partial class ADMIN_API_GRANT_MODEL_LIST_E_API_RES
    {
        public List<API_PROY_DET> API_PROY_DET_IN = new List<API_PROY_DET>();
        public List<API_PROY_DET> API_PROY_DET_NI = new List<API_PROY_DET>();

        public ADMIN_API_GRANT_MODEL_LIST_E_API_RES
        (
            List<API_PROY_DET> API_PROY_DET_IN
            , List<API_PROY_DET> API_PROY_DET_NI
        )
        {
            this.API_PROY_DET_IN = API_PROY_DET_IN;
            this.API_PROY_DET_NI = API_PROY_DET_NI;
        }
    }

    public class ADMIN_API_GRANT_MODEL_LIST
    {
        public  Usuario Usuario { get; set; }
        public Empresa Empresa { get; set; }       
        public API_USUARIOS API_USUARIOS { get; set; }
    }
    public class ADMIN_API_GRANT_PARAMS
    {
        public List<Tuple<string>> Items { get; set; }
        public ADMIN_API_GRANT_PARAMS()
        {
            Items = new List<Tuple<string>>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in Items) { sb.Append(a.Item1 + ", " + "\r\n"); }
            return sb.ToString();
        }
    }
    public class ADMIN_API_GRANTController : Controller
    {
        private API_CLS API_CLS = new API_CLS();
        private API_ENT API_ENT = new API_ENT();

        private GP_CLS GP_CLS = new GP_CLS();
        private GP_ENT GP_ENT = new GP_ENT();

        public string GLOBAL_SESSION = "SESSION_ADMIN_API_GRANT";
        public string GLOBAL_SESSION_PAN_EXP = "SESSION_ADMIN_API_GRANT_PANEL";

        
        public void ViewBags()
        {
            ViewBag.EMPRESAS  = GP_CLS.Empresa.ToList();
        }

        public ActionResult I(int filtro_estado=1,string filtro_rut = "", string filtro_username = "", string filtro_apellidopat = "",int filtro_ind_token=1,string filtro_token="")
        {
            try
            {

                ViewBags();
                var model = GP_ENT.SP_SEL_API_GRANT_LIST_USUARIOS(filtro_rut, filtro_username, filtro_apellidopat, filtro_estado, filtro_ind_token, filtro_token).ToList();
                //List<Empresa> LIST_EMPRESAS = GP_CLS.Empresa.ToList();

                //List<Usuario> LIST_USUARIOS = GP_CLS.Usuario
                //.Where(m =>
                //    (filtro_rut.Trim().Length > 0 ? m.RutEmpresa.Trim().ToUpper() == filtro_rut.Trim().ToUpper() : true) &&
                //    (filtro_username.Trim().Length > 0 ? m.UserName.Trim().Contains(filtro_username.Trim()) : true) &&
                //    (filtro_apellidopat.Trim().Length > 0 ? m.ApellidoPat.Trim().Contains(filtro_apellidopat.Trim()) : true) && 
                //    m.Estado==1
                //).ToList();
                //List<string> LIST_USUARIOS_USERNAME = LIST_USUARIOS.Select(m => m.UserName.Trim()).ToList();

                //List<API_USUARIOS> LIST_API_USUARIOS = API_CLS.API_USUARIOS.Where(m => LIST_USUARIOS_USERNAME.Contains(m.Username.Trim())).ToList();

                //List<ADMIN_API_GRANT_MODEL_LIST> model = new List<ADMIN_API_GRANT_MODEL_LIST>();
                //foreach (var item in LIST_USUARIOS.ToList())
                //{
                //    model.Add(new ADMIN_API_GRANT_MODEL_LIST() {
                //        Empresa = (LIST_EMPRESAS.Where(m => m.Rut.Trim() == item.RutEmpresa.Trim()).Count() > 0 ? LIST_EMPRESAS.Where(m => m.Rut.Trim() == item.Rut.Trim()).FirstOrDefault() : new Empresa() { Rut = "-", RazonSocial = "-" })
                //        ,Usuario = item
                //        ,API_USUARIOS = (LIST_API_USUARIOS.Where(m => m.Username.Trim() == item.UserName.Trim()).Count() > 0 ? LIST_API_USUARIOS.Where(m => m.Username.Trim() == item.UserName.Trim()).FirstOrDefault() : new API_USUARIOS() {   API_TOKEN= "-"}) 
                //    });
                //}

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_I_P", model.ToList());
                }
                else
                {
                    return View(model.ToList());
                }
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        #region E
        public JsonResult E_AJX(string JSON)
        {
            Session[GLOBAL_SESSION] = JsonConvert.DeserializeObject<ADMIN_API_GRANT_PARAMS>(JSON);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult E()
        {
            ViewBag.ERR = "0";
            ViewBag.UPD = "0";
            try
            {
                ViewBags();
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;
                Session[GLOBAL_SESSION_PAN_EXP] = "";

                #region GENERACION DE TOKEN
                API_USUARIOS API_USUARIOS = API_CLS.API_USUARIOS.Find(ID);
                if (API_USUARIOS == null) {
                    API_USUARIOS = new API_USUARIOS();
                    API_USUARIOS.Username = ID.Trim().ToUpper();
                    API_USUARIOS.API_TOKEN = Guid.NewGuid().ToString().ToUpper();///ASIGNACION DE TOKEN UNICO 
                    API_USUARIOS.API_SECRET = Guid.NewGuid().ToString().ToUpper();///ASIGNACION DE SECRET UNICO 
                    API_USUARIOS.Estado = 0;
                    API_USUARIOS.VigenciaD = DateTime.Parse("01/01/1900");
                    API_USUARIOS.VigenciaH = DateTime.Parse("01/01/1900");
                    API_USUARIOS.IndValVigFecha = false;
                    API_CLS.API_USUARIOS.Add(API_USUARIOS);
                    API_CLS.SaveChanges();
                    HELPERS.TrimModelProperties(typeof(API_USUARIOS), API_USUARIOS);
                }
                else {
                    HELPERS.TrimModelProperties(typeof(API_USUARIOS), API_USUARIOS);
                }
                #endregion

                Usuario Usuario = GP_CLS.Usuario.Where(m => m.UserName.Trim() == API_USUARIOS.Username.Trim()).FirstOrDefault();
                HELPERS.TrimModelProperties(typeof(Usuario), Usuario);
                ViewBag.Usuario = Usuario;

                Empresa Empresa = GP_CLS.Empresa.Where(m => m.Rut.Trim() == Usuario.RutEmpresa.Trim()).FirstOrDefault();
                if (Empresa == null)
                {
                    Empresa = new Empresa() { Rut = "-", RazonSocial = "-", NombreFantasia = "-" };
                }
                else
                {
                    HELPERS.TrimModelProperties(typeof(Empresa), Empresa);
                }
                ViewBag.EMPRESA = Empresa;
                return View(API_USUARIOS);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        public JsonResult AJX_E_NEW_SECRET()
        {
            try
            {
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;
                API_USUARIOS API_USUARIOS = API_CLS.API_USUARIOS.Find(ID);
                API_USUARIOS.API_SECRET = Guid.NewGuid().ToString().ToUpper();
                API_CLS.Entry(API_USUARIOS).State = EntityState.Modified;
                API_CLS.SaveChanges();
              
                return Json(new { Success = true}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AJX_E_SET_ACCESO(
        int iv=0,
        int e=0,
        string vd="",
        string vh=""
        )
        {
            try
            {
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;
                API_USUARIOS API_USUARIOS = API_CLS.API_USUARIOS.Find(ID);

                DateTime VD = new DateTime();
                DateTime VH = new DateTime();

                if (iv == 1)
                {
                    try { VD = DateTime.Parse(vd); } catch { throw new Exception("EL RANGO DE FECHA DESDE NO ES VALIDO (FORMATO DD/MM/YYYY)"); }
                    try { VH = DateTime.Parse(vh); } catch { throw new Exception("EL RANGO DE FECHA HASTA NO ES VALIDO (FORMATO DD/MM/YYYY)"); }
                    if (VH < VD) { throw new Exception("EL RANGO DE FECHA DESDE DEBE SER MENOR QUE EL RANGO DE FECHA HASTA"); }
                }
                else {
                    VD = DateTime.Parse("01/01/1900");
                    VH = DateTime.Parse("01/01/1900");
                }

                API_USUARIOS.IndValVigFecha = (iv==1?true:false);
                API_USUARIOS.Estado = e;
                API_USUARIOS.VigenciaD = VD;
                API_USUARIOS.VigenciaH = VH;
                API_CLS.Entry(API_USUARIOS).State = EntityState.Modified;
                API_CLS.SaveChanges();

                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region API
        public JsonResult AJX_E_SET_PAN_EXP(string PAN_EXP)
        {
            Session[GLOBAL_SESSION_PAN_EXP] = PAN_EXP;
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult API_I()
        {
            try
            {
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;

                ADMIN_API_GRANT_MODEL_LIST_E model = new ADMIN_API_GRANT_MODEL_LIST_E();
                model.API_PROY_CAB = new List<API_PROY_CAB>();
                model.API_PROY_DET = new List<API_PROY_DET>();
                model.SESSION_PAN_EXP = Session[GLOBAL_SESSION_PAN_EXP].ToString();


                List<API_PROY_CAB> PROY = API_CLS.API_PROY_CAB.ToList();
                List<API_PROY_DET> PROY_DET = API_CLS.API_PROY_DET.ToList();
                List<API_USUARIOS_PROY> PROY_CLI = API_CLS.API_USUARIOS_PROY.Where(m => m.Username == ID).ToList();
                List<API_USUARIOS_PROY_DET> PROY_CLI_DET = API_CLS.API_USUARIOS_PROY_DET.Where(m => m.Username == ID).ToList();


                foreach (var item in PROY)
                {
                    if (PROY_CLI.Where(m => m.ProyID == item.ProyID).Count() > 0)
                    {
                        model.API_PROY_CAB.Add(item);

                        foreach (var item2 in PROY_DET.Where(m => m.ProyID == item.ProyID).ToList())
                        {
                            if (PROY_CLI_DET.Where(m => m.ProyID == item.ProyID && m.DetID == item2.DetID).Count() > 0)
                            {
                                model.API_PROY_DET.Add(item2);
                            }
                        }
                    }
                }
                return PartialView("API_I", model);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        #region MOD API
        public ActionResult M_API()
        {
            return View();
        }
        public ActionResult M_API_I()
        {
            try
            {
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;

                var IN = API_CLS.API_USUARIOS_PROY.Where(m => m.Username == ID).Select(m => m.ProyID).ToList();

                #region MODEL 1
                var model1 = API_CLS.API_PROY_CAB.Where(m => IN.Contains(m.ProyID))
               .ToList();
                #endregion

                #region MODEL 2
                var model2 = API_CLS.API_PROY_CAB.Where(m => !IN.Contains(m.ProyID)).ToList();
                #endregion

                return PartialView("M_API_I", new ADMIN_API_GRANT_MODEL_LIST_E_API(model1, model2));
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
        public JsonResult AJX_MOD_API_N_DEL(int ope, long pID)
        {
            try
            {

                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(pID);

                if (ope == 1)
                {
                    API_USUARIOS_PROY API_USUARIOS_PROY = new API_USUARIOS_PROY();
                    API_USUARIOS_PROY.Username = ID;
                    API_USUARIOS_PROY.ProyID = pID;
                    API_CLS.API_USUARIOS_PROY.Add(API_USUARIOS_PROY);
                    API_CLS.SaveChanges();
                }
                if (ope == 2)
                {
                    API_USUARIOS_PROY API_USUARIOS_PROY = API_CLS.API_USUARIOS_PROY.Find(ID, pID);
                    API_CLS.API_USUARIOS_PROY.Remove(API_USUARIOS_PROY);
                    API_CLS.SaveChanges();

                    List<API_PROY_DET> API_PROY_DET = API_CLS.API_PROY_DET.Where(m => m.ProyID == pID).ToList();

                    foreach (var item in API_CLS.API_USUARIOS_PROY_DET.Where(m => m.ProyID == pID).ToList())
                    {
                        API_CLS.API_USUARIOS_PROY_DET.Remove(item);
                        API_CLS.SaveChanges();
                    }
                }
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region MOD API RES
        public ActionResult M_API_RES()
        {
            return View();
        }
        public ActionResult M_API_RES_I(long MOD_PROYDET_ID = 0)
        {
            try
            {
                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;

                var IN = API_CLS.API_USUARIOS_PROY_DET.Where(m => m.Username == ID && m.ProyID == MOD_PROYDET_ID).Select(m => m.DetID).ToList();

                #region MODEL 1
                var model1 = API_CLS.API_PROY_DET.Where(m => m.ProyID == MOD_PROYDET_ID && IN.Contains(m.DetID))
               .ToList();
                #endregion

                #region MODEL 2
                var model2 = API_CLS.API_PROY_DET.Where(m => m.ProyID == MOD_PROYDET_ID && !IN.Contains(m.DetID))
                .ToList();
                #endregion

                return PartialView("M_API_RES_I", new ADMIN_API_GRANT_MODEL_LIST_E_API_RES(model1, model2));
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }

        public JsonResult AJX_MOD_API_RES_N_DEL(int ope, long pID, long pdID)
        {
            try
            {

                ADMIN_API_GRANT_PARAMS PARAMS = (ADMIN_API_GRANT_PARAMS)Session[GLOBAL_SESSION];
                string ID = PARAMS.Items.ElementAt(0).Item1;

                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(pID);
                API_PROY_DET API_PROY_DET = API_CLS.API_PROY_DET.Find(pID, pdID);

                if (ope == 1)
                {
                    API_USUARIOS_PROY_DET API_USUARIOS_PROY_DET = new API_USUARIOS_PROY_DET();
                    API_USUARIOS_PROY_DET.Username = ID;
                    API_USUARIOS_PROY_DET.ProyID = pID;
                    API_USUARIOS_PROY_DET.DetID = pdID;
                    API_CLS.API_USUARIOS_PROY_DET.Add(API_USUARIOS_PROY_DET);
                    API_CLS.SaveChanges();
                }
                if (ope == 2)
                {
                    API_USUARIOS_PROY_DET API_USUARIOS_PROY_DET = API_CLS.API_USUARIOS_PROY_DET.Find(ID, pID, pdID);
                    API_CLS.API_USUARIOS_PROY_DET.Remove(API_USUARIOS_PROY_DET);
                    API_CLS.SaveChanges();
                }
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #endregion

        #endregion
    }
}