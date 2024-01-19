using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_LIB.Model.GET_POINT.GP_CLS;
using API_WEB_DOC.Controllers.util;

namespace API_WEB_DOC.Controllers
{
    public class DefaultController : Controller
    {
        API_CLS API_CLS = new API_CLS();
        API_ENT API_ENT = new API_ENT();
        GP_CLS GP_CLS = new GP_CLS();
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
        public ActionResult I(string t="", string s = "")
        {
            try
            {
                ViewBags();
                ViewBag.ERR = "0";

                string token = ConfigurationManager.AppSettings["Api_Token"];
                string secret = ConfigurationManager.AppSettings["Api_Secret"];

                API_USUARIOS API_USUARIOS = API_CLS.API_USUARIOS.Where(m => m.API_TOKEN == token && m.API_SECRET == secret).FirstOrDefault();

                try
                {
                    if (API_USUARIOS.IndValVigFecha)//VIGENCIA + VALIDA FECHA DE VIGENCIA DESDE HASTA
                    {
                        if (API_USUARIOS.Estado == 1)
                        {
                            DateTime from = API_USUARIOS.VigenciaD.Date;
                            DateTime to = API_USUARIOS.VigenciaH.Date;
                            DateTime now = DateTime.Now.Date;

                            if (from <= now && now <= to)
                            {
                                //OK
                                MODELS.BASE_LOGIN BASE_LOGIN = new MODELS.BASE_LOGIN();
                                Usuario Usuario = GP_CLS.Usuario.Where(m => m.UserName == API_USUARIOS.Username).FirstOrDefault();
                                Empresa Empresa = GP_CLS.Empresa.Where(m => m.Rut == Usuario.RutEmpresa).FirstOrDefault();
                                BASE_LOGIN.USUARIO = Usuario;
                                BASE_LOGIN.EMPRESA = Empresa;
                                Session[VARS.VARS_SESSION] = BASE_LOGIN;
                                Session.Timeout = 99999;
                                return RedirectToAction("I", "H");
                                //OK
                            }
                            else { throw new Exception(); }
                        }
                        else { throw new Exception(); }
                    }
                    else //VALIDA SOLO ESTADO
                    {
                        if (API_USUARIOS.Estado == 1)
                        {
                            //OK
                            MODELS.BASE_LOGIN BASE_LOGIN = new MODELS.BASE_LOGIN();
                            Usuario Usuario = GP_CLS.Usuario.Where(m => m.UserName == API_USUARIOS.Username).FirstOrDefault();
                            Empresa Empresa = GP_CLS.Empresa.Where(m => m.Rut == Usuario.RutEmpresa).FirstOrDefault();
                            BASE_LOGIN.USUARIO = Usuario;
                            BASE_LOGIN.EMPRESA = Empresa;
                            Session[VARS.VARS_SESSION] = BASE_LOGIN;
                            Session.Timeout = 99999;
                            return RedirectToAction("I", "H");
                            //OK


                        }
                        else { throw new Exception(); }
                    }
                }
                catch 
                {
                    ViewBag.ERR = "NO SE HAN ENCONTRADO DATOS";
                    return View();
                }
                //return View();
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
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