using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text.RegularExpressions;
using System.Text;

using API_GP_LOGISTICO.Controllers.util;
using static API_GP_LOGISTICO.Controllers.util.MODELS;

using API_LIB.Model.API.API_CLS;
using API_LIB.Model.API.API_ENT;
using API_LIB.Model.GET_POINT.GP_CLS;
using API_LIB.Model.GET_POINT.GP_ENT;
using API_LIB.Model.GET_POINT.MD;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

namespace API_GP_LOGISTICO.Controllers
{
    [RoutePrefix("API")]
    public class CallController : ApiController
    {
        private API_CLS API_CLS = new API_CLS();
        private API_ENT API_ENT = new API_ENT();

        private GP_CLS GP_CLS = new GP_CLS();
        private GP_ENT GP_ENT = new GP_ENT();

        private CONFIGURATION CONFIGURATION = new CONFIGURATION();
        private ACCESS ACCESS = new ACCESS();
        private API_RESPONSE_ERRORS ERROR = new API_RESPONSE_ERRORS();

        #region CREAR PICKUP (1)
        [HttpPost]
        [Route("PREINGRESOPICKUP/CREAR")]
        public IHttpActionResult recurso1([FromBody] API_REQUEST_TYPE_2_PICKUP REQUEST)
        {
            API_RESPONSE_TYPE_2 RESPONSE = new API_RESPONSE_TYPE_2();
            API_RESPONSE_TYPE_12 RESPONSEDET = new API_RESPONSE_TYPE_12();

            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_1;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje;
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje + ". " + MensajeError.Trim();
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje;
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSEDET);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSEDET.resultado = true;
            RESPONSEDET.resultado_descripcion = "";
            RESPONSEDET.resultado_codigo = 0;
            RESPONSEDET.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.numeropedido = (REQUEST.numeropedido == null ? "" : REQUEST.numeropedido);
            REQUEST.rutcliente = (REQUEST.rutcliente == null ? "" : REQUEST.rutcliente);
            REQUEST.nombrecliente = (REQUEST.nombrecliente == null ? "" : REQUEST.nombrecliente);
            REQUEST.email = (REQUEST.email == null ? "" : REQUEST.email);
            REQUEST.ruttercero = (REQUEST.ruttercero == null ? "" : REQUEST.ruttercero);
            REQUEST.nombretercero = (REQUEST.nombretercero == null ? "" : REQUEST.nombretercero);
            REQUEST.emailtercero = (REQUEST.emailtercero == null ? "" : REQUEST.emailtercero);
            REQUEST.usuariodig = (REQUEST.usuariodig == null ? "" : REQUEST.usuariodig);
            REQUEST.direccion = (REQUEST.direccion == null ? "" : REQUEST.direccion);
            REQUEST.comuna = (REQUEST.comuna == null ? "" : REQUEST.comuna);
            REQUEST.telefono = (REQUEST.telefono == null ? "" : REQUEST.telefono);
            REQUEST.glosa = (REQUEST.glosa == null ? "" : REQUEST.glosa);
            foreach (var item in REQUEST.items)
            {
                item.codigoarticulo = (item.codigoarticulo == null ? "" : item.codigoarticulo);
                item.descripart = (item.descripart == null ? "" : item.descripart);
            }
            #region LARGOS PERMITIDOS
            try
            {
                if (REQUEST.numeropedido.Trim().Length > 20) { throw new Exception("ERROR - NUMEROPEDIDO > QUE EL LARGO PERMITIDO 20"); }
                if (REQUEST.rutcliente.Trim().Length > 12) { throw new Exception("ERROR - RUTCLIENTE  > QUE EL LARGO PERMITIDO 12"); }
                if (REQUEST.nombrecliente.Trim().Length > 50) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 50"); }
                if (REQUEST.email.Trim().Length > 50) { throw new Exception("ERROR - EMAIL  > QUE EL LARGO PERMITIDO 50"); }
                if (REQUEST.ruttercero.Trim().Length > 12) { throw new Exception("ERROR - RUTTERCERO > QUE EL LARGO PERMITIDO 12"); }
                if (REQUEST.nombretercero.Trim().Length > 50) { throw new Exception("ERROR - NOMBRETERCERO > QUE EL LARGO PERMITIDO 50"); }
                if (REQUEST.emailtercero.Trim().Length > 50) { throw new Exception("ERROR - EMAILTERCERO > QUE EL LARGO PERMITIDO 50"); }
                if (REQUEST.usuariodig.Trim().Length > 15) { throw new Exception("ERROR - USUARIODIG > QUE EL LARGO PERMITIDO 15"); }

                if (REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).Count() > 0) { throw new Exception("ERROR - ITEM.CodigoArticulo  > QUE EL LARGO PERMITIDO 20, LINEA NRO." + REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).First().linea); }
                if (REQUEST.items.Where(m => m.descripart.Trim().Length > 50).Count() > 0) { throw new Exception("ERROR - ITEM.DESCRIPART   > QUE EL LARGO PERMITIDO 50, LINEA NRO." + REQUEST.items.Where(m => m.descripart.Trim().Length > 50).First().linea); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_PickUp_Result> 
                PICKUP = GP_ENT.sp_in_API_PickUp(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
                                                ,REQUEST.empid
                                                ,REQUEST.numeropedido
                                                ,REQUEST.fechacompra
                                                ,REQUEST.rutcliente
                                                ,REQUEST.nombrecliente
                                                ,REQUEST.email
                                                ,REQUEST.tipodocumento
                                                ,REQUEST.numerodocto
                                                ,REQUEST.fechadocto
                                                ,(decimal)REQUEST.montocompra
                                                ,REQUEST.ruttercero
                                                ,REQUEST.nombretercero
                                                ,REQUEST.emailtercero
                                                ,REQUEST.usuariodig
                                                ,REQUEST.tiposolicitud
                                                ,REQUEST.direccion
                                                ,REQUEST.comuna
                                                ,REQUEST.glosa
                                                ,REQUEST.telefono).ToList();

            if (PICKUP.Where(m => m.GenPickId > 0).Count() > 0)
            {
                RESPONSE.pickUP = (long)PICKUP.ElementAt(0).GenPickId;
                RESPONSEDET.PickId = (int)RESPONSE.pickUP;

                foreach (var item in REQUEST.items.ToList())
                {
                    List<sp_in_API_PickUpDet_Result> 
                        PICKUP_DET = GP_ENT.sp_in_API_PickUpDet(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
                                                               ,REQUEST.empid
                                                               ,(int)RESPONSE.pickUP
                                                               ,item.linea
                                                               ,item.codigoarticulo
                                                               ,item.descripart
                                                               ,item.cantidad
                                                               ,item.HUId
                                                               ,item.peso
                                                               ,item.volumen).ToList();

                    RESPONSEDET.items = PICKUP_DET.Select(m => new {
                                                                    PickDetId = m.PickDetId,
                                                                    Bulto = m.Bulto,
                                                                    Cantidad = m.Cantidad,
                                                                   }).Cast<dynamic>().ToList();
                }

                //return new HttpActionResult(Request, STATUS_CODE, RESPONSEDET);
            }
            else if (PICKUP.Where(m => m.GenPickId == 0).Count() > 0)
            {
                #region NOK
                ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, Estado = 1, IndTipo = false, Mensaje = PICKUP.ElementAt(0).Glosa.ToUpper(), Nombre = "ERROR PERSONALIZADO" };
                // ERROR = API_CLS.API_RESPONSE_ERRORS.Find(2000); //REQUEST - PICKUP ERROR DUPLICIDAD
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje;
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSEDET);
                #endregion
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje;
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSEDET);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSEDET);
            #endregion

        }
        #endregion

        #region LISTAR EMPRESA (2)
        [HttpGet]
        [HttpPost]
        [Route("EMPRESA/LISTAR")]
        public IHttpActionResult recurso2(API_REQUEST_TYPE_1 REQUEST)
        {
            API_RESPONSE_TYPE_1 RESPONSE = new API_RESPONSE_TYPE_1();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_2;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = true;
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            List<sp_sel_API_EmpresasList_Result> model = GP_ENT.sp_sel_API_EmpresasList(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
                                                                                       ,REQUEST.limit
                                                                                       ,REQUEST.rowset).ToList();   

            foreach (var item in model) 
            { 
                HELPERS.TrimModelProperties(typeof(sp_sel_API_EmpresasList_Result), item); 
            }
            if (model.Count() > 0) 
            { 
                RESPONSE.count = (int)model.ElementAt(0).count; 
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            RESPONSE.items = model.Select(m => new {
                                                    Id = m.Id,
                                                    EmpId = m.EmpId,
                                                    RutEmpresa = m.RutEmpresa,
                                                    CodigoExt = m.CodigoExt,
                                                    RazonSocial = m.RazonSocial,
                                                    NombreFantasia = m.NombreFantasia,
                                                    Direccion = m.Direccion,
                                                    Email = m.Email
                                                   }).Cast<dynamic>().ToList();

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        //Listado de recepciones de productos confirmadas ----
        #region LISTAR CONFIRMACIÓN DE RECEPCIÓN (3)
        [HttpGet]
        [HttpPost]
        [Route("CONFRECEPCION/LISTAR")]
        public IHttpActionResult recurso3(API_REQUEST_TYPE_3 REQUEST)
        {
            API_RESPONSE_TYPE_3 RESPONSE = new API_RESPONSE_TYPE_3();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_3;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empID, USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            DateTime fd = new DateTime();
            DateTime fh = new DateTime();

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = true;
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            #region VALIDACIONES DE CAMPOS
            REQUEST.fechaInicio = (REQUEST.fechaInicio == null ? "" : REQUEST.fechaInicio);
            REQUEST.fechaTermino = (REQUEST.fechaTermino == null ? "" : REQUEST.fechaTermino);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.numeroReferencia = (REQUEST.numeroReferencia == null ? "" : REQUEST.numeroReferencia);
            REQUEST.rutProveedor = (REQUEST.rutProveedor == null ? "" : REQUEST.rutProveedor);
            #region LARGOS PERMITIDOS
            try
            {

                try
                {
                    //fd = DateTime.Parse(REQUEST.fechaInicio);
                    fd = DateTime.ParseExact(REQUEST.fechaInicio, "dd-MM-yyyy", null);
                }
                catch { throw new Exception("ERROR - FECHAINICIO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 11-06-2020"); }

                try
                { //fh = DateTime.Parse(REQUEST.fechaTermino);
                    DateTime.TryParseExact(REQUEST.fechaTermino, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out fh);
                }
                catch { throw new Exception("ERROR - FECHATERMINO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 11-06-2020"); }

                if (REQUEST.tipoReferencia.Trim().Length > 80) { throw new Exception("ERROR - TIPOREFERENCIA > QUE EL LARGO PERMITIDO 80"); }
                if (REQUEST.numeroReferencia.Trim().Length > 40) { throw new Exception("ERROR - NUMEROREFERENCIA  > QUE EL LARGO PERMITIDO 40"); }
                if (REQUEST.rutProveedor.Trim().Length > 12) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 12"); }
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion
            #endregion

            #region PROCESAMIENTO

            List<sp_sel_API_Confirma_Recep_Result> 
                model_response = GP_ENT.sp_sel_API_Confirma_Recep(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
                                                                                            ,REQUEST.empID
                                                                                            ,fd
                                                                                            ,fh
                                                                                            ,REQUEST.solRecepID
                                                                                            ,REQUEST.tipoReferencia
                                                                                            ,REQUEST.numeroReferencia
                                                                                            ,REQUEST.rutProveedor
                                                                                            ,REQUEST.limit
                                                                                            ,REQUEST.rowset).ToList();

            if (model_response.Count() > 0) { RESPONSE.count = (long)model_response.ElementAt(0).Counter; }
            else { RESPONSE.count = 0; }

            List<sp_sel_API_Confirma_Recep_Result_MODEL> CABECERAS = new List<sp_sel_API_Confirma_Recep_Result_MODEL>();

            foreach (var item_recepciones in model_response.GroupBy(m => m.RecepcionId).Distinct().ToList())
            {
                sp_sel_API_Confirma_Recep_Result_MODEL CAB = new sp_sel_API_Confirma_Recep_Result_MODEL()
                {
                    Id = item_recepciones.ElementAt(0).Id,
                    RecepcionId = item_recepciones.ElementAt(0).RecepcionId,
                    INT_NAME = item_recepciones.ElementAt(0).INT_NAME,
                    FECHA_HORA = item_recepciones.ElementAt(0).FECHA_HORA,
                    SolRecepId = long.Parse(item_recepciones.ElementAt(0).SolRecepId.ToString()),
                    FechaProceso = item_recepciones.ElementAt(0).FechaProceso,
                    TipoDocumento = item_recepciones.ElementAt(0).TipoDocumento,
                    NumeroDocto = item_recepciones.ElementAt(0).NumeroDocto,
                    FechaDocto = item_recepciones.ElementAt(0).FechaDocto,
                    TipoReferencia = item_recepciones.ElementAt(0).TipoReferencia,
                    NumeroReferencia = item_recepciones.ElementAt(0).NumeroReferencia,
                    FechaReferencia = item_recepciones.ElementAt(0).FechaReferencia,
                    // Counter = item_recepciones.ElementAt(0).Counter,
                    RutProveedor = item_recepciones.ElementAt(0).RutProveedor,
                    GlosaRdm = item_recepciones.ElementAt(0).GlosaRdm
                };
                HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Recep_Result_MODEL), CAB);

                CAB.items = new List<sp_sel_API_Confirma_Recep_Result_MODEL_DET>();

                foreach (var item_recepciones_det in model_response.Where(m => CAB.RecepcionId == m.RecepcionId2).ToList().OrderBy(m => m.Linea))
                {
                    sp_sel_API_Confirma_Recep_Result_MODEL_DET DET = new sp_sel_API_Confirma_Recep_Result_MODEL_DET()
                    {
                        Linea = item_recepciones_det.Linea,
                        CodigoArticulo = item_recepciones_det.CodigoArticulo,
                        CodigoOriginal = item_recepciones_det.CodigoOriginal,
                        UnidadCompra = item_recepciones_det.UnidadCompra,
                        CantidadSolicitada = item_recepciones_det.CantidadSolicitada,
                        ItemReferencia = item_recepciones_det.ItemReferencia,
                        LoteSerie = item_recepciones_det.LoteSerie,
                        FechaVencto = item_recepciones_det.FechaVencto,
                        CantidadRecibida = item_recepciones_det.CantidadRecibida,
                        HuId = long.Parse(item_recepciones_det.HuId.ToString()),
                        Estado = int.Parse(item_recepciones_det.Estado.ToString())
                    };
                    HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Recep_Result_MODEL_DET), DET);
                    CAB.items.Add(DET);
                }
                CABECERAS.Add(CAB);
            }

            RESPONSE.cabeceras = CABECERAS.Cast<dynamic>().ToList();
            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //Listado de recepciones de despacho confirmadas ---
        #region LISTAR CONFIRMACIÓN DE DESPACHO (4)
        [HttpGet]
        [HttpPost]
        [Route("CONFDESPACHO/LISTAR")]
        public IHttpActionResult recurso4(API_REQUEST_TYPE_4 REQUEST)
        {
            API_RESPONSE_TYPE_3 RESPONSE = new API_RESPONSE_TYPE_3();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_4;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empID, USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            DateTime fd = new DateTime();
            DateTime fh = new DateTime();

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = true;
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            #region VALIDACIONES DE CAMPOS

            REQUEST.fechaInicio = (REQUEST.fechaInicio == null ? "" : REQUEST.fechaInicio);
            REQUEST.fechaTermino = (REQUEST.fechaTermino == null ? "" : REQUEST.fechaTermino);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.numeroReferencia = (REQUEST.numeroReferencia == null ? "" : REQUEST.numeroReferencia);
            REQUEST.rutProveedor = (REQUEST.rutProveedor == null ? "" : REQUEST.rutProveedor);

            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    //fd = DateTime.Parse(REQUEST.fechaInicio);
                    fd = DateTime.ParseExact(REQUEST.fechaInicio, "dd-MM-yyyy", null);
                }
                catch { throw new Exception("ERROR - FECHAINICIO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 11-06-2020"); }

                try
                { //fh = DateTime.Parse(REQUEST.fechaTermino);
                    DateTime.TryParseExact(REQUEST.fechaTermino, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out fh);
                }
                catch { throw new Exception("ERROR - FECHATERMINO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 11-06-2020"); }

                if (REQUEST.tipoReferencia.Trim().Length > 80) { throw new Exception("ERROR - TIPOREFERENCIA > QUE EL LARGO PERMITIDO 80"); }
                if (REQUEST.numeroReferencia.Trim().Length > 40) { throw new Exception("ERROR - NUMEROREFERENCIA  > QUE EL LARGO PERMITIDO 40"); }
                if (REQUEST.rutProveedor.Trim().Length > 12) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 12"); }
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #endregion

            #region PROCESAMIENTO
            List<sp_sel_API_Confirma_Desp_Result> 
                model_response = GP_ENT.sp_sel_API_Confirma_Desp(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault(),
                                                                 REQUEST.empID,
                                                                 fd,
                                                                 fh,
                                                                 REQUEST.SolDespId,
                                                                 REQUEST.tipoReferencia,
                                                                 REQUEST.numeroReferencia,
                                                                 REQUEST.rutProveedor,
                                                                 REQUEST.limit,
                                                                 REQUEST.rowset).ToList();
            if (model_response.Count() > 0)
            {
                RESPONSE.count = (long)model_response.ElementAt(0).Counter;
            }
            else
            {
                RESPONSE.count = 0;
            }

            List<sp_sel_API_Confirma_Desp_Result_MODEL> CABECERAS = new List<sp_sel_API_Confirma_Desp_Result_MODEL>();

            foreach (var item_despachos in model_response.GroupBy(m => m.ColaPickId).Distinct().ToList())
            {
                sp_sel_API_Confirma_Desp_Result_MODEL CAB = new sp_sel_API_Confirma_Desp_Result_MODEL()
                {
                    Id = item_despachos.ElementAt(0).Id,
                    ColaPickId = item_despachos.ElementAt(0).ColaPickId,
                    INT_NAME = item_despachos.ElementAt(0).INT_NAME,
                    FECHA_HORA = item_despachos.ElementAt(0).FECHA_HORA,
                    SolDespId = long.Parse(item_despachos.ElementAt(0).SolDespId.ToString()),
                    FechaProceso = item_despachos.ElementAt(0).FechaProceso,
                    TipoDocumento = item_despachos.ElementAt(0).TipoDocumento,
                    NumeroDocto = item_despachos.ElementAt(0).NumeroDocto,
                    FechaDocto = item_despachos.ElementAt(0).FechaDocto,
                    TipoReferencia = item_despachos.ElementAt(0).TipoReferencia,
                    NumeroReferencia = item_despachos.ElementAt(0).NumeroReferencia,
                    FechaReferencia = item_despachos.ElementAt(0).FechaReferencia,
                    //Counter = item_despachos.ElementAt(0).Counter,
                    RutCliente = item_despachos.ElementAt(0).RutCliente
                };
                HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Desp_Result_MODEL), CAB);
                CAB.Items = new List<sp_sel_API_Confirma_Desp_Result_MODEL_DET>();

                foreach (var item_despachos_det in model_response.Where(m => CAB.ColaPickId == m.ColaPickId2).ToList().OrderBy(m => m.Linea))
                {
                    sp_sel_API_Confirma_Desp_Result_MODEL_DET DET = new sp_sel_API_Confirma_Desp_Result_MODEL_DET()
                    {
                        Linea = item_despachos_det.Linea,
                        CodigoArticulo = item_despachos_det.CodigoArticulo,
                        CodigoOriginal = item_despachos_det.CodigoOriginal,
                        UnidadVenta = item_despachos_det.UnidadVenta,
                        CantidadSolicitada = item_despachos_det.CantidadSolicitada,
                        ItemReferencia = item_despachos_det.ItemReferencia,
                        LoteSerieSol = item_despachos_det.LoteSerieSol,
                        FecVenctoSol = item_despachos_det.FecVenctoSol,
                        CantidadDespachada = item_despachos_det.CantidadDespachada,
                        LoteSerieDesp = item_despachos_det.LoteSerieDesp,
                        FecVectoDesp = item_despachos_det.FecVectoDesp
                    };
                    HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Desp_Result_MODEL_DET), DET);
                    CAB.Items.Add(DET);
                }
                CABECERAS.Add(CAB);

            }

            RESPONSE.cabeceras = CABECERAS.Cast<dynamic>().ToList();
            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //INSERTA en tabla L_Integraciones un texto separado por ; ---------------
        #region CREAR INTEGRACIONES (5)
        [HttpPost]
        [Route("INTEGRACIONES/CREAR")]
        public IHttpActionResult recurso5([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_RESPONSE_TYPE_4_OK RESPONSE_OK = new API_RESPONSE_TYPE_4_OK();
            API_RESPONSE_TYPE_4_ERROR RESPONSE_ERROR = new API_RESPONSE_TYPE_4_ERROR();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "INTEGRACIONES/CREAR";

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_5;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje + ". " + MensajeError.Trim();
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            }
            #endregion

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE_OK.resultado = true;
            RESPONSE_OK.resultado_descripcion = "";
            RESPONSE_OK.resultado_codigo = 0;
            RESPONSE_OK.count = 1;

            #region VALIDACIONES DE CAMPOS
            REQUEST.archivo = (REQUEST.archivo == null ? "" : REQUEST.archivo);
            REQUEST.txt = (REQUEST.txt == null ? "" : REQUEST.txt);

            #region LARGOS PERMITIDOS
            //try
            //{
            //    if (REQUEST.key.Trim().Length > 15) { throw new Exception("ERROR - KEY > QUE EL LARGO PERMITIDO 20"); }
            //}
            //catch (Exception ex)
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
            //    RESPONSE.resultado = false;
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion

            #region PROCESAMIENTO 

            //string serialize = "";

            try
            {
                string[] Campos = REQUEST.txt.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                int c = 1;
                string Datos = "";
                string SeparadorCampo = "¬"; //alt 170
                string SeparadorLinea = "«"; //alt 174

                foreach (string Campo in Campos)
                {
                    //Si ya tiene lineas previas agrega caracter separador de Lineas
                    if (Datos.Trim() != "")
                    {
                        Datos = Datos.Trim() + SeparadorLinea.Trim();
                    }

                    Datos = Datos + c.ToString() + SeparadorCampo.Trim() + Campo.Replace(";", SeparadorCampo).Trim();

                    //Cada 1000 lineas inserta en tabla de integracion ------------------------------------------
                    if (c % 1000 == 0)
                    {
                        var model = GP_ENT.sp_in_API_Integraciones_Masivo(REQUEST.archivo,
                                                                          USERNAME.Trim(),
                                                                          DateTime.Now,
                                                                          Datos).ToList();
                        Datos = "";
                    }

                    c = c + 1;
                }

                //Si salio del ciclo y quedaron Items por insertar en tabla de integracion --------------------------------
                if (Datos != "")
                {
                    var model = GP_ENT.sp_in_API_Integraciones_Masivo(REQUEST.archivo,
                                                                      USERNAME.Trim(),
                                                                      DateTime.Now,
                                                                      Datos).ToList();
                    Datos = "";
                }

                //var model = GP_ENT.sp_in_API_Integracion(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault(),
                //                                         REQUEST.empid,
                //                                         REQUEST.archivo,
                //                                         REQUEST.txt).ToList();

                //serialize = JsonConvert.SerializeObject(model);

            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - VALIDACION CONSISTENCIA DE DATOS

                //LogInfo(NombreProceso, ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, "", body.ToString());

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE_ERROR.resultado = false;
                //RESPONSE_ERROR.Descripcion = ex.Message.Trim();
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje + ". " + ex.Message.Trim() + ". " + ex.InnerException.ToString().Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE_ERROR);

                #endregion
            }

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_proc_API_Integraciones_Result> INTEGRACIONES_PROCESA = new List<sp_proc_API_Integraciones_Result>();

            //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
            INTEGRACIONES_PROCESA = GP_ENT.sp_proc_API_Integraciones(REQUEST.archivo,
                                                                     USERNAME).ToList();
            if (INTEGRACIONES_PROCESA.Count > 0)
            {
                if (INTEGRACIONES_PROCESA[0].Resultado == "OK")
                {
                    RESPONSE_OK.resultado = true;
                    RESPONSE_OK.descripcion = INTEGRACIONES_PROCESA[0].Descripcion;
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].Descripcion,
                            true,
                            false,
                            "",
                            "");

                    RESPONSE_ERROR.resultado = false;
                    //RESPONSE_ERROR.descripcion = INTEGRACIONES_PROCESA[0].Descripcion;
                    RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                    RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE_ERROR);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        false,
                        "",
                        "");

                RESPONSE_ERROR.resultado = false;
                //RESPONSE_ERROR.descripcion = "";
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE_ERROR);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente todo el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE_OK);


            //var model = GP_ENT.Sp_Proc_IntegracionApi(REQUEST.archivo,
            //                                          USERNAME.Trim(),
            //                                          DateTime.Now);

            //serialize = JsonConvert.SerializeObject(model);

            //List<sp_in_API_Integracion_Result> INTEGRACIONES = JsonConvert.DeserializeObject<List<sp_in_API_Integracion_Result>>(serialize);

            ////List<sp_in_API_Integracion_Result> INTEGRACIONES = null;

            //if (INTEGRACIONES.Count == 0)
            //{
            //    ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, 
            //                                        Estado = 1, 
            //                                        IndTipo = false, 
            //                                        Mensaje = "Error al obtener información", 
            //                                        Nombre = "ERROR PERSONALIZADO" };

            //    RESPONSE_ERROR.resultado = false;
            //    RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
            //    RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            //}

            //else if (INTEGRACIONES.Where(m => m.Generacion > 0).Count() > 0)
            ////if (INTEGRACIONES[0].Generacion > 0)
            //{
            //    RESPONSE_OK.id = (int)INTEGRACIONES.ElementAt(0).Generacion;
            //    RESPONSE_OK.descripcion = (string)INTEGRACIONES.ElementAt(0).GlosaEstado;
            //}
            //else if (INTEGRACIONES.Where(m => m.Generacion == 0).Count() > 0)
            ////else if (INTEGRACIONES[0].Generacion == 0)
            //{
            //    #region NOK
            //    ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, Estado = 1, IndTipo = false, Mensaje = INTEGRACIONES.ElementAt(0).GlosaEstado.ToUpper(), Nombre = "ERROR PERSONALIZADO" };
            //    // ERROR = API_CLS.API_RESPONSE_ERRORS.Find(2000); //REQUEST - PICKUP ERROR DUPLICIDAD
            //    RESPONSE_ERROR.resultado = false;
            //    RESPONSE_ERROR.resultado_descripcion = "ERROR";
            //    RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
            //    RESPONSE_ERROR.count = 0;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;

            //    foreach (var item in INTEGRACIONES) { HELPERS.TrimModelProperties(typeof(sp_in_API_Integracion_Result), item); }
            //    //if (model.Count() > 0) { RESPONSE.count = (int)model.ElementAt(0).count; }
            //    //else { RESPONSE.count = 0; }

            //    RESPONSE_ERROR.errores = INTEGRACIONES
            //    .Select(m => new
            //    {
            //        Descripcion = m.GlosaEstado
            //    })
            //    .Cast<dynamic>().ToList();


            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            //    #endregion
            //}
            //else
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
            //    RESPONSE_ERROR.resultado = false;
            //    RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
            //    RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            //    #endregion
            //}

            //return new HttpActionResult(Request, STATUS_CODE, RESPONSE_OK);
            #endregion
        }
        #endregion

        #region CREAR PRODUCTO (6)
        [HttpPost]
        [Route("PRODUCTO/CREAR")]
        public IHttpActionResult recurso6([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_RESPONSE_TYPE_5 RESPONSE = new API_RESPONSE_TYPE_5();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = false;
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = true;
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.txt = (REQUEST.txt == null ? "" : REQUEST.txt);
            #endregion


            List<sp_in_API_CrearProducto_Result> PRODUCTO = GP_ENT.sp_in_API_CrearProducto(
            REQUEST.empid
            , REQUEST.txt).ToList();

            //if (INTEGRACIONES.Where(m => m.Id > 0).Count() > 0)
            if (PRODUCTO[0].Contador > 0)
            {
                RESPONSE.contador = (int)PRODUCTO.ElementAt(0).Contador;
                RESPONSE.estado = (string)PRODUCTO.ElementAt(0).Estado;
                RESPONSE.descripcion = (string)PRODUCTO.ElementAt(0).Descripcion;
            }
            //else if (INTEGRACIONES.Where(m => m.Id == 0).Count() > 0)
            else if (PRODUCTO[0].Contador == 0)
            {
                #region NOK
                ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, Estado = 1, IndTipo = false, Mensaje = PRODUCTO.ElementAt(0).Estado.ToUpper(), Nombre = "ERROR PERSONALIZADO" };
                // ERROR = API_CLS.API_RESPONSE_ERRORS.Find(2000); //REQUEST - PICKUP ERROR DUPLICIDAD
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR PRODUCTO (7)
        [HttpGet]
        [HttpPost]
        [Route("PRODUCTO/LISTAR")]
        public IHttpActionResult recurso7(API_REQUEST_TYPE_7 REQUEST)
        {
            API_RESPONSE_TYPE_6 RESPONSE = new API_RESPONSE_TYPE_6();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_7;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            List<sp_sel_API_ListarProducto_Result> model = GP_ENT.sp_sel_API_ListarProducto(REQUEST.empid,
                                                                                            REQUEST.codigoarticulo,
                                                                                            REQUEST.descripcion,
                                                                                            REQUEST.lineaproducto,
                                                                                            REQUEST.tipoproducto,
                                                                                            REQUEST.ean13,
                                                                                            REQUEST.dun14,
                                                                                            REQUEST.limit,
                                                                                            REQUEST.rowset
                                                                                            ).ToList();

            foreach (var item in model) 
            { 
                HELPERS.TrimModelProperties(typeof(sp_sel_API_ListarProducto_Result), item); 
            }
            
            if (model.Count() > 0) 
            { 
                RESPONSE.count = (int)model.ElementAt(0).Count; 
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            RESPONSE.items = model.Select(m => new {
                                                    Id = m.Id,
                                                    EmpId = m.EmpId,
                                                    CodigoArticulo = m.CodigoArticulo,
                                                    DescripArt = m.DescripArt,
                                                    UnidadMedida = m.UnidadMedida,
                                                    LineaProducto = m.LineaProducto,
                                                    TipoModelo = m.TipoModelo,
                                                    Version = m.Version,
                                                    Tipo = m.Tipo,
                                                    Marca = m.Marca,
                                                    EAN13 = m.EAN13,
                                                    DUN14 = m.DUN14
                                                   })
            .Cast<dynamic>().ToList();
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region CREAR SOLICITUD RECEPCIÓN (8)
        [HttpPost]
        [Route("SOLRECEP/CREAR")]
        public IHttpActionResult recurso8([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_RESPONSE_TYPE_7 RESPONSE = new API_RESPONSE_TYPE_7();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_8;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.txt = (REQUEST.txt == null ? "" : REQUEST.txt);

            #region LARGOS PERMITIDOS
            //try
            //{
            //    if (REQUEST.numeropedido.Trim().Length > 20) { throw new Exception("ERROR - NUMEROPEDIDO > QUE EL LARGO PERMITIDO 20"); }
            //    if (REQUEST.rutcliente.Trim().Length > 12) { throw new Exception("ERROR - RUTCLIENTE  > QUE EL LARGO PERMITIDO 12"); }
            //    if (REQUEST.nombrecliente.Trim().Length > 50) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.email.Trim().Length > 50) { throw new Exception("ERROR - EMAIL  > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.ruttercero.Trim().Length > 12) { throw new Exception("ERROR - RUTTERCERO > QUE EL LARGO PERMITIDO 12"); }
            //    if (REQUEST.nombretercero.Trim().Length > 50) { throw new Exception("ERROR - NOMBRETERCERO > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.emailtercero.Trim().Length > 50) { throw new Exception("ERROR - EMAILTERCERO > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.usuariodig.Trim().Length > 15) { throw new Exception("ERROR - USUARIODIG > QUE EL LARGO PERMITIDO 15"); }

            //    if (REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).Count() > 0) { throw new Exception("ERROR - ITEM.CodigoArticulo  > QUE EL LARGO PERMITIDO 20, LINEA NRO." + REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).First().linea); }
            //    if (REQUEST.items.Where(m => m.descripart.Trim().Length > 50).Count() > 0) { throw new Exception("ERROR - ITEM.DESCRIPART   > QUE EL LARGO PERMITIDO 50, LINEA NRO." + REQUEST.items.Where(m => m.descripart.Trim().Length > 50).First().linea); }
            //}
            //catch (Exception ex)
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
            //    RESPONSE.resultado = false;
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion

            List<sp_in_API_SolRecep_Result> SOLRECEP = GP_ENT.sp_in_API_SolRecep(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
                                                                                ,REQUEST.empid
                                                                                ,REQUEST.txt).ToList();

            //if (INTEGRACIONES.Where(m => m.Id > 0).Count() > 0)
            if (SOLRECEP[0].Count > 0)
            {
                RESPONSE.count = (int)SOLRECEP.ElementAt(0).Count;
                RESPONSE.solRecepId = (int)SOLRECEP.ElementAt(0).SolRecepId;
                RESPONSE.descripcion = (string)SOLRECEP.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        #region CREAR SOLICITUD DESPACHO (9)
        [HttpPost]
        [Route("SOLDESP/CREAR")]
        public IHttpActionResult recurso9([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_RESPONSE_TYPE_8 RESPONSE = new API_RESPONSE_TYPE_8();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.txt = (REQUEST.txt == null ? "" : REQUEST.txt);

            #region LARGOS PERMITIDOS
            //try
            //{
            //    if (REQUEST.numeropedido.Trim().Length > 20) { throw new Exception("ERROR - NUMEROPEDIDO > QUE EL LARGO PERMITIDO 20"); }
            //    if (REQUEST.rutcliente.Trim().Length > 12) { throw new Exception("ERROR - RUTCLIENTE  > QUE EL LARGO PERMITIDO 12"); }
            //    if (REQUEST.nombrecliente.Trim().Length > 50) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.email.Trim().Length > 50) { throw new Exception("ERROR - EMAIL  > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.ruttercero.Trim().Length > 12) { throw new Exception("ERROR - RUTTERCERO > QUE EL LARGO PERMITIDO 12"); }
            //    if (REQUEST.nombretercero.Trim().Length > 50) { throw new Exception("ERROR - NOMBRETERCERO > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.emailtercero.Trim().Length > 50) { throw new Exception("ERROR - EMAILTERCERO > QUE EL LARGO PERMITIDO 50"); }
            //    if (REQUEST.usuariodig.Trim().Length > 15) { throw new Exception("ERROR - USUARIODIG > QUE EL LARGO PERMITIDO 15"); }

            //    if (REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).Count() > 0) { throw new Exception("ERROR - ITEM.CodigoArticulo  > QUE EL LARGO PERMITIDO 20, LINEA NRO." + REQUEST.items.Where(m => m.codigoarticulo.Trim().Length > 20).First().linea); }
            //    if (REQUEST.items.Where(m => m.descripart.Trim().Length > 50).Count() > 0) { throw new Exception("ERROR - ITEM.DESCRIPART   > QUE EL LARGO PERMITIDO 50, LINEA NRO." + REQUEST.items.Where(m => m.descripart.Trim().Length > 50).First().linea); }
            //}
            //catch (Exception ex)
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
            //    RESPONSE.resultado = false;
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion
            List<sp_in_API_SolDesp_Result> SOLDESP = GP_ENT.sp_in_API_SolDesp(
            REQUEST.empid
            , REQUEST.txt).ToList();

            //if (INTEGRACIONES.Where(m => m.Id > 0).Count() > 0)
            if (SOLDESP[0].count > 0)
            {
                RESPONSE.count = (int)SOLDESP.ElementAt(0).count;
                RESPONSE.solDespId = (int)SOLDESP.ElementAt(0).SolDespId;
                RESPONSE.descripcion = (string)SOLDESP.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        #region LISTAR TRACKING SDD (10)
        [HttpGet]
        [HttpPost]
        [Route("TRACKINGSDD/LISTAR")]
        public IHttpActionResult recurso10(API_REQUEST_TYPE_8 REQUEST)
        {
            API_RESPONSE_TYPE_6 RESPONSE = new API_RESPONSE_TYPE_6();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.Limit;
            RESPONSE.rowset = REQUEST.Rowset;

            #region VALIDACIONES DE CAMPOS
            REQUEST.FechaInicio = (REQUEST.FechaInicio == null ? "" : REQUEST.FechaInicio);
            REQUEST.FechaTermino = (REQUEST.FechaTermino == null ? "" : REQUEST.FechaTermino);
            REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia);
            REQUEST.RutCliente = (REQUEST.RutCliente == null ? "" : REQUEST.RutCliente);
            #endregion

            #region LARGOS PERMITIDOS

            DateTime fi = new DateTime();
            DateTime ft = new DateTime();

            try
            {
                try
                {
                    if (REQUEST.FechaInicio.Equals(""))
                    {
                        fi = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fi = DateTime.ParseExact(REQUEST.FechaInicio, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch 
                { 
                    throw new Exception("ERROR - FECHAINICIO DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); 
                }

                try
                { //fh = DateTime.Parse(REQUEST.fechaTermino);
                    if (REQUEST.FechaTermino.Equals(""))
                    {
                        ft = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        ft = DateTime.ParseExact(REQUEST.FechaTermino, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch 
                { 
                    throw new Exception("ERROR - FECHATERMINO DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); 
                }

                if (REQUEST.TipoReferencia.Trim().Length > 80) 
                { 
                    throw new Exception("ERROR - TIPOREFERENCIA > QUE EL LARGO PERMITIDO 80"); 
                }

                if (REQUEST.RutCliente.Trim().Length > 12) 
                { 
                    throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 12"); 
                }
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region PROCESO

            List<sp_in_API_TrackingSDD_Result> TRACKINGSDD = GP_ENT.sp_in_API_TrackingSDD(REQUEST.EmpId,
                                                                                          fi,
                                                                                          ft,
                                                                                          REQUEST.SolDespId,
                                                                                          REQUEST.TipoReferencia,
                                                                                          REQUEST.NumeroReferencia,
                                                                                          REQUEST.RutCliente,
                                                                                          REQUEST.Limit,
                                                                                          REQUEST.Rowset
                                                                                          ).ToList();

            foreach (var item in TRACKINGSDD) 
            { 
                HELPERS.TrimModelProperties(typeof(sp_in_API_TrackingSDD_Result), item); 
            }

            if (TRACKINGSDD.Count() > 0) 
            { 
                RESPONSE.count = (int)TRACKINGSDD.ElementAt(0).Count; 
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            RESPONSE.items = TRACKINGSDD.Select(m => new {
                                                            SolDespId = m.SolDespId,
                                                            TipoReferencia = m.TipoReferencia,
                                                            NumeroReferencia = m.NumeroReferencia,
                                                            RutCliente = m.RutCliente,
                                                            Estado = m.Estado,
                                                            EstadoGlosa = m.EstadoGlosa,
                                                            FechaEstado = m.FechaEstado,
                                                            FechaDigitacion = m.FechaDigitacion,
                                                            Usuario = m.Usuario,
                                                            MotivoAnula = m.MotivoAnula,
                                                            DescripcionMotivoAnula = m.DescripcionMotivoAnula,
                                                            UsuarioAnula = m.UsuarioAnula,
                                                            GlosaAnula = m.GlosaAnula
                                                        }
                                                ).Cast<dynamic>().ToList();

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR TRACKING PICKING (21)
        [HttpGet]
        [HttpPost]
        [Route("TRACKINGCOURIER/LISTAR")]
        public IHttpActionResult recurso21(API_REQUEST_TYPE_16 REQUEST)
        {
            API_RESPONSE_TYPE_14 RESPONSE = new API_RESPONSE_TYPE_14();
            //API_REPONSE_TYPE_9 RESPONSEDET = new API_REPONSE_TYPE_9();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fi = new DateTime();
            DateTime ft = new DateTime();

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.userName = (REQUEST.userName == null ? "" : REQUEST.userName);
            REQUEST.fechaInicio = (REQUEST.fechaInicio == null ? "" : REQUEST.fechaInicio);
            REQUEST.fechaTermino = (REQUEST.fechaTermino == null ? "" : REQUEST.fechaTermino);
            REQUEST.rutCliente = (REQUEST.rutCliente == null ? "" : REQUEST.rutCliente);
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaInicio.Equals(""))
                    {
                        fi = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fi = DateTime.ParseExact(REQUEST.fechaInicio, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAINICIO DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }

                try
                { //fh = DateTime.Parse(REQUEST.fechaTermino);
                    if (REQUEST.fechaTermino.Equals(""))
                    {
                        ft = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        ft = DateTime.ParseExact(REQUEST.fechaTermino, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHATERMINO DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }

                if (REQUEST.rutCliente.Trim().Length > 12) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 12"); }
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion
            #endregion

            List<sp_sel_API_TrackingPicking_Result> TRACKING = GP_ENT.sp_sel_API_TrackingPicking(
            REQUEST.empId,
            USERNAME,
            fi,
            ft,
            REQUEST.estado,
            REQUEST.numeroRef,
            REQUEST.numeroDoc,
            REQUEST.rutCliente
            ).ToList();

            try
            {
                RESPONSE.count = TRACKING.Count;
                if (TRACKING.Count > 0)
                {
                    foreach (var item in TRACKING)
                    {
                        API_RESPONSE_TYPE_14_CAB responseCab = new API_RESPONSE_TYPE_14_CAB();
                        responseCab.EmpId = (item.EmpId == null ? 0 : item.EmpId);
                        responseCab.NumeroReferencia = (item.NumeroReferencia == null ? 0 : Int32.Parse(item.NumeroReferencia.Trim()));
                        responseCab.FechaReferencia = item.FechaReferencia.ToString();
                        responseCab.RutCliente = (item.RutCliente == null ? "" : item.RutCliente.Trim());
                        responseCab.RazonSocial = (item.RazonSocial == null ? "" : item.RazonSocial.Trim());
                        responseCab.Email = (item.Email == null ? "" : item.Email.Trim());
                        responseCab.TipoDocumento = (item.TipoDocumento == null ? "" : item.TipoDocumento.Trim());
                        responseCab.NumeroDocto = (item.NumeroDocto == null ? "" : item.NumeroDocto.Trim());
                        responseCab.FechaDocto = item.FechaDocto.ToString();
                        responseCab.MontoCompra = item.MontoCompra;
                        responseCab.RutTercero = (item.RutTercero == null ? "" : item.RutTercero.Trim());
                        responseCab.NombreTercero = (item.NombreTercero == null ? "" : item.NombreTercero.Trim());
                        responseCab.EmailTercero = (item.EmailTercero == null ? "" : item.EmailTercero.Trim());
                        responseCab.UsuarioDig = (item.UsuarioDig == null ? "" : item.UsuarioDig.Trim());
                        responseCab.TipoSolicitud = item.TipoSolicitud;
                        responseCab.Direccion = (item.Direccion == null ? "" : item.Direccion.Trim());
                        responseCab.Telefono = (item.Telefono == null ? "" : item.Telefono.Trim());
                        responseCab.Glosa = (item.Glosa == null ? "" : item.Glosa.Trim());
                        responseCab.Estado = (item.Estado == null ? "" : item.Estado.Trim());

                        List<sp_sel_API_TrackingPickingDet_Result> TRACKINGDET = GP_ENT.sp_sel_API_TrackingPickingDet(Int32.Parse(item.NumeroReferencia)).ToList();
                        if (TRACKINGDET.Count > 0)
                        {
                            responseCab.Detalle = TRACKINGDET.Select(m => new
                            {
                                Linea = m.Linea,
                                CodigoArticulo = m.CodigoArticulo.Trim(),
                                Peso = m.Peso,
                                Volumen = m.Volumen,
                                DescriPart = m.DescriPart,
                                Cantidad = m.Cantidad,
                                HUID = m.HUID,
                                Bulto = m.Bulto,
                            }).Cast<dynamic>().ToList();
                        }
                        RESPONSE.items.Add(responseCab);
                    }
                }
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;

                STATUS_CODE = HttpStatusCode.InternalServerError;

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
        }
        #endregion

        #region CONFIRMACION DE RECEPCIÓN DE BULTOS (11)
        [HttpPost]
        [Route("CONFRECEPBULTOS/CREAR")]
        public IHttpActionResult recurso11([FromBody] API_REQUEST_TYPE_10_CAB REQUEST)
        {
            API_RESPONSE_TYPE_10 RESPONSE = new API_RESPONSE_TYPE_10();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_11;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fh = new DateTime(); //fechaHora
            DateTime fp = new DateTime(); //fechaProceso
            DateTime fd = new DateTime(); //fechaDocto
            DateTime fr = new DateTime(); //FechaReferencia

            DateTime fv = new DateTime(); //FechaVencto

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO

            #region VALIDACIONES DE CAMPOS
            REQUEST.intName = (REQUEST.intName == null ? "" : REQUEST.intName);
            REQUEST.fechaHora = (REQUEST.fechaHora == null ? "" : REQUEST.fechaHora);
            REQUEST.fechaProceso = (REQUEST.fechaProceso == null ? "" : REQUEST.fechaProceso);
            REQUEST.fechaDocto = (REQUEST.fechaDocto == null ? "" : REQUEST.fechaDocto);
            REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia);
            REQUEST.FechaReferencia = (REQUEST.FechaReferencia == null ? "" : REQUEST.FechaReferencia);
            foreach (var item in REQUEST.Detalles)
            {
                item.CodigoArticulo = (item.CodigoArticulo == null ? "" : item.CodigoArticulo);
                item.CodigoOriginal = (item.CodigoOriginal == null ? "" : item.CodigoOriginal);
                item.UnidadCompra = (item.UnidadCompra == null ? "" : item.UnidadCompra);
                item.LoteSerie = (item.LoteSerie == null ? "" : item.LoteSerie);
                item.FechaVencto = (item.FechaVencto == null ? "" : item.FechaVencto);
                item.HU = (item.HU == null ? "" : item.HU);
            }
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaHora.Equals(""))
                    {
                        fh = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fh = DateTime.ParseExact(REQUEST.fechaHora, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAHORA DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }
                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        fp = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fp = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAPROCESO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaDocto.Equals(""))
                    {
                        fd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fd = DateTime.ParseExact(REQUEST.fechaDocto, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHADOCTO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.FechaReferencia.Equals(""))
                    {
                        fr = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fr = DateTime.ParseExact(REQUEST.FechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_Confirma_Recep_Bultos_Result> BULTOS = new List<sp_in_API_Confirma_Recep_Bultos_Result>();

            foreach (var item in REQUEST.Detalles.ToList())
            {
                try
                {
                    if (item.FechaVencto.Equals(""))
                    {
                        fv = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fv = DateTime.ParseExact(item.FechaVencto, "dd-MM-yyyy", null);
                    }
                }
                catch
                {
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(ERROR - FECHAVENCIMIENTO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020)";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                }
                BULTOS = GP_ENT.sp_in_API_Confirma_Recep_Bultos(REQUEST.empId,
                                                                REQUEST.recepcionId,
                                                                REQUEST.intName,
                                                                fh,
                                                                REQUEST.solRecepId,
                                                                fp,
                                                                REQUEST.TipoDocumento,
                                                                REQUEST.NumeroDocto,
                                                                fd,
                                                                REQUEST.TipoReferencia,
                                                                REQUEST.NumeroReferencia,
                                                                fr,
                                                                item.Linea,
                                                                item.CodigoArticulo,
                                                                item.CodigoOriginal,
                                                                item.UnidadCompra,
                                                                item.CantidadSolicitada,
                                                                item.ItemReferencia,
                                                                item.LoteSerie,
                                                                fv,
                                                                item.CantRecibida,
                                                                item.HU).ToList();
            }

            #region COMMENTS
            //List<sp_in_API_SolDesp_Result> BULTO = GP_ENT.sp_in_API_SolDesp(
            //REQUEST.empId,
            //REQUEST.recepcionId,
            //REQUEST.INT_NAME,
            //REQUEST.fechaHora,
            //REQUEST.solRecepId,
            //REQUEST.fechaProceso,
            //REQUEST.TipoDocumento,
            //REQUEST.NumeroDocto,
            //REQUEST.fechaDocto,
            //REQUEST.TipoReferencia,
            //REQUEST.NumeroReferencia,
            //REQUEST.FechaReferencia).ToList();
            //if (BULTO.Where(m => m.count > 0).Count() > 0)
            //{
            //    RESPONSE.pickUP = (long)BULTO.ElementAt(0).GenPickId;
            //foreach (var item in REQUEST.items.ToList())
            //{
            //    List<sp_in_API_PickUpDet_Result> PICKUP_DET = GP_ENT.sp_in_API_PickUpDet(
            //      this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
            //      , REQUEST.empid
            //      , (int)RESPONSE.pickUP
            //      , item.linea
            //      , item.codigoarticulo
            //      , item.descripart
            //      , item.cantidad
            //      , item.HUId).ToList();
            //}
            //}
            #endregion

            if (BULTOS.Where(m => m.Count > 0).Count() > 0)
            {
                RESPONSE.count = (int)BULTOS.ElementAt(0).Count;
                RESPONSE.resultado = (string)BULTOS.ElementAt(0).Resultado;
                RESPONSE.descripcion = (string)BULTOS.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA CONFORME CLIENTE (12)
        [HttpPost]
        [Route("ENTREGACONFORMECLIENTE/CREAR")]
        public IHttpActionResult recurso12([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_RESPONSE_TYPE_10 RESPONSE = new API_RESPONSE_TYPE_10();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_12;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fh = new DateTime(); //fechaHora
            DateTime fp = new DateTime(); //fechaProceso
            DateTime fd = new DateTime(); //fechaDocto
            DateTime fr = new DateTime(); //FechaReferencia

            DateTime fvs = new DateTime(); //FechaVenctoSol
            DateTime fvd = new DateTime(); //FechaVenctoDesp

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO

            #region VALIDACIONES DE CAMPOS
            REQUEST.intName = (REQUEST.intName == null ? "" : REQUEST.intName);
            REQUEST.fechaHora = (REQUEST.fechaHora == null ? "" : REQUEST.fechaHora);
            REQUEST.fechaProceso = (REQUEST.fechaProceso == null ? "" : REQUEST.fechaProceso);
            REQUEST.fechaDocto = (REQUEST.fechaDocto == null ? "" : REQUEST.fechaDocto);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.numeroReferencia = (REQUEST.numeroReferencia == null ? "" : REQUEST.numeroReferencia);
            REQUEST.fechaReferencia = (REQUEST.fechaReferencia == null ? "" : REQUEST.fechaReferencia);
            foreach (var item in REQUEST.detalles)
            {
                item.codigoArticulo = (item.codigoArticulo == null ? "" : item.codigoArticulo);
                item.codigoOriginal = (item.codigoOriginal == null ? "" : item.codigoOriginal);
                item.unidadVenta = (item.unidadVenta == null ? "" : item.unidadVenta);
                item.loteSerieSol = (item.loteSerieSol == null ? "" : item.loteSerieSol);
                item.fechaVenctoSol = (item.fechaVenctoSol == null ? "" : item.fechaVenctoSol);
                item.loteSerieDesp = (item.loteSerieDesp == null ? "" : item.loteSerieDesp);
                item.fechaVenctoDesp = (item.fechaVenctoDesp == null ? "" : item.fechaVenctoDesp);
            }
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaHora.Equals(""))
                    {
                        fh = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fh = DateTime.ParseExact(REQUEST.fechaHora, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAHORA DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }
                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        fp = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fp = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAPROCESO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaDocto.Equals(""))
                    {
                        fd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fd = DateTime.ParseExact(REQUEST.fechaDocto, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHADOCTO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaReferencia.Equals(""))
                    {
                        fr = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fr = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_Entrega_Conforme_Cliente_Result> BULTOS = new List<sp_in_API_Entrega_Conforme_Cliente_Result>();

            foreach (var item in REQUEST.detalles.ToList())
            {
                try
                {
                    try
                    {
                        if (item.fechaVenctoSol.Equals(""))
                        {
                            fvs = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvs = DateTime.ParseExact(item.fechaVenctoSol, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTOSOL DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                    try
                    {
                        if (item.fechaVenctoDesp.Equals(""))
                        {
                            fvd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvd = DateTime.ParseExact(item.fechaVenctoDesp, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTODESP DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
                BULTOS = GP_ENT.sp_in_API_Entrega_Conforme_Cliente(
                    REQUEST.empId,
                    REQUEST.colaPickId,
                    REQUEST.intName,
                    fh,
                    REQUEST.solDespId,
                    fp,
                    REQUEST.tipoDocumento,
                    REQUEST.numeroDocto,
                    fd,
                    REQUEST.tipoReferencia,
                    REQUEST.numeroReferencia,
                    fr,
                    item.linea,
                    item.codigoArticulo,
                    item.codigoOriginal,
                    item.unidadVenta,
                    item.cantidadSolicitada,
                    item.itemReferencia,
                    item.loteSerieSol,
                    fvs,
                    item.cantidadDespachada,
                    item.loteSerieDesp,
                    fvd,
                    item.huId).ToList();
            }

            if (BULTOS.Where(m => m.Count > 0).Count() > 0)
            {
                RESPONSE.count = (int)BULTOS.ElementAt(0).Count;
                RESPONSE.resultado = (string)BULTOS.ElementAt(0).Resultado;
                RESPONSE.descripcion = (string)BULTOS.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA NO CONFORME CLIENTE (13)
        [HttpPost]
        [Route("ENTREGANOCONFORMECLIENTE/CREAR")]
        public IHttpActionResult recurso13([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_RESPONSE_TYPE_10 RESPONSE = new API_RESPONSE_TYPE_10();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_13;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fh = new DateTime(); //fechaHora
            DateTime fp = new DateTime(); //fechaProceso
            DateTime fd = new DateTime(); //fechaDocto
            DateTime fr = new DateTime(); //FechaReferencia

            DateTime fvs = new DateTime(); //FechaVenctoSol
            DateTime fvd = new DateTime(); //FechaVenctoDesp

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO

            #region VALIDACIONES DE CAMPOS
            REQUEST.intName = (REQUEST.intName == null ? "" : REQUEST.intName);
            REQUEST.fechaHora = (REQUEST.fechaHora == null ? "" : REQUEST.fechaHora);
            REQUEST.fechaProceso = (REQUEST.fechaProceso == null ? "" : REQUEST.fechaProceso);
            REQUEST.fechaDocto = (REQUEST.fechaDocto == null ? "" : REQUEST.fechaDocto);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.numeroReferencia = (REQUEST.numeroReferencia == null ? "" : REQUEST.numeroReferencia);
            REQUEST.fechaReferencia = (REQUEST.fechaReferencia == null ? "" : REQUEST.fechaReferencia);
            foreach (var item in REQUEST.detalles)
            {
                item.codigoArticulo = (item.codigoArticulo == null ? "" : item.codigoArticulo);
                item.codigoOriginal = (item.codigoOriginal == null ? "" : item.codigoOriginal);
                item.unidadVenta = (item.unidadVenta == null ? "" : item.unidadVenta);
                item.loteSerieSol = (item.loteSerieSol == null ? "" : item.loteSerieSol);
                item.fechaVenctoSol = (item.fechaVenctoSol == null ? "" : item.fechaVenctoSol);
                item.loteSerieDesp = (item.loteSerieDesp == null ? "" : item.loteSerieDesp);
                item.fechaVenctoDesp = (item.fechaVenctoDesp == null ? "" : item.fechaVenctoDesp);
            }
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaHora.Equals(""))
                    {
                        fh = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fh = DateTime.ParseExact(REQUEST.fechaHora, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAHORA DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }
                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        fp = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fp = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAPROCESO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaDocto.Equals(""))
                    {
                        fd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fd = DateTime.ParseExact(REQUEST.fechaDocto, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHADOCTO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaReferencia.Equals(""))
                    {
                        fr = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fr = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_Entrega_No_Conforme_Cliente_Result> BULTOS = new List<sp_in_API_Entrega_No_Conforme_Cliente_Result>();

            foreach (var item in REQUEST.detalles.ToList())
            {
                try
                {
                    try
                    {
                        if (item.fechaVenctoSol.Equals(""))
                        {
                            fvs = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvs = DateTime.ParseExact(item.fechaVenctoSol, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTOSOL DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                    try
                    {
                        if (item.fechaVenctoDesp.Equals(""))
                        {
                            fvd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvd = DateTime.ParseExact(item.fechaVenctoDesp, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTODESP DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
                BULTOS = GP_ENT.sp_in_API_Entrega_No_Conforme_Cliente(
                    REQUEST.empId,
                    REQUEST.colaPickId,
                    REQUEST.intName,
                    fh,
                    REQUEST.solDespId,
                    fp,
                    REQUEST.tipoDocumento,
                    REQUEST.numeroDocto,
                    fd,
                    REQUEST.tipoReferencia,
                    REQUEST.numeroReferencia,
                    fr,
                    item.linea,
                    item.codigoArticulo,
                    item.codigoOriginal,
                    item.unidadVenta,
                    item.cantidadSolicitada,
                    item.itemReferencia,
                    item.loteSerieSol,
                    fvs,
                    item.cantidadDespachada,
                    item.loteSerieDesp,
                    fvd,
                    item.huId).ToList();
            }

            if (BULTOS.Where(m => m.Count > 0).Count() > 0)
            {
                RESPONSE.count = (int)BULTOS.ElementAt(0).Count;
                RESPONSE.resultado = (string)BULTOS.ElementAt(0).Resultado;
                RESPONSE.descripcion = (string)BULTOS.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA CONFORME TIENDA (14)
        [HttpPost]
        [Route("ENTREGACONFORMETIENDA/CREAR")]
        public IHttpActionResult recurso14([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_RESPONSE_TYPE_10 RESPONSE = new API_RESPONSE_TYPE_10();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_14;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fh = new DateTime(); //fechaHora
            DateTime fp = new DateTime(); //fechaProceso
            DateTime fd = new DateTime(); //fechaDocto
            DateTime fr = new DateTime(); //FechaReferencia

            DateTime fvs = new DateTime(); //FechaVenctoSol
            DateTime fvd = new DateTime(); //FechaVenctoDesp

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO

            #region VALIDACIONES DE CAMPOS
            REQUEST.intName = (REQUEST.intName == null ? "" : REQUEST.intName);
            REQUEST.fechaHora = (REQUEST.fechaHora == null ? "" : REQUEST.fechaHora);
            REQUEST.fechaProceso = (REQUEST.fechaProceso == null ? "" : REQUEST.fechaProceso);
            REQUEST.fechaDocto = (REQUEST.fechaDocto == null ? "" : REQUEST.fechaDocto);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.numeroReferencia = (REQUEST.numeroReferencia == null ? "" : REQUEST.numeroReferencia);
            REQUEST.fechaReferencia = (REQUEST.fechaReferencia == null ? "" : REQUEST.fechaReferencia);
            foreach (var item in REQUEST.detalles)
            {
                item.codigoArticulo = (item.codigoArticulo == null ? "" : item.codigoArticulo);
                item.codigoOriginal = (item.codigoOriginal == null ? "" : item.codigoOriginal);
                item.unidadVenta = (item.unidadVenta == null ? "" : item.unidadVenta);
                item.loteSerieSol = (item.loteSerieSol == null ? "" : item.loteSerieSol);
                item.fechaVenctoSol = (item.fechaVenctoSol == null ? "" : item.fechaVenctoSol);
                item.loteSerieDesp = (item.loteSerieDesp == null ? "" : item.loteSerieDesp);
                item.fechaVenctoDesp = (item.fechaVenctoDesp == null ? "" : item.fechaVenctoDesp);
            }
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaHora.Equals(""))
                    {
                        fh = DateTime.ParseExact("01-01-1900 00:00:00", "dd-MM-yyyy HH:mm:ss", null);
                    }
                    else
                    {
                        fh = DateTime.ParseExact(REQUEST.fechaHora, "dd-MM-yyyy HH:mm:ss", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAHORA DEBE SER FORMATO FECHA dd-MM-yyyy HH:mm:ss, por ejemplo 06-11-2020 20:18:11"); }
                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        fp = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fp = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAPROCESO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaDocto.Equals(""))
                    {
                        fd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fd = DateTime.ParseExact(REQUEST.fechaDocto, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHADOCTO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaReferencia.Equals(""))
                    {
                        fr = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fr = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_Entrega_Conforme_Tienda_Result> BULTOS = new List<sp_in_API_Entrega_Conforme_Tienda_Result>();

            foreach (var item in REQUEST.detalles.ToList())
            {
                try
                {
                    try
                    {
                        if (item.fechaVenctoSol.Equals(""))
                        {
                            fvs = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvs = DateTime.ParseExact(item.fechaVenctoSol, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTOSOL DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                    try
                    {
                        if (item.fechaVenctoDesp.Equals(""))
                        {
                            fvd = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fvd = DateTime.ParseExact(item.fechaVenctoDesp, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVENCTODESP DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
                BULTOS = GP_ENT.sp_in_API_Entrega_Conforme_Tienda(
                    REQUEST.empId,
                    REQUEST.colaPickId,
                    REQUEST.intName,
                    fh,
                    REQUEST.solDespId,
                    fp,
                    REQUEST.tipoDocumento,
                    REQUEST.numeroDocto,
                    fd,
                    REQUEST.tipoReferencia,
                    REQUEST.numeroReferencia,
                    fr,
                    item.linea,
                    item.codigoArticulo,
                    item.codigoOriginal,
                    item.unidadVenta,
                    item.cantidadSolicitada,
                    item.itemReferencia,
                    item.loteSerieSol,
                    fvs,
                    item.cantidadDespachada,
                    item.loteSerieDesp,
                    fvd,
                    item.huId).ToList();
            }

            if (BULTOS.Where(m => m.Count > 0).Count() > 0)
            {
                RESPONSE.count = (int)BULTOS.ElementAt(0).Count;
                RESPONSE.resultado = (string)BULTOS.ElementAt(0).Resultado;
                RESPONSE.descripcion = (string)BULTOS.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion

        }
        #endregion

        //Consulta Stock, se agrega detalle de saldos separados por estado
        #region CONSULTA STOCK (15)
        [HttpGet]
        [HttpPost]
        [Route("CONSTOCK/LISTAR")]
        public IHttpActionResult recurso15(API_REQUEST_TYPE_15_ConsultaStock REQUEST)
        {
            API_RESPONSE_TYPE_15 RESPONSE = new API_RESPONSE_TYPE_15();
            ERROR = new API_RESPONSE_ERRORS();

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_15;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "El json viene vacio";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea, siempre vendra un valor, independiente se envie o no el campo en la estructura Json
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, significa que por defecto es opcional

            REQUEST.TipoProducto = (REQUEST.TipoProducto == null ? "" : REQUEST.TipoProducto); //opcional
            REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo); // opcional

            #endregion

            #region PROCESO
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            string CodigoArticuloFiltro = "";

            //si trae un solo codigo articulo 
            if (REQUEST.CodigoArticulo.Trim() != "")
            {
                CodigoArticuloFiltro = REQUEST.CodigoArticulo;
            }
            else //si no trae producto y trae lista de articulos los recorre y concatena 
            {
                if (REQUEST.Articulos != null)
                {
                    if (REQUEST.Articulos.Count > 0)
                    {
                        //Valida datos de Items json
                        foreach (var item in REQUEST.Articulos)
                        {
                            if (CodigoArticuloFiltro.Trim() == "")
                            {
                                CodigoArticuloFiltro = item.CodigoArticulo.Trim();
                            }
                            else
                            {
                                //--alt 174 « separador tu

                                CodigoArticuloFiltro = CodigoArticuloFiltro + "«" + item.CodigoArticulo.Trim();
                            }
                        }
                    }
                }                
            }

            List<sp_sel_API_Consulta_Stock_Result> STOCKS = GP_ENT.sp_sel_API_Consulta_Stock(REQUEST.EmpId,
                                                                                             REQUEST.LineaProducto,
                                                                                             REQUEST.TipoProducto,
                                                                                             CodigoArticuloFiltro.Trim(),
                                                                                             USERNAME,
                                                                                             REQUEST.CodigoBodega).ToList();
            foreach (var item in STOCKS) 
            { 
                HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Result), item); 
            }

            //if (STOCKS.Count() > 0) 
            //{ 
            //    RESPONSE.count = (int)STOCKS.ElementAt(0).Count; 
            //}
            //else 
            //{ 
            //    RESPONSE.count = 0; 
            //}

            //RESPONSE.items = STOCKS.Select(m => new {Serie               = m.NumeroSerie,
            //                                        FechaVencimiento    = m.FechaVecto,
            //                                        CodigoArticulo      = m.CodigoArticulo,
            //                                        Stock               = m.Stock
            //                                       }).Cast<dynamic>().ToList();

            //Si retorna resultados carga JSON de retorno -----
            if (STOCKS.Count > 0)
            {
                RESPONSE.count = STOCKS.Count;

                string NumeroSerie = "";
                string FechaVecto = "";
                string CodigoArticulo = "";
                string GlosaEstado = "";

                API_RESPONSE_TYPE_15_DET RESPONSE_STOCK = new API_RESPONSE_TYPE_15_DET();
                API_RESPONSE_TYPE_15_SALDOS RESPONSE_SALDOS = new API_RESPONSE_TYPE_15_SALDOS();

                foreach (var Stock in STOCKS)
                {
                    if ((Stock.NumeroSerie != NumeroSerie) || (Stock.FechaVecto != FechaVecto) || (Stock.CodigoArticulo != CodigoArticulo))
                    {
                        if (CodigoArticulo.Trim() != "")
                        {
                            RESPONSE.items.Add(RESPONSE_STOCK);
                        }

                        //Inicializa variable de Item -------------
                        RESPONSE_STOCK = new API_RESPONSE_TYPE_15_DET();

                        RESPONSE_STOCK.Serie = Stock.NumeroSerie;
                        RESPONSE_STOCK.FechaVencimiento = Stock.FechaVecto;
                        RESPONSE_STOCK.CodigoArticulo = Stock.CodigoArticulo;
                        RESPONSE_STOCK.Stock = decimal.Parse(Stock.Stock.ToString());
                        RESPONSE_STOCK.StockTotal = decimal.Parse(Stock.StockTotal.ToString());

                        NumeroSerie = Stock.NumeroSerie;
                        FechaVecto = Stock.FechaVecto;
                        CodigoArticulo = Stock.CodigoArticulo;
                        GlosaEstado = "";
                    }

                    if (Stock.GlosaEstado != GlosaEstado)
                    {
                        RESPONSE_SALDOS = new API_RESPONSE_TYPE_15_SALDOS();

                        RESPONSE_SALDOS.Estado = Stock.GlosaEstado;
                        RESPONSE_SALDOS.StockEstado = decimal.Parse(Stock.StockEstado.ToString());

                        //Agrega nueva UM al JSON del articulo
                        RESPONSE_STOCK.StockEstados.Add(RESPONSE_SALDOS);

                        GlosaEstado = Stock.GlosaEstado;
                    }
                }

                //Agrega ultimo item que estaba procesando al JSON
                RESPONSE.items.Add(RESPONSE_STOCK);
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region GeneracionRutas_Beetrack (16)
        [HttpPost]
        [Route("GeneracionRutas_Beetrack")] //CAMBIAR NOMBRE
        public IHttpActionResult recurso16(API_REQUEST_TYPE_12 REQUEST)
        {
            API_RESPONSE_TYPE_10 RESPONSE = new API_RESPONSE_TYPE_10();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_15;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            //DateTime createAt = new DateTime(); //???

            //DateTime minDeliveryTime = new DateTime(); //???
            //DateTime maxDeliveryTime = new DateTime(); //???

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.start_date = (REQUEST.start_date == null ? "" : REQUEST.start_date);
            REQUEST.end_date = (REQUEST.end_date == null ? "" : REQUEST.end_date);
            REQUEST.vehicle.identifier = (REQUEST.vehicle.identifier == null ? "" : REQUEST.vehicle.identifier);
            REQUEST.created_at = (REQUEST.created_at == null ? "" : REQUEST.created_at);
            foreach (var stops in REQUEST.stops)
            {
                stops.identifier = (stops.identifier == null ? "" : stops.identifier);
                stops.address = (stops.address == null ? "" : stops.address);
                stops.latitude = (stops.latitude == null ? "" : stops.latitude);
                stops.longitude = (stops.longitude == null ? "" : stops.longitude);
                foreach (var times in stops.delivery_times)
                {
                    times.min_delivery_time = (times.min_delivery_time == null ? "" : times.min_delivery_time);
                    times.max_delivery_time = (times.max_delivery_time == null ? "" : times.max_delivery_time);
                }
            }

            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.start_date.Equals(""))
                    {
                        startDate = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", null);
                    }
                    else
                    {
                        startDate = DateTime.ParseExact(REQUEST.start_date, "yyyy-MM-dd", null);
                    }
                }
                catch { throw new Exception("ERROR - start_data DEBE SER FORMATO FECHA yyyy-MM-dd HH:mm:ss, por ejemplo 2021-03-14"); }
                try
                {
                    if (REQUEST.end_date.Equals(""))
                    {
                        endDate = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", null);
                    }
                    else
                    {
                        endDate = DateTime.ParseExact(REQUEST.end_date, "yyyy-MM-dd", null);
                    }
                }
                catch { throw new Exception("ERROR - end_date DEBE SER FORMATO FECHA yyyy-MM-dd HH:mm:ss, por ejemplo 2021-03-14"); }

            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region LARGOS PERMITIDOS
            #endregion
            #endregion

            var nombreArchivo = REQUEST.vehicle.identifier + DateTime.Now.ToString("yyMMddHHmmss") + ".csv"; ;
            var count = 0;
            var newTxt = "";
            foreach (var stops in REQUEST.stops)
            {
                count++;
                newTxt = newTxt + "INT-HOJARUTA-SDD;" + REQUEST.vehicle.identifier + ";" + stops.identifier + ";" + stops.latitude + ";" + stops.longitude
                     + ";" + 1 + ";" + count + ";";
                newTxt = newTxt + System.Environment.NewLine;

            }

            //List<sp_in_API_Integracion_Result> 
            //    STOPS = GP_ENT.sp_in_API_Integracion(this.Request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault()
            //                                        ,1
            //                                        ,nombreArchivo
            //                                        ,newTxt).ToList();
            RESPONSE.count = 1;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "Integración realizada correctamente";
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region CONSULTA STOCK BODEGA (GET)(17)
        [HttpGet]
        [Route("CONSTOCKBODEGA/LISTAR")]
        public IHttpActionResult Recurso16(API_REQUEST_TYPE_9 REQUEST)
        {
            API_RESPONSE_TYPE_9 RESPONSE = new API_RESPONSE_TYPE_9();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_15;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.TipoProducto = (REQUEST.TipoProducto == null ? "" : REQUEST.TipoProducto);
            REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo);
            #region LARGOS PERMITIDOS
            #endregion
            #endregion

            List<sp_sel_API_Consulta_Stock_Bodega_Result> STOCK = GP_ENT.sp_sel_API_Consulta_Stock_Bodega(REQUEST.EmpId
                                                                                                         ,REQUEST.LineaProducto
                                                                                                         ,REQUEST.TipoProducto
                                                                                                         ,REQUEST.CodigoArticulo
                                                                                                         ,USERNAME
                                                                                                         ).ToList();

            foreach (var item in STOCK) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Result), item); }

            if (STOCK.Count() > 0) 
            { 
                RESPONSE.count = (int)STOCK.ElementAt(0).Count; 
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            RESPONSE.items = STOCK.Select(m => new {
                                                    CodigoBodega = m.CodigoBodega,
                                                    GlosaBodega = m.GlosaBodega,
                                                    Serie = m.NumeroSerie,
                                                    FechaVencimiento = m.FechaVecto,
                                                    CodigoArticulo = m.CodigoArticulo,
                                                    Stock = m.Stock
                                                   }).Cast<dynamic>().ToList();

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region CONSULTA STOCK BODEGA (POST)(18) -- no usar --
        [HttpPost]
        [Route("CONSTOCKBODEGA/LISTAR")]
        public IHttpActionResult recurso16_2(API_REQUEST_TYPE_9 REQUEST)
        {
            API_RESPONSE_TYPE_9 RESPONSE = new API_RESPONSE_TYPE_9();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_15;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS

            REQUEST.TipoProducto = (REQUEST.TipoProducto == null ? "" : REQUEST.TipoProducto);
            REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo);

            #endregion

            List<sp_sel_API_Consulta_Stock_Bodega_Result> STOCK = GP_ENT.sp_sel_API_Consulta_Stock_Bodega(REQUEST.EmpId
                                                                                                         ,REQUEST.LineaProducto
                                                                                                         ,REQUEST.TipoProducto
                                                                                                         ,REQUEST.CodigoArticulo
                                                                                                         ,USERNAME).ToList();

            foreach (var item in STOCK) 
            { 
                HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Result), item); 
            }
            if (STOCK.Count() > 0) 
            { 
                RESPONSE.count = (int)STOCK.ElementAt(0).Count; 
            }
            else 
            { 
                RESPONSE.count = 0; 
            }

            RESPONSE.items = STOCK
            .Select(m => new
            {
                codigoBodega = m.CodigoBodega,
                glosaBodega = m.GlosaBodega,
                serie = m.NumeroSerie,
                codigoArticulo = m.CodigoArticulo,
                stock = m.Stock
            })
            .Cast<dynamic>().ToList();

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        //recurso 17 CREAR SOLICITUD RECEPCION JSON (17) -------
        #region CREAR SOLICITUD RECEPCION JSON (17) ***************
        [HttpPost]
        [Route("SOLRECEP/CREARJSON")]
        public IHttpActionResult recurso17([FromBody] API_REQUEST_TYPE_17 REQUEST)
        {
            API_RESPONSE_TYPE_7 RESPONSE = new API_RESPONSE_TYPE_7();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLRECEP/CREARJSON";

            //Valida json en blanco ---
            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK

            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "El json viene vacio", true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "El json viene vacio";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ---------------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Creación SDR, JSON: " + body.ToString(),
                    false,
                    true,
                    (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia),
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Valida Token y Secret, retorna Usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, ERROR.Mensaje.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, ERROR.Mensaje, true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES
            //Reglas de validacion ----------------------------------------------------
            // - campos string       : Podria traer un valor o un null. Si el campo no se incluye en el JSON traerá un null.
            // - campos numericos    : Siempre traerá un valor.         Si el campo no se incluye en el JSON traerá un cero.
            // - campos fechas       : Se manejan como campos string con las mismas reglas. Posteriormente se valida que las fechas vengan con el formato correcto.
            // - un campo string se puede dejar opcional asignandole un valor por defecto en caso que venga con null

            string ListadoCodigoArticulo = "";

            try
            {
                //valida datos cabecera json ---------------------------------------------------------------

                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                //REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional
                if (REQUEST.Proveedor == null || REQUEST.Proveedor == "") { throw new Exception("Debe informar Proveedor"); } //requerido
                if (REQUEST.RazonSocial == null || REQUEST.RazonSocial == "") { throw new Exception("Debe informar Razon Social"); } //requerido
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                if (REQUEST.Glosa == null || REQUEST.Glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido
                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional

                //decimal Valor1 //opcional numerico, toma cero
                //decimal Valor2 //opcional numerico, toma cero
                //decimal Valor3 //opcional numerico, toma cero

                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional                    
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                int PrimerItem = 1;

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadCompra == null || item.UnidadCompra == "") { throw new Exception("Debe informar Unidad Compra"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    item.NumeroSerie = (item.NumeroSerie == null ? "" : item.NumeroSerie); //opcional
                    item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido

                    //item.PorcQA //opcional numerico, toma cero
                    //item.SolDespIdCD //opcional numerico, toma cero
                    //item.CantidadCD //opcional numerico, toma cero

                    item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional

                    //decimal Valor1 //opcional numerico, toma cero
                    //decimal Valor2 //opcional numerico, toma cero
                    //decimal Valor3 //opcional numerico, toma cero

                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional                    
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional

                    //Valida datos de Items json, si trae detalles de crossdocking los campos son obligatorios
                    if (item.Crossdocking != null) 
                    { 
                        foreach (var Crossdocking1 in item.Crossdocking)
                        {
                            if (Crossdocking1.TipoReferenciaCD == null || Crossdocking1.TipoReferenciaCD == "") { throw new Exception("Debe informar Tipo Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.NumeroReferenciaCD == null || Crossdocking1.NumeroReferenciaCD == "") { throw new Exception("Debe informar Numero Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.ItemReferenciaCD <= 0) { throw new Exception("Debe informar Item Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.CantidadCD <= 0) { throw new Exception("Debe informar Cantidad Item " + Crossdocking1.ItemReferenciaCD.ToString().Trim() + " Crossdocking > 0"); } //requerido
                        }
                    } 

                    //REQUEST.items.Count --> cantidad items que trae el json 

                    //concatena items de la solicitud con ;
                    if (PrimerItem == 1)
                    {
                        ListadoCodigoArticulo = item.CodigoArticulo;
                        PrimerItem = 0;
                    }
                    else
                    {
                        ListadoCodigoArticulo = ListadoCodigoArticulo + ";" + item.CodigoArticulo;
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso, ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA FORMATO FECHAS -----

            DateTime FechaReferencia = new DateTime(); //fechaReferencia
            DateTime FechaProceso = new DateTime();
            DateTime Fecha1Cab = new DateTime(); //fecha1 cabecera
            DateTime Fecha2Cab = new DateTime(); //fecha2 cabecera
            DateTime Fecha3Cab = new DateTime(); //fecha3 cabecera
            DateTime fechaVectoLote = new DateTime(); //fechaVectoLote
            DateTime fecha1 = new DateTime(); //fecha1 detalle
            DateTime fecha2 = new DateTime(); //fecha2 detalle
            DateTime fecha3 = new DateTime(); //fecha3 detalle

            try
            {
                //VALIDA FECHAS CABECERA JSON 
                FechaReferencia = ValidaCampoFecha(REQUEST.FechaReferencia, "FechaReferencia");
                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "FechaProceso");
                Fecha1Cab = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1 cabecera");
                Fecha2Cab = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2 cabecera");
                Fecha3Cab = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3 cabecera");

                //VALIDA FECHAS ITEMS DETALLE JSON
                foreach (var item in REQUEST.Items.ToList())
                {
                    fechaVectoLote = ValidaCampoFecha(item.FechaVectoLote, "FechaVectoLote");
                    fecha1 = ValidaCampoFecha(item.Fecha1, "Fecha1 detalle");
                    fecha2 = ValidaCampoFecha(item.Fecha2, "Fecha2 detalle");
                    fecha3 = ValidaCampoFecha(item.Fecha3, "Fecha3 detalle");
                }                
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso, ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de validaciones iniciales realiza proceso --- 
            #region PROCESAMIENTO 

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_SolRecepcionJson_Result> SOLRECEP = new List<sp_in_API_SolRecepcionJson_Result>();
            List<sp_in_API_SolRecepcionDetJson_Result> SOLRECEPDET = new List<sp_in_API_SolRecepcionDetJson_Result>();
            List<sp_proc_API_SolRecepcionValidaJson_Result> SOLRECEPVALIDA = new List<sp_proc_API_SolRecepcionValidaJson_Result>();

            try
            {
                //Inserta cabecera solicitud recepcion 
                SOLRECEP = GP_ENT.sp_in_API_SolRecepcionJson(REQUEST.Empid,
                                                             USERNAME,
                                                             FechaProceso,
                                                             REQUEST.TipoSolicitud,
                                                             REQUEST.CodigoBodega,
                                                             REQUEST.Comprador,
                                                             REQUEST.Proveedor,
                                                             REQUEST.RazonSocial,
                                                             REQUEST.TipoReferencia,
                                                             REQUEST.NumeroReferencia,
                                                             FechaReferencia,
                                                             REQUEST.Glosa,
                                                             REQUEST.Dato1,
                                                             REQUEST.Dato2,
                                                             REQUEST.Dato3,
                                                             REQUEST.Valor1,
                                                             REQUEST.Valor2,
                                                             REQUEST.Valor3,
                                                             Fecha1Cab,
                                                             Fecha2Cab,
                                                             Fecha3Cab
                                                             ).ToList();
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            if (SOLRECEP.Count > 0)
            {
                if (SOLRECEP[0].Count > 0) //pregunta por el campo Count, si es mayor a cero procesó OK la cabecera
                {
                    RESPONSE.solRecepId = (int)SOLRECEP[0].SolRecepId;
                    RESPONSE.descripcion = SOLRECEP[0].Descripcion;

                    string Item_crossdocking = "";

                    //recorre items json
                    foreach (var item2 in REQUEST.Items.ToList())
                    {
                        Item_crossdocking = "";

                        //si tiene detalle de crossdocking lo envia
                        if (item2.Crossdocking != null)
                        {
                            foreach (var croosdock2 in item2.Crossdocking.ToList())
                            {
                                if (Item_crossdocking.Trim() == "") //Si es el primer item del crossdocking
                                {
                                    Item_crossdocking = croosdock2.TipoReferenciaCD.Trim() + ";" +
                                                        croosdock2.NumeroReferenciaCD.Trim() + ";" +
                                                        croosdock2.ItemReferenciaCD.ToString().Trim() + ";" +
                                                        croosdock2.CantidadCD.ToString();
                                }
                                else
                                {
                                    Item_crossdocking = Item_crossdocking.Trim() + "#" + croosdock2.TipoReferenciaCD.Trim() + ";" +
                                                                                         croosdock2.NumeroReferenciaCD.Trim() + ";" +
                                                                                         croosdock2.ItemReferenciaCD.ToString().Trim() + ";" +
                                                                                         croosdock2.CantidadCD.ToString(); 
                                }
                            }
                        }

                        fechaVectoLote = ValidaCampoFecha(item2.FechaVectoLote, "FechaVectoLote");
                        fecha1 = ValidaCampoFecha(item2.Fecha1, "Fecha1 detalle");
                        fecha2 = ValidaCampoFecha(item2.Fecha2, "Fecha2 detalle");
                        fecha3 = ValidaCampoFecha(item2.Fecha3, "Fecha3 detalle");

                        try
                        {
                            SOLRECEPDET = GP_ENT.sp_in_API_SolRecepcionDetJson(RESPONSE.solRecepId,
                                                                               item2.CodigoArticulo,
                                                                               item2.UnidadCompra,
                                                                               item2.Cantidad,
                                                                               item2.ItemReferencia,
                                                                               item2.CostoUnitario,
                                                                               item2.KilosTotales,
                                                                               item2.NumeroSerie,
                                                                               fechaVectoLote,
                                                                               item2.Estado,
                                                                               item2.PorcQA,
                                                                               fecha1,
                                                                               fecha2,
                                                                               fecha3,
                                                                               item2.Dato1,
                                                                               item2.Dato2,
                                                                               item2.Dato3,
                                                                               item2.Valor1,
                                                                               item2.Valor2,
                                                                               item2.Valor3,
                                                                               item2.Sucursal,
                                                                               Item_crossdocking
                                                                               ).ToList();
                        }
                        catch (Exception ex)
                        {
                            #region NOK
                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                            LogInfo(NombreProceso,
                                    ERROR.Mensaje.Trim(),
                                    true,
                                    true,
                                    REQUEST.NumeroReferencia,
                                    body.ToString());

                            RESPONSE.resultado = "ERROR";
                            RESPONSE.descripcion = ex.Message.Trim();
                            RESPONSE.resultado_codigo = ERROR.ErrID;
                            RESPONSE.resultado_descripcion = ERROR.Mensaje;
                            STATUS_CODE = HttpStatusCode.BadRequest;
                            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                            #endregion
                        }

                        if (SOLRECEPDET.Count > 0)
                        {
                            if (SOLRECEPDET[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                            {
                                #region NOK
                                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                                LogInfo(NombreProceso,
                                        ERROR.Mensaje.Trim() + ". " + "Error procesando Articulo: " + item2.CodigoArticulo + ", " + SOLRECEPDET[0].Descripcion,
                                        true,
                                        true,
                                        REQUEST.NumeroReferencia,
                                        body.ToString());

                                RESPONSE.resultado = "ERROR";
                                RESPONSE.descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + SOLRECEPDET[0].Descripcion;
                                RESPONSE.resultado_codigo = ERROR.ErrID;
                                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                                RESPONSE.solRecepId = 0;
                                STATUS_CODE = HttpStatusCode.BadRequest;
                                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                #endregion
                            }
                        }
                    } //fin ciclo For each detalle --------------

                    try
                    {
                        //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                        SOLRECEPVALIDA = GP_ENT.sp_proc_API_SolRecepcionValidaJson(RESPONSE.solRecepId,
                                                                                   ListadoCodigoArticulo,
                                                                                   REQUEST.Items.Count
                                                                                   ).ToList();
                    }
                    catch (Exception ex)
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim(),
                                true,
                                true,
                                REQUEST.NumeroReferencia,
                                body.ToString());

                        RESPONSE.resultado = "ERROR";
                        RESPONSE.descripcion = "";
                        RESPONSE.resultado_codigo = ERROR.ErrID;
                        RESPONSE.resultado_descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }

                    if (SOLRECEPVALIDA.Count > 0)
                    {
                        if (SOLRECEPVALIDA[0].Resultado != "OK")
                        {
                            #region NOK
                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

                            LogInfo(NombreProceso,
                                    ERROR.Mensaje.Trim() + ". " + SOLRECEPVALIDA[0].Descripcion,
                                    true,
                                    true,
                                    REQUEST.NumeroReferencia,
                                    body.ToString());

                            RESPONSE.resultado = "ERROR";
                            RESPONSE.descripcion = SOLRECEPVALIDA[0].Descripcion;
                            RESPONSE.resultado_codigo = ERROR.ErrID;
                            RESPONSE.resultado_descripcion = ERROR.Mensaje;
                            RESPONSE.solRecepId = 0;
                            STATUS_CODE = HttpStatusCode.BadRequest;
                            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                            #endregion
                        }
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + SOLRECEP[0].Descripcion,
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = SOLRECEP[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion        
        
        //recurso 18 CREAR SOLICITUD DESPACHO JSON (18) --------
        #region CREAR SOLICITUD DESPACHO JSON (18) ****************
        [HttpPost]
        [Route("SOLDESP/CREARJSON")]
        public IHttpActionResult recurso18([FromBody] API_REQUEST_TYPE_18 REQUEST)
        {
            API_RESPONSE_TYPE_8 RESPONSE = new API_RESPONSE_TYPE_8();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/CREARJSON";

            //Valida estructura JSON recibido sea valida
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "El json viene vacio", true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "El json viene vacio";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido como parametro ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Creación SDD, JSON: " + body.ToString(),
                    false,
                    true,
                    (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia),
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                LogInfo(NombreProceso, ERROR.Mensaje.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            string ListadoCodigoArticulo = "";

            //setea variable de respuesta en OK ----
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.count = 1;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----
                REQUEST.Proceso = (REQUEST.Proceso == null ? "" : REQUEST.Proceso); //opcional
                //REQUEST.empid // se valida con el usuario
                if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                //REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                //REQUEST.tipoDocumento //opcional numerico, toma cero
                REQUEST.NumeroDocto = (REQUEST.NumeroDocto == null ? "" : REQUEST.NumeroDocto); //opcional
                REQUEST.FechaDocto = (REQUEST.FechaDocto == null ? "" : REQUEST.FechaDocto); //opcional
                REQUEST.Glosa = (REQUEST.Glosa == null ? "" : REQUEST.Glosa); //opcional
                REQUEST.Cliente = (REQUEST.Cliente == null ? "" : REQUEST.Cliente); //opcional
                REQUEST.RazonSocial = (REQUEST.RazonSocial == null ? "" : REQUEST.RazonSocial); //opcional
                REQUEST.Telefono = (REQUEST.Telefono == null ? "" : REQUEST.Telefono); //opcional
                REQUEST.Email = (REQUEST.Email == null ? "" : REQUEST.Email); //opcional
                REQUEST.Direccion = (REQUEST.Direccion == null ? "" : REQUEST.Direccion); //opcional
                REQUEST.Contacto = (REQUEST.Contacto == null ? "" : REQUEST.Contacto); //opcional
                //REQUEST.rutaDespacho = (REQUEST.rutaDespacho == null ? "" : REQUEST.rutaDespacho); //opcional numerico, toma cero
                REQUEST.Region = (REQUEST.Region == null ? "" : REQUEST.Region); //opcional
                REQUEST.Comuna = (REQUEST.Comuna == null ? "" : REQUEST.Comuna); //opcional
                REQUEST.Ciudad = (REQUEST.Ciudad == null ? "" : REQUEST.Ciudad); //opcional
                REQUEST.Vendedor = (REQUEST.Vendedor == null ? "" : REQUEST.Vendedor); //opcional
                REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional

                REQUEST.Dato1 = (REQUEST.Dato1== null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional
                //REQUEST.Valor1 //opcional numerico, toma cero
                //REQUEST.Valor2 //opcional numerico, toma cero
                //REQUEST.Valor3 //opcional numerico, toma cero
                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                //REQUEST.Sucursal //opcional numerico, toma cero

                int PrimerItem = 1;

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadVenta == null || item.UnidadVenta == "") { throw new Exception("Debe informar Unidad Venta"); } //requerido
                    item.NumeroSerie = (item.NumeroSerie == null ? "" : item.NumeroSerie); //opcional
                    item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido
                    //item.costoUnitario //opcional numerico, toma cero
                    //item.kilosTotales //opcional numerico, toma cero
                    //item.porcQa //opcional numerico, toma cero
                    //item.maquila //opcional numerico, toma cero
                    item.Pallet = (item.Pallet == null ? "" : item.Pallet); //opcional
                                                                            //item.itemReferencia //opcional numerico, toma cero
                    item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional
                    //item.Valor1 //opcional numerico, toma cero
                    //item.Valor2 //opcional numerico, toma cero
                    //item.Valor3 //opcional numerico, toma cero
                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional

                    //REQUEST.items.Count --> cantidad items que trae el json 

                    //concatena items de la solicitud con ;
                    if (PrimerItem == 1)
                    {
                        ListadoCodigoArticulo = item.CodigoArticulo;
                        PrimerItem = 0;
                    }
                    else
                    {
                        ListadoCodigoArticulo = ListadoCodigoArticulo + ";" + item.CodigoArticulo;
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATO FECHAS

            DateTime FechaReferencia = new DateTime(); //fechaReferencia
            DateTime FechaProceso = new DateTime(); //FechaProceso
            DateTime FechaDocto = new DateTime(); //fechaDocto
            DateTime FechaVectoLote = new DateTime(); //fechaVectoLote
            DateTime Fecha1 = new DateTime(); //Fecha1
            DateTime Fecha2 = new DateTime(); //Fecha2
            DateTime Fecha3 = new DateTime(); //Fecha3
            DateTime Fecha1det = new DateTime(); //Fecha1 detalle
            DateTime Fecha2det = new DateTime(); //Fecha2 detalle
            DateTime Fecha3det = new DateTime(); //Fecha3 detalle

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----
                //VALIDA DATOS CABECERA JSON -----
                //try
                //{
                //    if (REQUEST.FechaReferencia.Equals(""))
                //    {FechaReferencia = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
                //    else
                //    {FechaReferencia = DateTime.ParseExact(REQUEST.FechaReferencia, "dd-MM-yyyy", null);}
                //}
                //catch { throw new Exception("ERROR - FechaReferencia DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }

                FechaReferencia = ValidaCampoFecha(REQUEST.FechaReferencia, "FechaReferencia");
                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "FechaProceso");
                FechaDocto = ValidaCampoFecha(REQUEST.FechaDocto, "FechaDocto");
                Fecha1 = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1");
                Fecha2 = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2");
                Fecha3 = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3");
                                

                //VALIDA FECHAS ITEMS DETALLE JSON
                foreach (var item in REQUEST.Items.ToList())
                {
                    FechaVectoLote = ValidaCampoFecha(item.FechaVectoLote, "FechaVectoLote");
                    Fecha1det = ValidaCampoFecha(item.Fecha1, "Fecha1");
                    Fecha2det = ValidaCampoFecha(item.Fecha2, "Fecha2");
                    Fecha3det = ValidaCampoFecha(item.Fecha3, "Fecha3");
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_SolDespJson_Result> SOLDESPJSON = new List<sp_in_API_SolDespJson_Result>();
            List<sp_in_API_SolDespDetJson_Result> SOLDESPDETJSON = new List<sp_in_API_SolDespDetJson_Result>();
            List<sp_proc_API_SolDespachoValidaJson_Result> SOLDESPVALIDA = new List<sp_proc_API_SolDespachoValidaJson_Result>();

            try
            {
                //Inserta cabecera solicitud despacho
                SOLDESPJSON = GP_ENT.sp_in_API_SolDespJson(REQUEST.Proceso,
                                                           REQUEST.Empid,
                                                           REQUEST.TipoSolicitud,
                                                           REQUEST.CodigoBodega,
                                                           REQUEST.TipoReferencia,
                                                           REQUEST.NumeroReferencia,
                                                           FechaReferencia,
                                                           FechaProceso,
                                                           REQUEST.TipoDocumento,
                                                           REQUEST.NumeroDocto,
                                                           FechaDocto,
                                                           REQUEST.Glosa,
                                                           REQUEST.Cliente,
                                                           REQUEST.RazonSocial,
                                                           REQUEST.Telefono,
                                                           REQUEST.Email,
                                                           REQUEST.Direccion,
                                                           REQUEST.Contacto,
                                                           REQUEST.RutaDespacho,
                                                           REQUEST.Region,
                                                           REQUEST.Comuna,
                                                           REQUEST.Ciudad,
                                                           REQUEST.Vendedor,
                                                           REQUEST.Comprador,
                                                           REQUEST.Dato1,
                                                           REQUEST.Dato2,
                                                           REQUEST.Dato3,
                                                           REQUEST.Valor1,
                                                           REQUEST.Valor2,
                                                           REQUEST.Valor3,
                                                           Fecha1,
                                                           Fecha2,
                                                           Fecha3,
                                                           REQUEST.Sucursal).ToList();
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //si retorna respuesta
            if (SOLDESPJSON.Count > 0)
            {
                if (SOLDESPJSON[0].Count > 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK la cabecera
                {
                    RESPONSE.solDespId = (int)SOLDESPJSON[0].SolDespId; //id generado
                    RESPONSE.descripcion = SOLDESPJSON[0].Descripcion;

                    //recorre items json
                    foreach (var item2 in REQUEST.Items.ToList())
                    {
                        FechaVectoLote = ValidaCampoFecha(item2.FechaVectoLote, "FechaVectoLote");
                        Fecha1det = ValidaCampoFecha(item2.Fecha1, "Fecha1");
                        Fecha2det = ValidaCampoFecha(item2.Fecha2, "Fecha2");
                        Fecha3det = ValidaCampoFecha(item2.Fecha3, "Fecha3");

                        try

                        {
                            //Inserta detalle solicitud despacho
                            SOLDESPDETJSON = GP_ENT.sp_in_API_SolDespDetJson(RESPONSE.solDespId,
                                                                             item2.CodigoArticulo,
                                                                             item2.UnidadVenta,
                                                                             item2.NumeroSerie,
                                                                             FechaVectoLote,
                                                                             item2.Cantidad,
                                                                             item2.Estado,
                                                                             item2.CostoUnitario,
                                                                             item2.KilosTotales,
                                                                             item2.PorcQa,
                                                                             item2.Maquila,
                                                                             item2.Pallet,
                                                                             item2.ItemReferencia,
                                                                             Fecha1det,
                                                                             Fecha2det,
                                                                             Fecha3det,
                                                                             item2.Dato1,
                                                                             item2.Dato2,
                                                                             item2.Dato3,
                                                                             item2.Valor1,
                                                                             item2.Valor2,
                                                                             item2.Valor3,
                                                                             item2.Sucursal
                                                                             ).ToList();
                        }
                        catch (Exception ex)
                        {
                            #region NOK
                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                            LogInfo(NombreProceso,
                                    ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                                    true,
                                    true,
                                    REQUEST.NumeroReferencia,
                                    body.ToString());

                            RESPONSE.resultado = "ERROR";
                            RESPONSE.descripcion = ex.Message.Trim();
                            RESPONSE.resultado_codigo = ERROR.ErrID;
                            RESPONSE.resultado_descripcion = ERROR.Mensaje;
                            STATUS_CODE = HttpStatusCode.BadRequest;
                            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                            #endregion
                        }

                        if (SOLDESPDETJSON.Count > 0)
                        {
                            if (SOLDESPDETJSON[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                            {
                                #region NOK
                                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017); //REQUEST - ERROR PROCESO BASE DE DATOS

                                LogInfo(NombreProceso,
                                        ERROR.Mensaje.Trim() + ". " + "Error procesando Articulo: " + item2.CodigoArticulo + ", " + SOLDESPDETJSON[0].Descripcion,
                                        true,
                                        true,
                                        REQUEST.NumeroReferencia,
                                        body.ToString());

                                RESPONSE.resultado = "ERROR";
                                RESPONSE.descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + SOLDESPDETJSON[0].Descripcion;
                                RESPONSE.resultado_codigo = ERROR.ErrID;
                                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                                RESPONSE.solDespId = 0;                                
                                STATUS_CODE = HttpStatusCode.BadRequest;
                                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                #endregion
                            }
                        }
                    }// fin ciclo for each detalle -------

                    try
                    {
                        //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                        SOLDESPVALIDA = GP_ENT.sp_proc_API_SolDespachoValidaJson(REQUEST.Empid,
                                                                                 USERNAME,
                                                                                 RESPONSE.solDespId,
                                                                                 ListadoCodigoArticulo,
                                                                                 REQUEST.Items.Count
                                                                                 ).ToList();
                    }
                    catch (Exception ex)
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                                true,
                                true,
                                REQUEST.NumeroReferencia,
                                body.ToString());

                        RESPONSE.resultado = "ERROR";
                        RESPONSE.descripcion = ex.Message.Trim();
                        RESPONSE.resultado_codigo = ERROR.ErrID;
                        RESPONSE.resultado_descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }

                    if (SOLDESPVALIDA.Count > 0)
                    {
                        if (SOLDESPVALIDA[0].Resultado != "OK")
                        {
                            #region NOK
                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - VALIDACION CONSISTENCIA DE DATOS

                            LogInfo(NombreProceso,
                                    ERROR.Mensaje.Trim() + ". " + SOLDESPVALIDA[0].Descripcion,
                                    true,
                                    true,
                                    REQUEST.NumeroReferencia,
                                    body.ToString());

                            RESPONSE.resultado = "ERROR";
                            RESPONSE.descripcion = SOLDESPVALIDA[0].Descripcion;
                            RESPONSE.resultado_codigo = ERROR.ErrID;
                            RESPONSE.resultado_descripcion = ERROR.Mensaje;
                            RESPONSE.solDespId = 0;
                            STATUS_CODE = HttpStatusCode.BadRequest;
                            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                            #endregion
                        }
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017); //REQUEST - ERROR PROCESO BASE DE DATOS

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + SOLDESPJSON[0].Descripcion,
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = SOLDESPJSON[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        #region SUBIR IMAGEN (19)
        [HttpPost]
        [Route("SUBIRIMG")]
        public IHttpActionResult recurso19([FromBody] API_REQUEST_TYPE_15 REQUEST)
        {
            API_RESPONSE_TYPE_11 RESPONSE = new API_RESPONSE_TYPE_11();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + ". " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.imgBase64 = (REQUEST.imgBase64 == null ? "" : REQUEST.imgBase64);

            #region LARGOS PERMITIDOS
            //try
            //{
            //    try
            //    {
            //        if (REQUEST.fechaReferencia.Equals(""))
            //        {
            //            fRef = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
            //        }
            //        else
            //        {
            //            fRef = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
            //        }
            //    }
            //    catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            //    try
            //    {
            //        if (REQUEST.fechaRequerida.Equals(""))
            //        {
            //            fReq = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
            //        }
            //        else
            //        {
            //            fReq = DateTime.ParseExact(REQUEST.fechaRequerida, "dd-MM-yyyy", null);
            //        }
            //    }
            //    catch { throw new Exception("ERROR - FECHAREQUERIDA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            //}
            //catch (Exception ex)
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
            //    RESPONSE.resultado = "ERROR";
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion

            List<sp_in_API_Image_Result> IMG = GP_ENT.sp_in_API_Image(USERNAME,
                                                                      REQUEST.empid,
                                                                      REQUEST.nombreArchivo
                                                                      ).ToList();
            if (IMG.ElementAt(0).Resultado > 0)
            {
                try
                {
                    if (Directory.Exists(ConfigurationManager.AppSettings["ImgCliente"]))
                    {
                        Console.WriteLine("That path exists already.");
                    }
                    else
                    {
                        Directory.CreateDirectory(ConfigurationManager.AppSettings["ImgCliente"]);
                    }

                    //string nombrearchivo1 = "";
                    string image1 = "";
                    string extencion = "";
                    extencion = ConfigurationManager.AppSettings["Extension"].ToString();

                    byte[] dataBuffer = Convert.FromBase64String(REQUEST.imgBase64);
                    //nombrearchivo1 = SGA_MVMovilEvalAdj.UserName.Trim() + "_" + SGA_MVMovilEvalAdj.Proyecto + "_" + SGA_MVMovilEvalAdj.TareaId + "_" + SGA_MVMovilEvalAdj.Pregunta + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + extencion.Trim();

                    image1 = ConfigurationManager.AppSettings["ImgCliente"] + REQUEST.nombreArchivo + extencion;

                    var nameFile = REQUEST.nombreArchivo + extencion;

                    System.IO.FileStream stream = new FileStream(image1, FileMode.CreateNew);
                    System.IO.BinaryWriter writer =
                        new BinaryWriter(stream);
                    writer.Write(dataBuffer, 0, dataBuffer.Length);
                    writer.Close();
                }
                catch (Exception ex)
                {
                    //return Ok(ex.InnerException);
                    return Unauthorized();
                }

                RESPONSE.count = (int)IMG.ElementAt(0).Resultado;
                RESPONSE.resultado = "OK";
                RESPONSE.resultado_descripcion = (string)IMG.ElementAt(0).Descripcion;
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.count = (int)IMG.ElementAt(0).Resultado;
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = (string)IMG.ElementAt(0).Descripcion;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region CREAR ADJ SDD (22)
        [HttpPost]
        [Route("SOLDESPACHOADJ/CREAR")]
        public IHttpActionResult recurso22(API_REQUEST_TYPE_22 REQUEST)
        {
            API_RESPONSE_TYPE_13 RESPONSE = new API_RESPONSE_TYPE_13();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //#region VALIDA EMPRESA NOK
            //if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            //{
            //    RESPONSE.resultado = 0;
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje;
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //}
            //#endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            //RESPONSE.resultado = "OK";
            //RESPONSE.resultado_descripcion = "";
            //RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.archivo = (REQUEST.archivo == null ? "" : REQUEST.archivo);
            #endregion
            try
            {
                List<sp_in_API_SolDespachoAdj_Result> SDDADJ = GP_ENT.sp_in_API_SolDespachoAdj(
                USERNAME,
                REQUEST.solDespId,
                REQUEST.archivo
                ).ToList();

                RESPONSE.resultado_codigo = SDDADJ.ElementAt(0).Resultado;
                RESPONSE.resultado_descripcion = SDDADJ.ElementAt(0).Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();

                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region ACTUALIZA ESTADO SDD (23)
        [HttpGet]
        [HttpPost]
        [Route("ESTADOSDD/ACTUALIZAR")]
        public IHttpActionResult recurso23(API_REQUEST_TYPE_23 REQUEST)
        {
            API_RESPONSE_TYPE_24_DET RESPONSE = new API_RESPONSE_TYPE_24_DET();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            //RESPONSE.resultado = "OK";
            //RESPONSE.resultado_descripcion = "";
            //RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            #endregion
            try
            {
                List<sp_upd_cambEstado_SDD_Api_Result> estsdd = GP_ENT.sp_upd_cambEstado_SDD_Api(REQUEST.EmpId,
                                                                                                 USERNAME,
                                                                                                 REQUEST.TipoReferencia,
                                                                                                 REQUEST.NumeroReferencia,
                                                                                                 REQUEST.SDD,
                                                                                                 REQUEST.Estado).ToList();

                RESPONSE.resultado_codigo = estsdd.ElementAt(0).resultado_codigo;
                RESPONSE.resultado_descripcion = estsdd.ElementAt(0).resultado_descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado = "Error";
                RESPONSE.descripcion = ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region ACTUALIZA ESTADO SDR (24)
        [HttpGet]
        [HttpPost]
        [Route("ESTADOSDR/ACTUALIZAR")]
        public IHttpActionResult recurso24(API_REQUEST_TYPE_24 REQUEST)
        {
            API_RESPONSE_TYPE_19 RESPONSE = new API_RESPONSE_TYPE_19();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region PROCESAMIENTO
            STATUS_CODE = HttpStatusCode.OK;
            //RESPONSE.resultado = "OK";
            //RESPONSE.resultado_descripcion = "";
            //RESPONSE.resultado_codigo = 0;

            try
            {
                List<sp_upd_cambEstado_SDR_Api_Result> estsdr = GP_ENT.sp_upd_cambEstado_SDR_Api(REQUEST.EmpId,
                                                                                                 USERNAME,
                                                                                                 REQUEST.TipoReferencia,
                                                                                                 REQUEST.NumeroReferencia,
                                                                                                 REQUEST.SDR,
                                                                                                 REQUEST.Estado).ToList();
                RESPONSE.resultado_codigo = estsdr.ElementAt(0).resultado_codigo;
                RESPONSE.resultado_descripcion = estsdr.ElementAt(0).resultado_descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region CONSULTA STOCK BODEGA UBICACIÓN (25)
        [HttpGet]
        [HttpPost]
        [Route("STOCKBODEGAUBI/LISTAR")]
        public IHttpActionResult recurso25(API_REQUEST_TYPE_20 REQUEST)
        {
            API_RESPONSE_TYPE_16 RESPONSE = new API_RESPONSE_TYPE_16();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            //RESPONSE.resultado = "OK";
            //RESPONSE.resultado_descripcion = "";
            //RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            #endregion
            try
            {
                List<sp_sel_API_Consulta_Stock_Bodega_Ubicacion_Result> csbur = GP_ENT.sp_sel_API_Consulta_Stock_Bodega_Ubicacion(REQUEST.empId,
                                                                                                                                  REQUEST.lineaProducto,
                                                                                                                                  REQUEST.tipoProducto,
                                                                                                                                  REQUEST.CodigoArticulo,
                                                                                                                                  USERNAME).ToList();
                if (csbur.Count != 0)
                {
                    RESPONSE.count = csbur.ElementAt(0).Count;
                    RESPONSE.resultado = csbur.ElementAt(0).Resultado;
                    RESPONSE.codigoBodega = csbur.ElementAt(0).CodigoBodega;
                    RESPONSE.glosaBodega = csbur.ElementAt(0).GlosaBodega;

                    //foreach (var item in csbur) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Ubicacion_Result), item); }

                    RESPONSE.detalle = csbur.Select(m => new {codigoArticulo = m.CodigoArticulo,
                                                              codigoUbicacion = m.CodigoUbicacion,
                                                              descripcion = m.Descripcion,
                                                              stock = m.Stock}).Cast<dynamic>().ToList();
                }
                else
                {
                    RESPONSE.count = 0;
                    RESPONSE.resultado = "Vacío";
                    RESPONSE.resultado_codigo = 0;
                    RESPONSE.resultado_descripcion = "No se han encontrado registros";
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO 
                RESPONSE.resultado = "Error";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        #region BSALE WEBHOOK (26)
        [HttpPost]
        [Route("WebhookBsale")]
        //public IHttpActionResult recurso26(string data)
        public IHttpActionResult recurso26(object data)
        {

            API_RESPONSE_TYPE_17 RESPONSE = new API_RESPONSE_TYPE_17();
            ERROR = new API_RESPONSE_ERRORS();

            //TFunction fun = new TFunction();
            //if (Directory.Exists(ConfigurationSettings.AppSettings["Log"]))
            //{
            //    Console.WriteLine("That path exists already.");
            //}
            //else
            //{
            //    Directory.CreateDirectory(ConfigurationSettings.AppSettings["Log"]);
            //}

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            try
            {
                List<sp_in_API_Webhook_Bsale_Result> WBSL = GP_ENT.sp_in_API_Webhook_Bsale(data.ToString()).ToList();

                RESPONSE.Resultado = WBSL.ElementAt(0).Resultado;
                RESPONSE.Descripcion = WBSL.ElementAt(0).Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        //Trae listado de Confirmaciones pendientes de enviar en tabla L_IntegraConfirmaciones (Estado 1) ==========
        #region INTEGRACIÓN CONFIRMACIONES JSON (27)
        [HttpPost]
        [HttpGet]
        [Route("INTEGRACION_CONFIRMACIONES_JSON/LISTAR")]
        public IHttpActionResult recurso27(API_REQUEST_TYPE_27 REQUEST)
        {
            API_RESPONSE_TYPE_27 RESPONSE = new API_RESPONSE_TYPE_27();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido ---
            #region VALIDA JSON RECIBIDO NOK

            //valida tipos de datos recibidos en el json coincidan con la estructura que se espera---
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            try
            {
                List<sp_sel_API_IntegraConfirmacionesJson_Result> CONFIRMACIONES = GP_ENT.sp_sel_API_IntegraConfirmacionesJson(REQUEST.EmpId, 
                                                                                                                               REQUEST.NombreProceso, 
                                                                                                                               REQUEST.Limit, 
                                                                                                                               REQUEST.RowSet).ToList();
                foreach (var item in CONFIRMACIONES)
                {
                    HELPERS.TrimModelProperties(typeof(sp_sel_API_IntegraConfirmacionesJson_Result), item);
                }

                //Si retorna resultados carga JSON de retorno -----
                if (CONFIRMACIONES.Count > 0)
                {
                    RESPONSE.Count = CONFIRMACIONES.Count;

                    long Id_integracion = 0;

                    API_RESPONSE_TYPE_27_CAB RESPONSE_CAB = new API_RESPONSE_TYPE_27_CAB();
                    API_RESPONSE_TYPE_27_DET RESPONSE_DET = new API_RESPONSE_TYPE_27_DET();

                    foreach (var Confirmacion in CONFIRMACIONES)
                    {
                        if (Confirmacion.IdCab != Id_integracion) //Si cambia de Cabecera de integracion
                        {
                            if (Id_integracion != 0) //Si no es el primer id que esta procesando, debe guardar el id que esta completando y debe comenzar con uno nuevo ----
                            {
                                RESPONSE.Confirmacion.Add(RESPONSE_CAB);
                            }

                            //Inicializa variable de Cabecera ------------
                            RESPONSE_CAB = new API_RESPONSE_TYPE_27_CAB();

                            RESPONSE_CAB.Id = Confirmacion.IdCab;
                            RESPONSE_CAB.EmpId = REQUEST.EmpId;
                            RESPONSE_CAB.UserName = Confirmacion.UserName;
                            RESPONSE_CAB.NombreProceso = Confirmacion.NombreProceso;
                            RESPONSE_CAB.FechaProceso = Confirmacion.FechaProceso.ToString("dd-MM-yyyy");
                            RESPONSE_CAB.Folio = long.Parse(Confirmacion.Folio.ToString());
                            RESPONSE_CAB.FolioRel = long.Parse(Confirmacion.FolioRel.ToString());
                            //RESPONSE_CAB.Estado = 1;
                            RESPONSE_CAB.FechaEstado = Confirmacion.FechaEstado.ToString("dd-MM-yyyy");
                            RESPONSE_CAB.TipoDocumento = Confirmacion.TipoDocumento;
                            RESPONSE_CAB.NumeroDocto = Confirmacion.NumeroDocto;
                            RESPONSE_CAB.FechaDocto = Confirmacion.FechaDocto.ToString("dd-MM-yyyy");
                            RESPONSE_CAB.TipoReferencia = Confirmacion.TipoReferencia;
                            RESPONSE_CAB.NumeroReferencia = Confirmacion.NumeroReferencia;
                            RESPONSE_CAB.FechaReferencia = Confirmacion.FechaReferencia.ToString("dd-MM-yyyy");
                            RESPONSE_CAB.RutCliente = Confirmacion.RutCliente;
                            RESPONSE_CAB.RazonSocial = Confirmacion.RazonSocial;
                            //RESPONSE_CAB.TipoIntegracion = 1
                            RESPONSE_CAB.Texto1 = Confirmacion.Texto1Cab;

                            Id_integracion = Confirmacion.IdCab;
                        }

                        //Inicializa variable de Item -------------
                        RESPONSE_DET = new API_RESPONSE_TYPE_27_DET();

                        RESPONSE_DET.Id = Confirmacion.IdDet;
                        RESPONSE_DET.IntId = Confirmacion.IdCab;
                        RESPONSE_DET.Linea = Confirmacion.Linea;
                        RESPONSE_DET.ItemReferencia = Confirmacion.ItemReferencia;
                        RESPONSE_DET.Cadena = Confirmacion.Cadena;
                        RESPONSE_DET.CodigoArticulo = Confirmacion.CodigoArticulo;
                        RESPONSE_DET.NroSerieDesp = Confirmacion.NroSerieDesp;
                        RESPONSE_DET.FechaVectoDesp = Confirmacion.FechaVectoDesp.ToString("dd-MM-yyyy");
                        RESPONSE_DET.UnidadMedida = Confirmacion.UnidadMedida;
                        RESPONSE_DET.Cantidad = float.Parse(Confirmacion.Cantidad.ToString());
                        RESPONSE_DET.CantidadProc = float.Parse(Confirmacion.CantidadProc.ToString());
                        RESPONSE_DET.Texto1 = Confirmacion.Texto1Det;
                        RESPONSE_DET.Texto2 = Confirmacion.Texto2Det;
                        RESPONSE_DET.Texto3 = Confirmacion.Texto3Det;
                        RESPONSE_DET.Estado = Confirmacion.Estado;

                        RESPONSE_CAB.Items.Add(RESPONSE_DET);
                    }

                    //Agrega ultimo item que estaba procesando al JSON
                    RESPONSE.Confirmacion.Add(RESPONSE_CAB);
                }

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error al ejecutar";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
        }

        #region Metodo Original --- INTEGRACIÓN CONFIRMACIONES JSON (27)

        //[HttpPost]
        //[Route("integracion_confirmaciones_json/LISTAR")]
        //public IHttpActionResult recurso27(API_REQUEST_TYPE_27 REQUEST)
        //{
        //    API_REPONSE_TYPE_18 RESPONSE = new API_REPONSE_TYPE_18();
        //    ERROR = new API_RESPONSE_ERRORS();
        //    string USERNAME = "";
        //    HttpStatusCode STATUS_CODE = new HttpStatusCode();

        //    #region VALIDA ACCESO API/RECURSO NOK
        //    CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
        //    if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
        //    {
        //        RESPONSE.Resultado_Codigo = ERROR.ErrID;
        //        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
        //        STATUS_CODE = HttpStatusCode.Unauthorized;
        //        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        //    }
        //    #endregion

        //    #region VALIDA EMPRESA NOK
        //    if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
        //    {
        //        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
        //        RESPONSE.Resultado_Codigo = ERROR.ErrID;
        //        STATUS_CODE = HttpStatusCode.Unauthorized;
        //        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        //    }
        //    #endregion

        //    #region RESPONSE OK
        //    STATUS_CODE = HttpStatusCode.OK;
        //    #region VALIDACIONES DE CAMPOS
        //    #endregion
        //    try
        //    {
        //        var model = GP_ENT.sp_sel_API_IntegraConfirmacionesJson(REQUEST.EmpId, REQUEST.NombreProceso, REQUEST.Limit, REQUEST.RowSet).ToList();
        //        if (model.Count() == 0)
        //        {
        //            RESPONSE.Count = 0;
        //            RESPONSE.Resultado = "Vacío";
        //            RESPONSE.Resultado_Codigo = 0;
        //            RESPONSE.Resultado_Descripcion = "No se han encontrado registros";
        //            STATUS_CODE = HttpStatusCode.Unauthorized;
        //            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

        //        }
        //        var consulta = from Resultado in model
        //                       group Resultado by new
        //                       {
        //                           Resultado.Count,
        //                           Resultado.Resultado,
        //                           Resultado.Resultado_Descripcion,
        //                           Resultado.Resultado_Codigo,
        //                       } into Resultado
        //                       select new
        //                       {
        //                           Count = Resultado.Key.Count,
        //                           Resultado = Resultado.Key.Resultado,
        //                           ResultadoDescripcion = Resultado.Key.Resultado_Descripcion,
        //                           ResultadoCodigo = Resultado.Key.Resultado_Codigo,
        //                           Confirmacion = from Confirmacion in model
        //                                          join conf in Resultado
        //                                              on Confirmacion.Id equals conf.Id
        //                                          group Confirmacion by new
        //                                          {
        //                                              Confirmacion.Id,
        //                                              Confirmacion.IntId,
        //                                              Confirmacion.UserName,
        //                                              Confirmacion.NombreProceso,
        //                                              Confirmacion.FechaProceso,
        //                                              Confirmacion.Folio,
        //                                              Confirmacion.FolioRel,
        //                                              Confirmacion.Estado,
        //                                              Confirmacion.FechaEstado,
        //                                              Confirmacion.TipoDocumento,
        //                                              Confirmacion.NumeroDocto,
        //                                              Confirmacion.FechaDocto,
        //                                              Confirmacion.TipoReferencia,
        //                                              Confirmacion.NumeroReferencia,
        //                                              Confirmacion.FechaReferencia,
        //                                              Confirmacion.RutCliente,
        //                                              Confirmacion.RazonSocial,
        //                                          } into Confirmacion
        //                                          select new
        //                                          {
        //                                              Id = Confirmacion.Key.Id,
        //                                              IntId = Confirmacion.Key.IntId,
        //                                              UserName = Confirmacion.Key.UserName,
        //                                              NombreProceso = Confirmacion.Key.NombreProceso,
        //                                              FechaProceso = Confirmacion.Key.FechaProceso,
        //                                              Folio = Confirmacion.Key.Folio,
        //                                              FolioRel = Confirmacion.Key.FolioRel,
        //                                              Estado = Confirmacion.Key.Estado,
        //                                              FechaEstado = Confirmacion.Key.FechaEstado,
        //                                              TipoDocumento = Confirmacion.Key.TipoDocumento,
        //                                              NumeroDocto = Confirmacion.Key.NumeroDocto,
        //                                              FechaDocto = Confirmacion.Key.FechaDocto,
        //                                              TipoReferencia = Confirmacion.Key.TipoReferencia,
        //                                              NumeroReferencia = Confirmacion.Key.NumeroReferencia,
        //                                              FechaReferencia = Confirmacion.Key.FechaReferencia,
        //                                              RutCliente = Confirmacion.Key.RutCliente,
        //                                              RazonSocial = Confirmacion.Key.RazonSocial,
        //                                              Items = from Items in Confirmacion
        //                                                      join its in Resultado on Items.Linea equals its.Linea
        //                                                      group Items by new
        //                                                      {
        //                                                          Items.Linea,
        //                                                          Items.ItemReferencia,
        //                                                          Items.Cadena,
        //                                                          Items.CodigoArticulo,
        //                                                          Items.NroSerieDesp,
        //                                                          Items.FechaVectoDesp,
        //                                                          Items.UnidadMedida,
        //                                                          Items.Cantidad,
        //                                                          Items.CantidadProc,
        //                                                      } into Items
        //                                                      select new
        //                                                      {
        //                                                          Linea = Items.Key.Linea,
        //                                                          ItemReferencia = Items.Key.ItemReferencia,
        //                                                          Cadena = Items.Key.Cadena,
        //                                                          CodigoArticulo = Items.Key.CodigoArticulo,
        //                                                          NroSerieDesp = Items.Key.NroSerieDesp,
        //                                                          FechaVectoDesp = Items.Key.FechaVectoDesp,
        //                                                          UnidadMedida = Items.Key.UnidadMedida,
        //                                                          Cantidad = Items.Key.Cantidad,
        //                                                          CantidadProc = Items.Key.CantidadProc,
        //                                                      }
        //                                          }

        //                       };
        //        return Ok(consulta.ElementAt(0));
        //    }
        //    catch (Exception ex)
        //    {
        //        #region NOK
        //        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
        //        RESPONSE.Resultado_Codigo = ERROR.ErrID;
        //        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
        //        STATUS_CODE = HttpStatusCode.Unauthorized;
        //        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        //        #endregion
        //    }
        //    #endregion
        //}

        #endregion

        #endregion

        #region INTEGRACIÓN CONFIRMACIONES CSV (28)
        [HttpPost]
        [Route("integracion_confirmaciones_csv/LISTAR")]
        public IHttpActionResult recurso28(API_REQUEST_TYPE_27 REQUEST)
        {
            API_RESPONSE_TYPE_18 RESPONSE = new API_RESPONSE_TYPE_18();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            #region VALIDACIONES DE CAMPOS
            #endregion
            try
            {
                var model = GP_ENT.sp_sel_API_IntegraConfirmacionesCSV(REQUEST.EmpId, REQUEST.NombreProceso, REQUEST.Limit, REQUEST.RowSet).ToList();
                var aux = model.Count();
                if (model.Count() == 0)
                {
                    RESPONSE.Count = 0;
                    RESPONSE.Resultado = "Vacío";
                    RESPONSE.Resultado_Codigo = 0;
                    RESPONSE.Resultado_Descripcion = "No se han encontrado registros";
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

                }
                var consulta = from Resultado in model
                               group Resultado by new
                               {
                                   Resultado.Count,
                                   Resultado.Resultado,
                                   Resultado.Resultado_Descripcion,
                                   Resultado.Resultado_Codigo,
                               } into Resultado
                               select new
                               {
                                   Count = Resultado.Key.Count,
                                   Resultado = Resultado.Key.Resultado,
                                   ResultadoDescripcion = Resultado.Key.Resultado_Descripcion,
                                   ResultadoCodigo = Resultado.Key.Resultado_Codigo,
                                   Confirmacion = from Confirmacion in model
                                                  join conf in Resultado
                                                      on Confirmacion.Id equals conf.Id
                                                  group Confirmacion by new
                                                  {
                                                      Confirmacion.Id,
                                                      Confirmacion.IntId,
                                                      Confirmacion.UserName,
                                                      Confirmacion.NombreProceso,
                                                      Confirmacion.FechaProceso,
                                                      Confirmacion.Folio,
                                                      Confirmacion.FolioRel,
                                                      Confirmacion.Estado,
                                                      Confirmacion.FechaEstado,
                                                      Confirmacion.TipoDocumento,
                                                      Confirmacion.NumeroDocto,
                                                      Confirmacion.FechaDocto,
                                                      Confirmacion.TipoReferencia,
                                                      Confirmacion.NumeroReferencia,
                                                      Confirmacion.FechaReferencia,
                                                      Confirmacion.RutCliente,
                                                      Confirmacion.RazonSocial,
                                                  } into Confirmacion
                                                  select new
                                                  {
                                                      Id = Confirmacion.Key.Id,
                                                      IntId = Confirmacion.Key.IntId,
                                                      UserName = Confirmacion.Key.UserName,
                                                      NombreProceso = Confirmacion.Key.NombreProceso,
                                                      FechaProceso = Confirmacion.Key.FechaProceso,
                                                      Folio = Confirmacion.Key.Folio,
                                                      FolioRel = Confirmacion.Key.FolioRel,
                                                      Estado = Confirmacion.Key.Estado,
                                                      FechaEstado = Confirmacion.Key.FechaEstado,
                                                      TipoDocumento = Confirmacion.Key.TipoDocumento,
                                                      NumeroDocto = Confirmacion.Key.NumeroDocto,
                                                      FechaDocto = Confirmacion.Key.FechaDocto,
                                                      TipoReferencia = Confirmacion.Key.TipoReferencia,
                                                      NumeroReferencia = Confirmacion.Key.NumeroReferencia,
                                                      FechaReferencia = Confirmacion.Key.FechaReferencia,
                                                      RutCliente = Confirmacion.Key.RutCliente,
                                                      RazonSocial = Confirmacion.Key.RazonSocial,
                                                      Items = from Items in Confirmacion
                                                              join its in Resultado
                                                                  on Items.Texto1 equals its.Texto1
                                                              group Confirmacion by new
                                                              {
                                                                  Items.Texto1,
                                                              } into Items
                                                              select new
                                                              {
                                                                  Texto1 = Items.Key.Texto1,
                                                              }
                                                  }
                               };
                return Ok(consulta.ElementAt(0));
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
        }
        #endregion

        //Marca como Traspasado un registro de la tabla L_IntegraConfirmaciones (Estado 2)
        #region INTEGRACIÓN CONFIRMACIONES UPD (29)
        [HttpPost]
        [Route("INTEGRACION_CONFIRMACIONES_UPD")]
        public IHttpActionResult recurso29(API_REQUEST_TYPE_29 REQUEST)
        {
            API_RESPONSE_TYPE_18 RESPONSE = new API_RESPONSE_TYPE_18();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida estructura JSON recibido sea valida
            #region VALIDACIONES JSON RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim() + ". " + ERROR.Mensaje.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = "El json viene vacio" + ". " + ERROR.Mensaje.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region PROCESAMIENTO

            //setea variable de respuesta en OK ----
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Resultado_Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES
            try
            {
                if (REQUEST.IntId <= 0) { throw new Exception("Debe informar IntId"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ex.Message.Trim() + ". " + ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            try
            {
                List<sp_upd_API_IntegraConfirmaciones_Result> resultado = GP_ENT.sp_upd_API_IntegraConfirmaciones(REQUEST.IntId).ToList();

                RESPONSE.Resultado = resultado.ElementAt(0).Resultado;
                RESPONSE.Resultado_Codigo = resultado.ElementAt(0).Resultado_Codigo;
                RESPONSE.Resultado_Descripcion = resultado.ElementAt(0).Resultado_Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 30 CREAR PRODUCTO JSON (30) ======================================================================
        #region CREAR PRODUCTO JSON (30)
        [HttpPost]
        [Route("PRODUCTO/CREARJSON")]
        public IHttpActionResult recurso30([FromBody] API_REQUEST_TYPE_30 REQUEST)
        {
            API_RESPONSE_TYPE_30 RESPONSE = new API_RESPONSE_TYPE_30();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "PRODUCTO/CREARJSON";

            //Valida estructura JSON recibido sea valida
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "El json viene vacio", true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "El json viene vacio";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            if (!ModelState.IsValid) //si hay errores en el json
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, ERROR.Mensaje.Trim(), true, true, "", "");

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa (la empresa la saca del REQUEST, primero valida que venga un JSON valido)
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                LogInfo(NombreProceso, ERROR.Mensaje.Trim(), true, true, "", "");
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.count = 0;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            //Valida que el json tenga items ---
            if (REQUEST.Articulos == null) { throw new Exception("No vienen articulos en el json"); } //requerido
            if (REQUEST.Articulos.Count == 0) { throw new Exception("No vienen articulos en el json"); } //requerido

            //Guarda JSON recibido como parametro ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Creación Producto, JSON: " + body.ToString(),
                    false,
                    true,
                    "Producto",
                    body.ToString());

            try
            {
                //recorre el array de Articulos en el json
                foreach (var Articulo in REQUEST.Articulos)
                {
                    if (Articulo.CodigoArticulo == null || Articulo.CodigoArticulo == "") { throw new Exception("Debe enviar Codigo Articulo (CodigoArticulo)"); } //requerido
                    if (Articulo.DescripArt == null || Articulo.DescripArt == "") { throw new Exception("Debe enviar descripción del producto (DescripArt)"); } //requerido
                    if (Articulo.DescripTecnica == null || Articulo.DescripTecnica == "") { throw new Exception("Debe enviar descripción Técnica del producto (DescripTecnica)"); } //requerido
                    if (Articulo.DescripCorta == null || Articulo.DescripCorta == "") { throw new Exception("Debe enviar descripción corta del producto"); } //requerido
                                        
                    //Validacion especial LineaProducto -----------------------------------------------------------------------------------------------
                    //La version estandar para los clientes exige linea Producto
                    //La opcion especial se envia CodigoExt y GlosaLinea para buscar codigo correspondiente o crearlo

                    Articulo.CodigoExt = (Articulo.CodigoExt == null ? "" : Articulo.CodigoExt); //opcional
                    Articulo.GlosaLineaProducto = (Articulo.GlosaLineaProducto == null ? "" : Articulo.GlosaLineaProducto); //opcional

                    if (Articulo.CodigoExt == "" && Articulo.GlosaLineaProducto == "") //Version estandar, no trae CodigoExt ni su Glosa, debe pedir lineaProducto
                    {
                        if (Articulo.LineaProducto <= 0) { throw new Exception("Debe informar Linea Producto > 0 (LineaProducto)"); } //requerido
                    }
                    else //Version especial
                    {
                        if (Articulo.CodigoExt == "" || Articulo.GlosaLineaProducto == "") { throw new Exception("Si utiliza Codigo Externo debe enviar Codigo y su Glosa"); } //requerido
                        Articulo.LineaProducto = 0;

                    } //FIN Validacion especial LineaProducto -----------------------------------------------------------------------------------------

                    if (Articulo.TipoProducto == null || Articulo.TipoProducto == "") { throw new Exception("Debe informar Tipo Producto"); } //requerido
                    if (Articulo.TipoArticulo <= 0) { throw new Exception("Debe informar Tipo Articulo > 0 (TipoArticulo)"); } //requerido
                    if (Articulo.VigenciaDesde == null || Articulo.VigenciaDesde == "" || Articulo.VigenciaDesde == "01-01-1900") { throw new Exception("Debe informar fecha vigencia desde (VigenciaDesde)"); } //requerido
                    if (Articulo.VigenciaHasta == null || Articulo.VigenciaHasta == "" || Articulo.VigenciaHasta == "01-01-1900") { throw new Exception("Debe informar fecha vigencia hasta (VigenciaHasta)"); } //requerido

                    Articulo.Rotacion = (Articulo.Rotacion == null ? "" : Articulo.Rotacion); //opcional
                    Articulo.CodigoFabrica = (Articulo.CodigoFabrica == null ? "" : Articulo.CodigoFabrica); //opcional
                    if (Articulo.UsaSerie != 0 && Articulo.UsaSerie != 1) { throw new Exception("Debe indicar si usa o no Serie, 0 o 1 (UsaSerie)"); } //requerido
                    if (Articulo.UsaLote != 0 && Articulo.UsaLote != 1) { throw new Exception("Debe indicar si usa o no Lote, 0 o 1 (UsaLote)"); } //requerido
                    if (Articulo.EAN13 == null || Articulo.EAN13 == "") { throw new Exception("Debe informar código EAN13 (EAN13)"); } //requerido
                    Articulo.DUN14 = (Articulo.DUN14 == null ? "" : Articulo.DUN14); //opcional
                    Articulo.UnidadMedidaCompra = (Articulo.UnidadMedidaCompra == null ? "" : Articulo.UnidadMedidaCompra); //opcional
                    Articulo.UnidadMedidaVenta = (Articulo.UnidadMedidaVenta == null ? "" : Articulo.UnidadMedidaVenta); //opcional
                    Articulo.CodigoProveedor = (Articulo.CodigoProveedor == null ? "" : Articulo.CodigoProveedor); //opcional
                    Articulo.Marca = (Articulo.Marca == null ? "" : Articulo.Marca); //opcional

                    Articulo.Origen = (Articulo.Origen == null ? "" : Articulo.Origen); //opcional

                    //if (Articulo.UnidadMedida == null || Articulo.UnidadMedida == "") { throw new Exception("Debe informar Unidad de Medida (UnidadMedida)"); } //requerido
                    //if (Articulo.EstadoRecep <= 0) { throw new Exception("Debe informar Estado recepcion por defecto (EstadoRecep)"); } //requerido
                    //if (Articulo.TipoHUId <= 0) { throw new Exception("Debe Tipo HU (TipoHUId)"); } //requerido
                    //if (Articulo.SubTipoHUId <= 0) { throw new Exception("Debe informar subtipo HU (SubTipoHUId)"); } //requerido

                    Articulo.Dato1 = (Articulo.Dato1 == null ? "" : Articulo.Dato1); //opcional
                    Articulo.Dato2 = (Articulo.Dato2 == null ? "" : Articulo.Dato2); //opcional
                    Articulo.Dato3 = (Articulo.Dato3 == null ? "" : Articulo.Dato3); //opcional
                    //decimal Valor1 //opcional numerico, toma cero
                    //decimal Valor2 //opcional numerico, toma cero
                    //decimal Valor3 //opcional numerico, toma cero
                    Articulo.Fecha1 = (Articulo.Fecha1 == null ? "" : Articulo.Fecha1); //opcional                    
                    Articulo.Fecha2 = (Articulo.Fecha2 == null ? "" : Articulo.Fecha2); //opcional
                    Articulo.Fecha3 = (Articulo.Fecha3 == null ? "" : Articulo.Fecha3); //opcional

                    //Valida que el json tenga Unidades de Medida para los Articulos ---
                    if (Articulo.UnidadMedida == null) { throw new Exception("No vienen Unidades de medida en el json para el producto " + Articulo.CodigoArticulo.Trim()); } //requerido
                    if (Articulo.UnidadMedida.Count == 0) { throw new Exception("No vienen Unidades de medida en el json para el producto " + Articulo.CodigoArticulo.Trim()); } //requerido

                    foreach (var UnidadMedida in Articulo.UnidadMedida.ToList())
                    {
                        if (UnidadMedida.CodigoUM == null || UnidadMedida.CodigoUM == "") { throw new Exception("Debe enviar Unidad Medida (CodigoUM), producto Codigo: " +
                                                                                                                Articulo.CodigoArticulo.Trim()); } //requerido
                        //UnidadMedida.PesoNeto //opcional
                        //UnidadMedida.PesoBruto //opcional
                        //UnidadMedida.Ancho //opcional
                        //UnidadMedida.Alto //opcional
                        //UnidadMedida.Profundidad //opcional
                        //UnidadMedida.Volumen //opcional
                        if (UnidadMedida.Factor <= 0) { throw new Exception("Debe informar Factor para la unidad de medida " + UnidadMedida.CodigoUM.Trim() + 
                                                                            ", producto codigo: " + Articulo.CodigoArticulo.Trim()); } //requerido
                        //UnidadMedida.UNBase //opcional
                        //UnidadMedida.UNTotal //opcional
                        //UnidadMedida.TiempoRecep //opcional
                        //UnidadMedida.TiempoDesp //opcional
                    }

                    //Valida datos de Items json, si trae detalles de KIT los campos son obligatorios
                    if (Articulo.Kit != null)
                    {
                        foreach (var Kit1 in Articulo.Kit)
                        {
                            if (Kit1.CodigoArticulo == null || Kit1.CodigoArticulo == "") { throw new Exception("Debe informar Producto Kit"); } //requerido
                            if (Kit1.CantidadRequerida <= 0) { throw new Exception("Debe informar Cantidad Item Kit " + Kit1.CodigoArticulo.ToString().Trim() + ", requerido debe ser > 0"); } //requerido
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(), true, true, "", body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA FECHAS INGRESADAS

            DateTime VigenciaDesde = new DateTime();
            DateTime VigenciaHasta = new DateTime();
            DateTime Fecha1 = new DateTime();
            DateTime Fecha2 = new DateTime();
            DateTime Fecha3 = new DateTime();

            try
            {
                #region VALIDA ITEMS DETALLE JSON
                foreach (var Articulo in REQUEST.Articulos.ToList())
                {
                    VigenciaDesde = ValidaCampoFecha(Articulo.VigenciaDesde, "Fecha Vigencia desde");
                    VigenciaHasta = ValidaCampoFecha(Articulo.VigenciaHasta, "Fecha Vigencia hasta");
                    Fecha1 = ValidaCampoFecha(Articulo.Fecha1, "Fecha1");
                    Fecha2 = ValidaCampoFecha(Articulo.Fecha2, "Fecha2");
                    Fecha3 = ValidaCampoFecha(Articulo.Fecha3, "Fecha3");
                }

                #endregion
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(), true, true, "", body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_ArticulosJson_Result> ARTICULOJSON = new List<sp_in_API_ArticulosJson_Result>();
            List<sp_in_API_ArticulosUMedidasJson_Result> UNIDADMEDIDAJSON = new List<sp_in_API_ArticulosUMedidasJson_Result>();

            int CantidadOK = 0;
            int CantidadERROR = 0;
            string Item_Kit = "";
            string SeparadorCampo = "¬"; //alt 170
            string SeparadorLinea = "«"; //alt 174

            //Recorre articulos del json
            foreach (var Articulo in REQUEST.Articulos.ToList())
            {
                VigenciaDesde = ValidaCampoFecha(Articulo.VigenciaDesde, "Fecha Vigencia desde");
                VigenciaHasta = ValidaCampoFecha(Articulo.VigenciaHasta, "Fecha Vigencia hasta");
                Fecha1 = ValidaCampoFecha(Articulo.Fecha1, "Fecha1");
                Fecha2 = ValidaCampoFecha(Articulo.Fecha2, "Fecha2");
                Fecha3 = ValidaCampoFecha(Articulo.Fecha3, "Fecha3");

                Item_Kit = "";

                //si tiene detalle de crossdocking lo envia
                if (Articulo.Kit != null)
                {
                    foreach (var Kit2 in Articulo.Kit.ToList())
                    {
                        if (Item_Kit.Trim() == "") //Si es el primer item del Kit
                        {
                            Item_Kit = Kit2.CodigoArticulo.Trim() + SeparadorCampo +
                                       Kit2.CantidadRequerida.ToString();
                        }
                        else
                        {
                            Item_Kit = Item_Kit.Trim() + SeparadorLinea + Kit2.CodigoArticulo.Trim() + SeparadorCampo +
                                                                          Kit2.CantidadRequerida.ToString();
                        }
                    }
                }

                try
                {
                    //Insert producto -----------------------------------------------------
                    ARTICULOJSON = GP_ENT.sp_in_API_ArticulosJson(REQUEST.EmpId,
                                                                  Articulo.CodigoArticulo,
                                                                  Articulo.DescripArt,
                                                                  Articulo.DescripTecnica,
                                                                  Articulo.DescripCorta,
                                                                  Articulo.LineaProducto,
                                                                  Articulo.TipoProducto,
                                                                  Articulo.TipoArticulo,
                                                                  VigenciaDesde,
                                                                  VigenciaHasta,
                                                                  Articulo.Rotacion,
                                                                  Articulo.CodigoFabrica,
                                                                  Articulo.UsaSerie,
                                                                  Articulo.UsaLote,
                                                                  Articulo.EAN13,
                                                                  Articulo.DUN14,
                                                                  Articulo.UnidadMedidaCompra,
                                                                  Articulo.UnidadMedidaVenta,
                                                                  Articulo.CodigoProveedor,
                                                                  Articulo.Marca,
                                                                  USERNAME,
                                                                  Articulo.CodigoExt,
                                                                  Articulo.GlosaLineaProducto,
                                                                  Articulo.Dato1,
                                                                  Articulo.Dato2,
                                                                  Articulo.Dato3,
                                                                  Articulo.Valor1,
                                                                  Articulo.Valor2,
                                                                  Articulo.Valor3,
                                                                  Fecha1,
                                                                  Fecha2,
                                                                  Fecha3,
                                                                  Articulo.Origen,
                                                                  Item_Kit
                                                                  ).ToList();
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                    LogInfo(NombreProceso,
                            "ERROR: " + ex.Message.Trim(),
                            true,
                            true,
                            Articulo.CodigoArticulo,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = ex.Message.Trim();
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
                //Crea un detalle de respuesta
                API_RESPONSE_TYPE_30_DET RESPONSE_DETALLE = new API_RESPONSE_TYPE_30_DET();

                if (ARTICULOJSON.Count > 0)
                {
                    if (ARTICULOJSON[0].Resultado == "OK")
                    {
                        //Recorre Unidades de Medida del Articulo en el json
                        //Inserta o actualiza unidades de medida.
                        //La primera queda como unidad medida base del producto
                        foreach (var UnidadMedida in Articulo.UnidadMedida.ToList())
                        {
                            try
                            {
                                UNIDADMEDIDAJSON = GP_ENT.sp_in_API_ArticulosUMedidasJson(REQUEST.EmpId,
                                                                                          Articulo.CodigoArticulo,
                                                                                          USERNAME,
                                                                                          UnidadMedida.CodigoUM,
                                                                                          UnidadMedida.PesoNeto,
                                                                                          UnidadMedida.PesoBruto,
                                                                                          UnidadMedida.Ancho,
                                                                                          UnidadMedida.Alto,
                                                                                          UnidadMedida.Profundidad,
                                                                                          UnidadMedida.Volumen,
                                                                                          UnidadMedida.Factor,
                                                                                          UnidadMedida.UNBase,
                                                                                          UnidadMedida.UNTotal,
                                                                                          UnidadMedida.TiempoRecep,
                                                                                          UnidadMedida.TiempoDesp
                                                                                          ).ToList();
                            }
                            catch (Exception ex)
                            {
                                #region NOK
                                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                                LogInfo(NombreProceso,
                                        "ERROR: " + ex.Message.Trim(),
                                        true,
                                        true,
                                        Articulo.CodigoArticulo,
                                        body.ToString());

                                RESPONSE.resultado = "ERROR";
                                RESPONSE.descripcion = ex.Message.Trim();
                                RESPONSE.resultado_codigo = ERROR.ErrID;
                                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                                STATUS_CODE = HttpStatusCode.BadRequest;
                                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                #endregion
                            }

                            if (UNIDADMEDIDAJSON.Count > 0)
                            {
                                if (UNIDADMEDIDAJSON[0].Resultado != "OK")
                                {
                                    #region NOK
                                    CantidadERROR = CantidadERROR + 1;

                                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                                    LogInfo(NombreProceso, 
                                            ERROR.Mensaje.Trim() + ". " + "Error creando Unidad Medida: " + UnidadMedida.CodigoUM.Trim() +
                                                                   ", Articulo: " + Articulo.CodigoArticulo.Trim() +
                                                                   ". " + UNIDADMEDIDAJSON[0].Descripcion.Trim(), 
                                            true, 
                                            true,
                                            Articulo.CodigoArticulo, 
                                            body.ToString());

                                    RESPONSE_DETALLE.resultado = "ERROR";
                                    RESPONSE_DETALLE.descripcion = "Error creando Unidad Medida: " + UnidadMedida.CodigoUM.Trim() +
                                                                   ", Articulo: " + Articulo.CodigoArticulo.Trim() + 
                                                                   ". " + UNIDADMEDIDAJSON[0].Descripcion.Trim();
                                    RESPONSE_DETALLE.resultado_codigo = ERROR.ErrID;
                                    RESPONSE_DETALLE.resultado_descripcion = ERROR.Mensaje;
                                    RESPONSE_DETALLE.CodigoArticulo = Articulo.CodigoArticulo.Trim();

                                    RESPONSE.Lineas.Add(RESPONSE_DETALLE);

                                    break; //sale del ciclo de unidad medida

                                    //STATUS_CODE = HttpStatusCode.Unauthorized;
                                    //return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                    #endregion
                                }
                            }
                        } //FIN ciclo por unidades de medida

                        //Si recorrio todas las unidades de medida del producto sin dar errores significa que el producto fue bien integrado
                        if (UNIDADMEDIDAJSON[0].Resultado == "OK")
                        {
                            CantidadOK = CantidadOK + 1;

                            //RESPONSE_DETALLE.count = 1;
                            RESPONSE_DETALLE.resultado = "OK";
                            RESPONSE_DETALLE.descripcion = "Proceso finalizado correctamente. " + ARTICULOJSON[0].Descripcion.Trim() + ". Producto: " + Articulo.CodigoArticulo.Trim();
                            RESPONSE_DETALLE.resultado_codigo = 0;
                            RESPONSE_DETALLE.resultado_descripcion = "";
                            RESPONSE_DETALLE.CodigoArticulo = Articulo.CodigoArticulo.Trim(); 

                            RESPONSE.Lineas.Add(RESPONSE_DETALLE);
                        }
                    }

                    if (ARTICULOJSON[0].Resultado == "ERROR")
                    {
                        #region NOK
                        CantidadERROR = CantidadERROR + 1;

                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + "Error creando Producto: " + Articulo.CodigoArticulo + ", " + ARTICULOJSON[0].Descripcion,
                                true,
                                true,
                                Articulo.CodigoArticulo,
                                body.ToString());

                        RESPONSE_DETALLE.resultado = "ERROR";
                        RESPONSE_DETALLE.descripcion = "Error creando Producto: " + Articulo.CodigoArticulo + ", " + ARTICULOJSON[0].Descripcion;
                        RESPONSE_DETALLE.resultado_codigo = ERROR.ErrID;
                        RESPONSE_DETALLE.resultado_descripcion = ERROR.Mensaje;
                        RESPONSE_DETALLE.CodigoArticulo = Articulo.CodigoArticulo.Trim();

                        RESPONSE.Lineas.Add(RESPONSE_DETALLE);

                        //STATUS_CODE = HttpStatusCode.Unauthorized;
                        //return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }

            } // fin ciclo

            #endregion

            RESPONSE.count = REQUEST.Articulos.Count; //lineas procesadas
            RESPONSE.ProcesadosOK = CantidadOK;
            RESPONSE.ProcesadosERROR = CantidadERROR;

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 31 LISTAR PRODUCTO JSON (31) ----------
        #region LISTAR PRODUCTO JSON (31)
        [HttpGet]
        [HttpPost]
        [Route("PRODUCTO/LISTARJSON")]
        public IHttpActionResult recurso31([FromBody] API_REQUEST_TYPE_31 REQUEST)
        {
            API_RESPONSE_TYPE_31 RESPONSE = new API_RESPONSE_TYPE_31();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si no envian json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida usuario empresa ---
            #region VALIDA EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            try
            {
                REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo); //opcional
                REQUEST.DescripArt = (REQUEST.DescripArt == null ? "" : REQUEST.DescripArt); //opcional
                if (REQUEST.Limit <= 0) { throw new Exception("Limit debe ser mayor a 1"); } //requerido
                if (REQUEST.Rowset < 0) { throw new Exception("Rowset no puede ser un valor negativo"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_sel_API_ArticulosJson_Result> ARTICULOJSON = new List<sp_sel_API_ArticulosJson_Result>();

            API_RESPONSE_TYPE_31_ARTICULOS RESPONSE_ARTICULOS = new API_RESPONSE_TYPE_31_ARTICULOS();
            API_RESPONSE_TYPE_31_DETALLES_UM RESPONSE_DETALLES_UM = new API_RESPONSE_TYPE_31_DETALLES_UM();
            API_RESPONSE_TYPE_31_DETALLES_CAT RESPONSE_DETALLES_CAT = new API_RESPONSE_TYPE_31_DETALLES_CAT();

            //int CantidadOK = 0;
            //int CantidadERROR = 0;

            //Ejecuta consulta de articulos
            ARTICULOJSON = GP_ENT.sp_sel_API_ArticulosJson(REQUEST.EmpId,
                                                           REQUEST.CodigoArticulo,
                                                           REQUEST.DescripArt, 
                                                           REQUEST.Limit , 
                                                           REQUEST.Rowset).ToList();
            RESPONSE.Limit = REQUEST.Limit;
            RESPONSE.Rowset = REQUEST.Rowset; 

            //Si retorna resultados carga JSON de retorno
            if (ARTICULOJSON.Count > 0)
            {
                string CodigoArticulo = "";
                string UnidadMedida = "";
                string CodigoCategoria = "";

                RESPONSE.Count = long.Parse(ARTICULOJSON[0].Total.ToString()); //Total de articulos segun el filtro

                foreach (var Articulo in ARTICULOJSON)
                {
                    //Cuando cambie de articulo carga 
                    if (Articulo.CodigoArticulo != CodigoArticulo)
                    {
                        //si ya cargo la estructura de un articulo anterior en las variables lo agrega al JSON de salida
                        if (CodigoArticulo != "")
                        {
                            //Agrega nuevo articulo al JSON
                            RESPONSE.Articulos.Add(RESPONSE_ARTICULOS);
                        }

                        RESPONSE_ARTICULOS = new API_RESPONSE_TYPE_31_ARTICULOS();

                        RESPONSE_ARTICULOS.CodigoArticulo = Articulo.CodigoArticulo;
                        RESPONSE_ARTICULOS.DescripArt = Articulo.DescripArt;
                        RESPONSE_ARTICULOS.DescripTecnica = Articulo.DescripTecnica;
                        RESPONSE_ARTICULOS.DescripCorta = Articulo.DescripCorta;
                        RESPONSE_ARTICULOS.UnidadMedidaBase = Articulo.UnidadMedidaBase;
                        RESPONSE_ARTICULOS.LineaProducto = Int32.Parse(Articulo.LineaProducto.ToString());
                        RESPONSE_ARTICULOS.TipoProducto = Articulo.TipoProducto;
                        RESPONSE_ARTICULOS.VigenciaDesde = Articulo.VigenciaDesde;
                        RESPONSE_ARTICULOS.VigenciaHasta=Articulo.VigenciaHasta;
                        RESPONSE_ARTICULOS.Rotacion = Articulo.Rotacion;
                        RESPONSE_ARTICULOS.CodigoFabrica = Articulo.CodigoFabrica;
                        RESPONSE_ARTICULOS.UsaSerie = Int32.Parse(Articulo.UsaSerie.ToString());
                        RESPONSE_ARTICULOS.UsaLote = Int32.Parse(Articulo.UsaLote.ToString());
                        RESPONSE_ARTICULOS.EAN13 = Articulo.EAN13;
                        RESPONSE_ARTICULOS.DUN14 = Articulo.DUN14;
                        RESPONSE_ARTICULOS.UnidadMedidaCompra = Articulo.UnidadMedidaCompra;
                        RESPONSE_ARTICULOS.UnidadMedidaVenta = Articulo.UnidadMedidaVenta;
                        RESPONSE_ARTICULOS.CodigoProveedor = Articulo.CodigoProveedor;
                        RESPONSE_ARTICULOS.Version = Articulo.Version;
                        RESPONSE_ARTICULOS.Marca = Articulo.Marca;

                        CodigoArticulo = Articulo.CodigoArticulo;
                        UnidadMedida = "";
                        CodigoCategoria = "";                        
                    }

                    if ((Articulo.UnidadMedida != UnidadMedida) && (Articulo.UnidadMedida != ""))
                    {
                        RESPONSE_DETALLES_UM = new API_RESPONSE_TYPE_31_DETALLES_UM();

                        RESPONSE_DETALLES_UM.CodigoUM = Articulo.UnidadMedida;
                        RESPONSE_DETALLES_UM.Factor = decimal.Parse(Articulo.Factor.ToString());

                        //Agrega nueva UM al JSON del articulo
                        RESPONSE_ARTICULOS.UnidadMedida.Add(RESPONSE_DETALLES_UM);

                        UnidadMedida = Articulo.UnidadMedida;
                    }

                    if ((Articulo.CodigoCategoria != CodigoCategoria) && (Articulo.CodigoCategoria != ""))
                    {
                        bool Existe = RESPONSE_ARTICULOS.Categoria.ToList().Any(x => x.Categoria == Articulo.CodigoCategoria);

                        //Si la categoria no existe la agrega ----
                        if (Existe == false)
                        {
                            RESPONSE_DETALLES_CAT = new API_RESPONSE_TYPE_31_DETALLES_CAT();

                            RESPONSE_DETALLES_CAT.Categoria = Articulo.CodigoCategoria;
                            RESPONSE_DETALLES_CAT.SubCategoria = Articulo.DescripcionCat;

                            //Agrega nueva Categoria al JSON del articulo
                            RESPONSE_ARTICULOS.Categoria.Add(RESPONSE_DETALLES_CAT);

                            CodigoCategoria = Articulo.CodigoCategoria;
                        }                        
                    }
                }

                //Agrega ultimo articulo que estaba procesando al JSON
                RESPONSE.Articulos.Add(RESPONSE_ARTICULOS);
            }
            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 32 LISTAR PRODUCTO JSON TRANSFARMA (32) ----------
        #region LISTAR PRODUCTO JSON TRANSFARMA (32)
        [HttpGet]
        [HttpPost]
        [Route("PRODUCTO/LISTARTF")]
        public IHttpActionResult recurso32([FromBody] API_REQUEST_TYPE_31 REQUEST)
        {
            API_RESPONSE_TYPE_32 RESPONSE = new API_RESPONSE_TYPE_32();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y secret, retorna usuario -----------
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida usuario empresa ---
            #region VALIDA USUARIO EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Error al validar la relación Usuario - empresa";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            try
            {
                REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo); //opcional
                REQUEST.DescripArt = (REQUEST.DescripArt == null ? "" : REQUEST.DescripArt); //opcional
                if (REQUEST.Limit <= 0) { throw new Exception("Limit debe ser mayor a 1"); } //requerido
                if (REQUEST.Rowset < 0) { throw new Exception("Rowset no puede ser un valor negativo"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_sel_API_ArticulosJsonTransfarma_Result> ARTICULOJSON = new List<sp_sel_API_ArticulosJsonTransfarma_Result>();

            API_RESPONSE_TYPE_32_ARTICULOS RESPONSE_ARTICULOS = new API_RESPONSE_TYPE_32_ARTICULOS();

            //int CantidadOK = 0;
            //int CantidadERROR = 0;

            try
            {
                //Ejecuta consulta de articulos
                ARTICULOJSON = GP_ENT.sp_sel_API_ArticulosJsonTransfarma(REQUEST.EmpId,
                                                                         REQUEST.CodigoArticulo,
                                                                         REQUEST.DescripArt,
                                                                         REQUEST.Limit,
                                                                         REQUEST.Rowset).ToList();
                //Si retorna resultados carga JSON de retorno
                if (ARTICULOJSON.Count > 0)
                {
                    string CodigoArticulo = "";
                    //string UnidadMedida = "";
                    //string Categoria = "";
                    //string SubCategoria = "";

                    RESPONSE.Limit = long.Parse(ARTICULOJSON[0].Limit.ToString());
                    RESPONSE.Rowset = long.Parse(ARTICULOJSON[0].Rowset.ToString());
                    RESPONSE.Count = long.Parse(ARTICULOJSON[0].Total.ToString()); //Total de articulos segun el filtro

                    foreach (var Articulo in ARTICULOJSON)
                    {
                        //Cuando cambie de articulo carga 
                        if (Articulo.CodigoArticulo != CodigoArticulo)
                        {
                            //si ya cargo la estructura de un articulo anterior en las variables lo agrega al JSON de salida
                            if (CodigoArticulo != "")
                            {
                                //Agrega nuevo articulo al JSON
                                RESPONSE.Articulos.Add(RESPONSE_ARTICULOS);
                            }
                            
                            //Inicializa variable de articulo  -------------
                            RESPONSE_ARTICULOS = new API_RESPONSE_TYPE_32_ARTICULOS();

                            RESPONSE_ARTICULOS.Cliente = Articulo.Cliente;
                            RESPONSE_ARTICULOS.CodigoArticulo = Articulo.CodigoArticulo;
                            RESPONSE_ARTICULOS.DescripArt = Articulo.DescripArt;
                            RESPONSE_ARTICULOS.UnidadPresentacion = Articulo.UnidadMedidaVenta;
                            RESPONSE_ARTICULOS.ClasificacionTemperatura = Articulo.ClasifTemperatura;
                            RESPONSE_ARTICULOS.ClasificacionEmbarque = Articulo.ClasifEmbarque;
                            RESPONSE_ARTICULOS.HojaSeguridad = Articulo.HojaSeguridad;
                            RESPONSE_ARTICULOS.Largo = decimal.Parse(Articulo.Profundidad.ToString());
                            RESPONSE_ARTICULOS.Ancho = decimal.Parse(Articulo.Ancho.ToString());
                            RESPONSE_ARTICULOS.Alto = decimal.Parse(Articulo.Alto.ToString());
                            RESPONSE_ARTICULOS.UnidadMedida = Articulo.UnidadMedidaBase;
                            RESPONSE_ARTICULOS.Paletizado = Articulo.Paletizado;
                            RESPONSE_ARTICULOS.CodigoCliente = Articulo.EmpId.ToString();
                            RESPONSE_ARTICULOS.GrupoMaterial = Articulo.GrupoMaterial;
                            RESPONSE_ARTICULOS.EAN = Articulo.EAN13;
                            RESPONSE_ARTICULOS.CategoriaEAN = Articulo.CategoriaEAN;
                            RESPONSE_ARTICULOS.CostoUnitario = decimal.Parse(Articulo.CostoUnitario.ToString());

                            if (Articulo.ArchivoImagen.Trim() == "")
                            {
                                RESPONSE_ARTICULOS.ArchivoImagen = "";
                            }
                            else
                            {
                                if (ConfigurationManager.AppSettings["ImgCliente"].Trim() != "")
                                {
                                    string RutaArchivo = "";
                                    RutaArchivo = ConfigurationManager.AppSettings["ImgCliente"].Trim() +
                                                  Articulo.ArchivoImagen.Replace(@"..", "");

                                    byte[] imageArray = System.IO.File.ReadAllBytes(RutaArchivo.Replace(@"/", @"\").ToString());
                                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);

                                    RESPONSE_ARTICULOS.ArchivoImagen = base64ImageRepresentation;
                                }
                            }

                            RESPONSE_ARTICULOS.PesoNeto = decimal.Parse(Articulo.PesoNeto.ToString());
                            RESPONSE_ARTICULOS.UnidadPeso = Articulo.UnidadPeso;
                            RESPONSE_ARTICULOS.Volumen = decimal.Parse(Articulo.Volumen.ToString());
                            RESPONSE_ARTICULOS.UnidadVolumen = Articulo.UnidadVolumen;
                            RESPONSE_ARTICULOS.Cuarentena = Articulo.Cuarentena;
                            RESPONSE_ARTICULOS.TipoAlmacenamiento = Articulo.TipoAlmacenamiento;
                            RESPONSE_ARTICULOS.ClasificacionAlmacenaje = Articulo.ClasifAlmacenaje;
                            RESPONSE_ARTICULOS.Reposicion = Articulo.Reposicion;
                            RESPONSE_ARTICULOS.Temperatura = Articulo.Temperatura;
                            RESPONSE_ARTICULOS.InventarioCiclico = Articulo.InventarioCiclico;
                            RESPONSE_ARTICULOS.Apilamiento = Articulo.Apilamiento;
                            RESPONSE_ARTICULOS.ClasificacionABC = Articulo.Rotacion;
                            RESPONSE_ARTICULOS.Lote = Articulo.Lote;
                            RESPONSE_ARTICULOS.RestriccionViaAerea = Articulo.RestriccionViaAerea;
                            RESPONSE_ARTICULOS.EmbalajeCadenaFrio = Articulo.EmbalajeCadenaFrio;

                            RESPONSE_ARTICULOS.ArchivoPDF = "";

                            if (Articulo.ArchivoPDF.Trim() != "")
                            {
                                if (ConfigurationManager.AppSettings["RutaAdjuntos"].Trim() != "")
                                {
                                    RESPONSE_ARTICULOS.ArchivoPDF = ConfigurationManager.AppSettings["RutaAdjuntos"].Trim() + "/" +
                                                                    Articulo.ArchivoPDF.Trim();
                                }
                            }

                            CodigoArticulo = Articulo.CodigoArticulo;
                            //UnidadMedida = "";
                            //Categoria = "";
                            //SubCategoria = "";
                        }

                        //if ((Articulo.UnidadMedida != UnidadMedida) && (Articulo.UnidadMedida != ""))
                        //{
                        //    RESPONSE_DETALLES_UM = new API_RESPONSE_TYPE_32_DETALLES_UM();

                        //    RESPONSE_DETALLES_UM.CodigoUM = Articulo.UnidadMedida;
                        //    RESPONSE_DETALLES_UM.Factor = decimal.Parse(Articulo.Factor.ToString());

                        //    //Agrega nueva UM al JSON del articulo
                        //    RESPONSE_ARTICULOS.UnidadesMedida.Add(RESPONSE_DETALLES_UM);

                        //    UnidadMedida = Articulo.UnidadMedida;
                        //}

                        //if ((Articulo.DescripcionCat != Categoria) && (Articulo.DescripcionCat != "") && (Articulo.DescripcionCatDet != SubCategoria))
                        //{
                        //    bool Existe = RESPONSE_ARTICULOS.Categorias.ToList().Any(x => x.Categoria.Trim() == Articulo.DescripcionCat.Trim() &&
                        //                                                                  x.SubCategoria.Trim() == Articulo.DescripcionCatDet.Trim());

                        //    //Si la categoria/subcategoria no existe en la lista la agrega ----
                        //    if (Existe == false)
                        //    {
                        //        RESPONSE_DETALLES_CAT = new API_RESPONSE_TYPE_32_DETALLES_CAT();

                        //        RESPONSE_DETALLES_CAT.Categoria = Articulo.DescripcionCat;
                        //        RESPONSE_DETALLES_CAT.SubCategoria = Articulo.DescripcionCatDet;

                        //        //Agrega nueva Categoria al JSON del articulo
                        //        RESPONSE_ARTICULOS.Categorias.Add(RESPONSE_DETALLES_CAT);

                        //        Categoria = Articulo.DescripcionCat;
                        //        SubCategoria = Articulo.DescripcionCatDet;
                        //    }
                        //}
                    }

                    //Agrega ultimo articulo que estaba procesando al JSON
                    RESPONSE.Articulos.Add(RESPONSE_ARTICULOS);
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.InternalServerError;

                //Limpia lista de articulos que estaba creando para no retornar un listado incompleto ---
                RESPONSE.Articulos = new List<API_RESPONSE_TYPE_32_ARTICULOS>();

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 33 LISTAR MOVIMIENTO MATERIAL JSON TRANSFARMA (33) ----------
        #region LISTAR MOVIMIENTO MATERIAL JSON TRANSFARMA (33)
        [HttpGet]
        [HttpPost]
        [Route("PRODUCTO/MOVIMIENTOSTF")]
        public IHttpActionResult Recurso33([FromBody] API_REQUEST_TYPE_33 REQUEST)
        {
            API_RESPONSE_TYPE_33 RESPONSE = new API_RESPONSE_TYPE_33();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y secret, retorna usuario -----------
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida usuario empresa ---
            #region VALIDA USUARIO EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Error al validar la relación Usuario - empresa";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            try
            {
                if (REQUEST.CodigoArticulo == null || REQUEST.CodigoArticulo == "") { throw new Exception("Debe enviar Codigo Artículo"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_sel_API_Consulta_Movto_Transfarma_Result> MOVIMIENTOS = new List<sp_sel_API_Consulta_Movto_Transfarma_Result>();

            API_RESPONSE_TYPE_33_MOVIMIENTOS RESPONSE_MOVIMIENTOS = new API_RESPONSE_TYPE_33_MOVIMIENTOS();

            //int CantidadOK = 0;
            //int CantidadERROR = 0;

            try
            {
                //Ejecuta consulta de movimientos
                MOVIMIENTOS = GP_ENT.sp_sel_API_Consulta_Movto_Transfarma(REQUEST.EmpId,
                                                                          REQUEST.CodigoArticulo).ToList();

                //Si retorna resultados carga JSON de retorno
                if (MOVIMIENTOS.Count > 0)
                {
                    RESPONSE.Count = MOVIMIENTOS.Count;

                    foreach (var Movimiento in MOVIMIENTOS)
                    {
                        //Inicializa variable de Movimiento -------------
                        RESPONSE_MOVIMIENTOS = new API_RESPONSE_TYPE_33_MOVIMIENTOS();

                        RESPONSE_MOVIMIENTOS.Almacen = int.Parse(Movimiento.Almacen.ToString());
                        RESPONSE_MOVIMIENTOS.CodigoMaterial=Movimiento.CodigoMaterial;
                        RESPONSE_MOVIMIENTOS.Lote = Movimiento.Lote;
                        RESPONSE_MOVIMIENTOS.Fecha = DateTime.Parse(Movimiento.FechaHora.ToString());
                        RESPONSE_MOVIMIENTOS.NumeroDocumento = int.Parse(Movimiento.NumeroDocumento);
                        RESPONSE_MOVIMIENTOS.TipoMovimiento = Movimiento.TipoMovimiento;
                        RESPONSE_MOVIMIENTOS.Cantidad = decimal.Parse(Movimiento.Cantidad.ToString());
                        RESPONSE_MOVIMIENTOS.UnidadMedida = Movimiento.UnidadMedida;
                        RESPONSE_MOVIMIENTOS.ClasificacionAlmacenaje = Movimiento.ClasificacionAlmacenaje;
                        RESPONSE_MOVIMIENTOS.Zona = Movimiento.Zona;
                        RESPONSE_MOVIMIENTOS.Posicion = Movimiento.Posicion;
                        RESPONSE_MOVIMIENTOS.Username = Movimiento.Usuario;

                        RESPONSE.Movimientos.Add(RESPONSE_MOVIMIENTOS);
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Count=0;
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.InternalServerError;

                //Limpia lista de articulos que estaba creando para no retornar un listado incompleto ---
                RESPONSE.Movimientos = new List<API_RESPONSE_TYPE_33_MOVIMIENTOS>();

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 34 LISTAR SALDOS MATERIAL JSON TRANSFARMA (34) ----------
        #region LISTAR STOCK MATERIAL JSON TRANSFARMA (34)
        [HttpGet]
        [HttpPost]
        [Route("PRODUCTO/STOCKTF")]
        public IHttpActionResult Recurso34([FromBody] API_REQUEST_TYPE_34 REQUEST)
        {
            API_RESPONSE_TYPE_34 RESPONSE = new API_RESPONSE_TYPE_34();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y secret, retorna usuario -----------
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida usuario empresa ---
            #region VALIDA USUARIO EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Error al validar la relación Usuario - empresa";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            try
            {
                REQUEST.CodigoArticulo = (REQUEST.CodigoArticulo == null ? "" : REQUEST.CodigoArticulo); //opcional
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_sel_API_Stock_Material_Transfarma_Result> STOCKS = new List<sp_sel_API_Stock_Material_Transfarma_Result>();

            API_RESPONSE_TYPE_34_STOCK RESPONSE_STOCK = new API_RESPONSE_TYPE_34_STOCK();

            //int CantidadOK = 0;
            //int CantidadERROR = 0;

            try
            {
                //Ejecuta consulta de movimientos
                STOCKS = GP_ENT.sp_sel_API_Stock_Material_Transfarma(REQUEST.EmpId,
                                                                     REQUEST.CodigoArticulo).ToList();

                //Si retorna resultados carga JSON de retorno -----
                if (STOCKS.Count > 0)
                {
                    RESPONSE.Count = STOCKS.Count;

                    foreach (var Stock in STOCKS)
                    {
                        //Inicializa variable de Movimiento -------------
                        RESPONSE_STOCK = new API_RESPONSE_TYPE_34_STOCK();

                        RESPONSE_STOCK.Almacen = Stock.CodigoBodega.ToString();
                        RESPONSE_STOCK.CodigoMaterial = Stock.CodigoArticulo;
                        RESPONSE_STOCK.Lote = Stock.Lote;
                        RESPONSE_STOCK.Cantidad = int.Parse(Stock.Stock.ToString());
                        RESPONSE_STOCK.UnidadMedida = Stock.UnidadMedida;
                        RESPONSE_STOCK.Clasificacion = Stock.Clasificacion;
                        RESPONSE_STOCK.Zona = Stock.Zona;

                        RESPONSE.StockMaterial.Add(RESPONSE_STOCK);
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Count = 0;
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.InternalServerError;

                //Limpia lista de articulos que estaba creando para no retornar un listado incompleto ---
                RESPONSE.StockMaterial = new List<API_RESPONSE_TYPE_34_STOCK>();

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //Asocia documento generado a la revision (35) ----
        #region ASIGNACION DE DOCUMENTO (35)
        [HttpPost]
        [Route("SOLDESP/ASIGNADOCTO")]
        public IHttpActionResult recurso35(API_REQUEST_TYPE_35 REQUEST)
        {
            API_RESPONSE_TYPE_24_DET RESPONSE = new API_RESPONSE_TYPE_24_DET();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //valida token y secret, retorna usuario -----------
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido ---
            #region VALIDA JSON RECIBIDO NOK

            //valida tipos de datos recibidos en el json coincidan con la estructura que se espera---
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida usuario empresa ---
            #region VALIDA USUARIO EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Error al validar la relación Usuario - empresa";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = "";

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea, siempre vendra un valor, independiente se envie o no el campo en la estructura Json
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, significa que por defecto es opcional

            int TieneLista = 0;

            try
            {
                if (REQUEST.Usuario == null || REQUEST.Usuario == "" || REQUEST.Usuario == "0") { throw new Exception("Debe informar Usuario"); } //requerido
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "" || REQUEST.TipoReferencia == "0") { throw new Exception("Debe informar Tipo Referencia origen"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "" || REQUEST.NumeroReferencia == "0") { throw new Exception("Debe informar Numero Referencia origen"); } //requerido
                if (REQUEST.ColaPickId <= 0) { throw new Exception("Debe Id Picking"); } //requerido

                //Valida que se informe Documento generado. Comprueba si se informó un solo documento o multiples documentos en una lista.
                //Valida si se envió una lista de documentos para asignar
                if (REQUEST.Documentos != null)
                {
                    foreach (var DocumentosAsignar in REQUEST.Documentos)
                    {
                        TieneLista = 1;
                        if (DocumentosAsignar.TipoDocumento <= 0) { throw new Exception("Debe informar Tipo Documento asignar"); } //requerido
                        if (DocumentosAsignar.NumeroDocto == null || DocumentosAsignar.NumeroDocto == "") { throw new Exception("Debe informar Numero Documento asignar"); } //requerido
                        //DocumentosAsignar.Monto //opcional numerico, toma cero
                    }

                    if (TieneLista == 0) //Si no se envía listado revisa si se envió un solo Documento 
                    {
                        if (REQUEST.TipoDocumento <= 0) { throw new Exception("Debe informar Tipo Documento asignar"); } //requerido
                        if (REQUEST.NumeroDocto == null || REQUEST.NumeroDocto == "" || REQUEST.NumeroDocto == "0") { throw new Exception("Debe informar Numero Documento asignar"); } //requerido
                                                                                                                                                                                       //REQUEST.Monto //opcional numerico, toma cero
                    }
                }
                else //Si no se envía listado revisa si se envió un solo Documento 
                {
                    if (REQUEST.TipoDocumento <= 0) { throw new Exception("Debe informar Tipo Documento asignar"); } //requerido
                    if (REQUEST.NumeroDocto == null || REQUEST.NumeroDocto == "" || REQUEST.NumeroDocto == "0") { throw new Exception("Debe informar Numero Documento asignar"); } //requerido
                    //REQUEST.Monto //opcional numerico, toma cero
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            try
            {
                string Listado = "";

                //si tiene lista de documentos asignados

                //if (REQUEST.Documentos != null)
                if (TieneLista == 1) //Si tiene listado
                {
                    foreach (var DocumentosAsignar2 in REQUEST.Documentos.ToList())
                    {
                        if (Listado.Trim() == "") //Si es el primer item de la lista de documentos
                        {
                            Listado = DocumentosAsignar2.TipoDocumento.ToString().Trim() + ";" +
                                      DocumentosAsignar2.NumeroDocto.Trim() + ";" +
                                      DocumentosAsignar2.Monto.ToString().Trim();
                        }
                        else
                        {
                            Listado = Listado.Trim() + "#" + DocumentosAsignar2.TipoDocumento.ToString().Trim() + ";" +
                                                             DocumentosAsignar2.NumeroDocto.Trim() + ";" +
                                                             DocumentosAsignar2.Monto.ToString().Trim();
                        }
                    }
                }
                else //Viene un solo documento a asignar
                {
                    Listado = REQUEST.TipoDocumento.ToString().Trim() + ";" +
                              REQUEST.NumeroDocto.Trim() + ";" +
                              REQUEST.Monto.ToString().Trim();
                }

                List<sp_upd_API_AsignaDocumento_Result> RESULTADO = GP_ENT.sp_upd_API_AsignaDocumento(REQUEST.EmpId,
                                                                                                      REQUEST.Usuario,
                                                                                                      REQUEST.TipoReferencia,
                                                                                                      REQUEST.NumeroReferencia,
                                                                                                      REQUEST.ColaPickId,
                                                                                                      Listado.Trim()).ToList();
                RESPONSE.resultado = RESULTADO.ElementAt(0).Resultado;
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = RESULTADO.ElementAt(0).Resultado_Codigo;
                RESPONSE.resultado_descripcion = RESULTADO.ElementAt(0).Resultado_Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "Error";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.InternalServerError;

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        //Especial TOYOTA
        //Asocia la OC de SAP con la SDR generada con datos de TOPAS a traves de datos de referencia (36) ----------
        #region ACTUALIZA REFERENCIA SDR (36)
        [HttpPost]
        [Route("SOLRECEP/ACTUALIZAREFSDR")]
        public IHttpActionResult recurso36([FromBody] API_REQUEST_TYPE_36 REQUEST)
        {
            API_RESPONSE_TYPE_36 RESPONSE = new API_RESPONSE_TYPE_36();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //Valida Token y Secret, retorna Usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Guarda JSON recibido como parametro ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo("SOLRECEP/ACTUALIZAREFSDR", 
                    "Actualiza referencia SDR, JSON: " + body.ToString(), 
                    false, 
                    true, 
                    REQUEST.NumeroReferencia, 
                    body.ToString());

            //valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de validaciones iniciales realiza proceso --- 
            #region PROCESAMIENTO 

            DateTime FechaProceso = new DateTime();
            DateTime FechaReferencia = new DateTime(); 
            DateTime FechaReferencia2 = new DateTime(); 

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES

            //valida datos cabecera json
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan con un cero
            // - un campo string se puede dejar opcional enviando un valor por defecto 

            try
            {
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                //REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional
                if (REQUEST.Proveedor == null || REQUEST.Proveedor == "") { throw new Exception("Debe informar Proveedor"); } //requerido
                if (REQUEST.RazonSocial == null || REQUEST.RazonSocial == "") { throw new Exception("Debe informar Razon Social"); } //requerido

                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido

                if (REQUEST.TipoReferencia2 == null || REQUEST.TipoReferencia2 == "") { throw new Exception("Debe informar Tipo Referencia2"); } //requerido
                if (REQUEST.NumeroReferencia2 == null || REQUEST.NumeroReferencia2 == "") { throw new Exception("Debe informar Número Referencia2"); } //requerido
                if (REQUEST.FechaReferencia2 == null || REQUEST.FechaReferencia2 == "") { throw new Exception("Debe informar Fecha Referencia2"); } //requerido

                if (REQUEST.Glosa == null || REQUEST.Glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido
                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadCompra == null || item.UnidadCompra == "") { throw new Exception("Debe informar Unidad Compra"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    if (item.ItemReferencia <= 0) { throw new Exception("Debe informar Item Referencia"); } //requerido
                    if (item.ItemReferencia2 <= 0) { throw new Exception("Debe informar Item Referencia2"); } //requerido
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA FORMATOS FECHAS
            try
            {
                #region VALIDA CAMPOS CABECERA JSON 
                try
                {
                    if (REQUEST.FechaProceso.Equals(""))
                    {
                        FechaProceso = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        FechaProceso = DateTime.ParseExact(REQUEST.FechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FechaProceso debe ser formato dd-MM-yyyy (ejemplo: 06-11-2020)"); }

                try
                {
                    if (REQUEST.FechaReferencia.Equals(""))
                    {
                        FechaReferencia = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        FechaReferencia = DateTime.ParseExact(REQUEST.FechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FechaReferencia debe ser formato dd-MM-yyyy (ejemplo: 06-11-2020)"); }

                try
                {
                    if (REQUEST.FechaReferencia2.Equals(""))
                    {
                        FechaReferencia2 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        FechaReferencia2 = DateTime.ParseExact(REQUEST.FechaReferencia2, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FechaReferencia2 debe ser formato dd-MM-yyyy (ejemplo: 06-11-2020)"); }

                #endregion
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_upd_API_ReferenciaSDR_Result> CAB = new List<sp_upd_API_ReferenciaSDR_Result>();
            List<sp_upd_API_ReferenciaSDRDet_Result> DET = new List<sp_upd_API_ReferenciaSDRDet_Result>();
            //List<sp_proc_API_SolRecepcionValidaJson_Result> SOLRECEPVALIDA = new List<sp_proc_API_SolRecepcionValidaJson_Result>();

            //Inserta cabecera solicitud recepcion 
            CAB = GP_ENT.sp_upd_API_ReferenciaSDR(REQUEST.Empid,
                                                  USERNAME,
                                                  FechaProceso,
                                                  REQUEST.TipoSolicitud,
                                                  REQUEST.CodigoBodega,
                                                  REQUEST.Comprador,
                                                  REQUEST.Proveedor,
                                                  REQUEST.RazonSocial,
                                                  REQUEST.TipoReferencia,
                                                  REQUEST.NumeroReferencia,
                                                  FechaReferencia,
                                                  REQUEST.TipoReferencia2,
                                                  REQUEST.NumeroReferencia2,
                                                  FechaReferencia2,
                                                  REQUEST.Glosa,
                                                  REQUEST.Dato1,
                                                  REQUEST.Dato2,
                                                  REQUEST.Dato3
                                                  ).ToList();
            if (CAB.Count > 0)
            {
                if (CAB[0].Count > 0) //pregunta por el campo Count, si es mayor a cero procesó OK la cabecera
                {
                    RESPONSE.solRecepId = (int)CAB[0].SolRecepId;
                    RESPONSE.descripcion = CAB[0].Descripcion;

                    //recorre items json
                    foreach (var item2 in REQUEST.Items.ToList())
                    {
                        DET = GP_ENT.sp_upd_API_ReferenciaSDRDet(RESPONSE.solRecepId,
                                                                item2.CodigoArticulo,
                                                                item2.UnidadCompra,
                                                                item2.Cantidad,
                                                                item2.ItemReferencia,
                                                                item2.ItemReferencia2
                                                                ).ToList();
                        if (DET.Count > 0)
                        {
                            if (DET[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                            {
                                #region NOK
                                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO
                                RESPONSE.resultado = "ERROR";
                                RESPONSE.descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + DET[0].Descripcion;
                                RESPONSE.resultado_codigo = ERROR.ErrID;
                                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                                RESPONSE.solRecepId = 0;
                                STATUS_CODE = HttpStatusCode.BadRequest;
                                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                #endregion
                            }
                        }
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = CAB[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion        

        //Metodo para recibir resultado de Webhook de Tracking Enviame
        #region RECIBE WEBHOOK - TRACKING ENVIAME (37)
        [HttpPost]
        [HttpGet]
        [Route("SOLDESP/WH_TRACKING_ENVIAME")]
        public IHttpActionResult recurso37(object data)
        {
            API_RESPONSE_TYPE_17 RESPONSE = new API_RESPONSE_TYPE_17();
            ERROR = new API_RESPONSE_ERRORS();

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            try
            {
                LogInfo("SOLDESP/WH_TRACKING_ENVIAME", 
                        "Resultado recibido desde Enviame: " + data.ToString(), 
                        false, 
                        true, 
                        "", 
                        data.ToString());

                List<sp_in_API_Webhook_Tracking_Enviame_Result> WBSL = GP_ENT.sp_in_API_Webhook_Tracking_Enviame(data.ToString()).ToList();

                RESPONSE.Resultado = WBSL.ElementAt(0).Resultado;
                RESPONSE.Descripcion = WBSL.ElementAt(0).Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        //Crea Solicitud ASN (38)
        #region CREA SOLICITUD ASN (38)
        [HttpPost]
        [Route("SOLASN/CREAR")]
        public IHttpActionResult recurso38([FromBody] API_REQUEST_TYPE_38 REQUEST)
        {
            API_RESPONSE_TYPE_38 RESPONSE = new API_RESPONSE_TYPE_38();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            //Valida Token y Secret, retorna Usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida, que no venga vacío y que tenga detalle de items ---
            #region VALIDA JSON RECIBIDO NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json viene vacio";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida que el json tenga items ---
            if (REQUEST.Items == null) 
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json no contiene detalle de items";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida que el json tenga items ---
            if (REQUEST.Items.Count == 0) 
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = "El json no contiene detalle de items";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Guarda JSON recibido como parametro ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo("SOLASN/CREAR", 
                    "Creación Sol ASN, JSON: " + body.ToString(), 
                    false, 
                    true, 
                    "", 
                    body.ToString());

            //Valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Luego de validaciones iniciales realiza proceso --- 
            #region PROCESAMIENTO 

            DateTime FechaCreacion = DateTime.Now; 
            DateTime CustomFecha1 = new DateTime();
            DateTime CustomFecha2 = new DateTime();
            DateTime CustomFecha3 = new DateTime();
            DateTime FechaDocto = new DateTime();
            DateTime DetCustomDateField_1 = new DateTime();
            DateTime DetCustomDateField_2 = new DateTime();
            DateTime DetCustomDateField_3 = new DateTime();

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES

            ////valida datos cabecera json
            //// - campos string       : si no vienen en el JSON quedan null
            //// - campos numericos    : si no vienen en el JSON quedan con un cero
            //// - un campo string se puede dejar opcional enviando un valor por defecto 

            try
            {
                //if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                //if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                ////REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                //REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional
                //if (REQUEST.Proveedor == null || REQUEST.Proveedor == "") { throw new Exception("Debe informar Proveedor"); } //requerido
                //if (REQUEST.RazonSocial == null || REQUEST.RazonSocial == "") { throw new Exception("Debe informar Razon Social"); } //requerido
                //if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                //if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                //if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                //if (REQUEST.Glosa == null || REQUEST.Glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido
                //REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                //REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                //REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional

                //decimal Valor1 //opcional numerico, toma cero
                //decimal Valor2 //opcional numerico, toma cero
                //decimal Valor3 //opcional numerico, toma cero

                //REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional 
                //REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional 
                //REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                //int PrimerItem = 1;

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    //if (item.UnidadCompra == null || item.UnidadCompra == "") { throw new Exception("Debe informar Unidad Compra"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    //item.NumeroSerie = (item.NumeroSerie == null ? "" : item.NumeroSerie); //opcional
                    //item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    //if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido

                    //item.PorcQA //opcional numerico, toma cero
                    //item.SolDespIdCD //opcional numerico, toma cero
                    //item.CantidadCD //opcional numerico, toma cero

                    //item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    //item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    //item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional

                    //decimal Valor1 //opcional numerico, toma cero
                    //decimal Valor2 //opcional numerico, toma cero
                    //decimal Valor3 //opcional numerico, toma cero

                    //item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional                    
                    //item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    //item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA FECHAS ------
            try
            {
                #region VALIDA FECHAS CABECERA JSON 
                try
                {
                    if (REQUEST.CustomDateField_1.Equals(""))
                        { CustomFecha1 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                    else
                        { CustomFecha1 = DateTime.ParseExact(REQUEST.CustomDateField_1, "dd-MM-yyyy", null); }

                    if (REQUEST.CustomDateField_2.Equals(""))
                        { CustomFecha2 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                    else
                        { CustomFecha2 = DateTime.ParseExact(REQUEST.CustomDateField_2, "dd-MM-yyyy", null); }

                    if (REQUEST.CustomDateField_3.Equals(""))
                        { CustomFecha3 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                    else
                        { CustomFecha3 = DateTime.ParseExact(REQUEST.CustomDateField_3, "dd-MM-yyyy", null); }
                }
                catch 
                { 
                    throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); 
                }
                #endregion

                #region VALIDA FECHAS ITEMS DETALLE JSON
                foreach (var item in REQUEST.Items.ToList())
                {
                    try
                    {
                        if (item.FechaDocumento.Equals(""))
                        { FechaDocto = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                        else
                        { FechaDocto = DateTime.ParseExact(item.FechaDocumento, "dd-MM-yyyy", null); }

                        if (item.DetCustomDateField_1.Equals(""))
                        { DetCustomDateField_1 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                        else
                        { DetCustomDateField_1 = DateTime.ParseExact(item.DetCustomDateField_1, "dd-MM-yyyy", null); }

                        if (item.DetCustomDateField_2.Equals(""))
                        { DetCustomDateField_2 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                        else
                        { DetCustomDateField_2 = DateTime.ParseExact(item.DetCustomDateField_2, "dd-MM-yyyy", null); }

                        if (item.DetCustomDateField_3.Equals(""))
                        { DetCustomDateField_3 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null); }
                        else
                        { DetCustomDateField_3 = DateTime.ParseExact(item.DetCustomDateField_3, "dd-MM-yyyy", null); }
                    }
                    catch { throw new Exception("ERROR - FECHAVECTOLOTE DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                }

                #endregion
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //FechaCreacion = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);

            if (REQUEST.CustomDateField_1.Equals(""))
                {CustomFecha1 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
            else
                {CustomFecha1 = DateTime.ParseExact(REQUEST.CustomDateField_1, "dd-MM-yyyy", null);}

            if (REQUEST.CustomDateField_2.Equals(""))
                {CustomFecha2 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
            else
                {CustomFecha2 = DateTime.ParseExact(REQUEST.CustomDateField_2, "dd-MM-yyyy", null);}

            if (REQUEST.CustomDateField_3.Equals(""))
                {CustomFecha3 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
            else
                {CustomFecha3 = DateTime.ParseExact(REQUEST.CustomDateField_3, "dd-MM-yyyy", null);}

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_SolicitudASN_Result> SOLICITUD_ASN = new List<sp_in_API_SolicitudASN_Result>();
            //List<sp_in_API_SolRecepcionDetJson_Result> SOLRECEPDET = new List<sp_in_API_SolRecepcionDetJson_Result>();
            //List<sp_proc_API_SolRecepcionValidaJson_Result> SOLRECEPVALIDA = new List<sp_proc_API_SolRecepcionValidaJson_Result>();

            int Linea = 1;
            string Archivo = "";
            decimal ReciboAsnId = 0;

            //recorre items ASN
            foreach (var item2 in REQUEST.Items.ToList())
            {
                if (item2.FechaDocumento.Equals(""))
                    {FechaDocto = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
                else
                    {FechaDocto = DateTime.ParseExact(item2.FechaDocumento, "dd-MM-yyyy", null);}

                if (item2.DetCustomDateField_1.Equals(""))
                    {DetCustomDateField_1 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
                else
                    {DetCustomDateField_1 = DateTime.ParseExact(item2.DetCustomDateField_1, "dd-MM-yyyy", null);}

                if (item2.DetCustomDateField_2.Equals(""))
                    {DetCustomDateField_2 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
                else
                    {DetCustomDateField_2 = DateTime.ParseExact(item2.DetCustomDateField_2, "dd-MM-yyyy", null);}

                if (item2.DetCustomDateField_3.Equals(""))
                    {DetCustomDateField_3 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);}
                else
                    {DetCustomDateField_3 = DateTime.ParseExact(item2.DetCustomDateField_3, "dd-MM-yyyy", null);}

                //if (item2.Fecha3.Equals(""))
                //{
                //    fecha3 = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                //}
                //else
                //{
                //    fecha3 = DateTime.ParseExact(item2.Fecha3, "dd-MM-yyyy", null);
                //}

                //Inserta Solicitud ASN, cabecera y detalle
                SOLICITUD_ASN = GP_ENT.sp_in_API_SolicitudASN(REQUEST.Empid,
                                                              REQUEST.Usuario,
                                                              FechaCreacion,
                                                              REQUEST.Origen,
                                                              REQUEST.RutProveedor,
                                                              REQUEST.ASN_CodigoBarraExt,
                                                              REQUEST.TipoReferencia,
                                                              REQUEST.NumeroReferencia,
                                                              REQUEST.Patente,
                                                              REQUEST.RutChofer,
                                                              REQUEST.NombreChofer,
                                                              1,
                                                              REQUEST.CustomDecimalField_1,
                                                              REQUEST.CustomDecimalField_2,
                                                              REQUEST.CustomDecimalField_3,
                                                              CustomFecha1,
                                                              CustomFecha2,
                                                              CustomFecha3,
                                                              REQUEST.CustomTextField_1,
                                                              REQUEST.CustomTextField_2,
                                                              REQUEST.CustomTextField_3,
                                                              Linea,
                                                              item2.LPN_CodigoBarraExt,
                                                              item2.TipoReferencia,
                                                              item2.NumeroReferencia,
                                                              item2.TipoDocumento,
                                                              item2.NumeroDocumento,
                                                              FechaDocto,
                                                              item2.CodigoArticulo,
                                                              item2.Cantidad,
                                                              item2.DetCustomDecimalField_1,
                                                              item2.DetCustomDecimalField_2,
                                                              item2.DetCustomDecimalField_3,
                                                              DetCustomDateField_1,
                                                              DetCustomDateField_2,
                                                              DetCustomDateField_3,
                                                              item2.DetCustomTextField_1,
                                                              item2.DetCustomTextField_2,
                                                              item2.DetCustomTextField_3
                                                              ).ToList();
                if (SOLICITUD_ASN.Count > 0)
                {
                    if (SOLICITUD_ASN[0].Count > 0) //pregunta por el campo Count, si es mayor a cero procesó OK la cabecera
                    {
                        //RESPONSE.AsnId = (int)SOLICITUD_ASN[0].ReciboAsnId;
                        //RESPONSE.descripcion = SOLICITUD_ASN[0].Descripcion; //en Descripcion viene el nombre de archivo
                        
                        Archivo = SOLICITUD_ASN[0].Descripcion;
                        ReciboAsnId = (decimal)SOLICITUD_ASN[0].ReciboAsnId;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO
                        RESPONSE.resultado = "ERROR";
                        RESPONSE.descripcion = SOLICITUD_ASN[0].Descripcion;
                        RESPONSE.resultado_codigo = ERROR.ErrID;
                        RESPONSE.resultado_descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = "";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

                Linea = Linea + 1;

            } //FIN ciclo recorre items

            //----------------------------------------------
            //Genera proceso que carga tabla ASN --------------
            //----------------------------------------------------

            List<sp_GP_INT_RecibosASN_LPN_Result> RESULTADO = GP_ENT.sp_GP_INT_RecibosASN_LPN(Archivo,
                                                                                              REQUEST.Usuario,
                                                                                              FechaCreacion,
                                                                                              REQUEST.Proceso,
                                                                                              ReciboAsnId).ToList();
            RESPONSE.AsnId = (int)RESULTADO[0].ReciboAsnId;
            RESPONSE.resultado = (RESULTADO.ElementAt(0).Generacion == 1 ? "OK" : "ERROR");
            RESPONSE.descripcion = RESULTADO.ElementAt(0).GlosaEstado.Trim();
            RESPONSE.resultado_codigo = 0;
            RESPONSE.resultado_descripcion = (RESULTADO.ElementAt(0).Generacion == 1 ? "Proceso ASN exitoso" : "Error al integrar");

            //Si llega hasta aca no tuvo errores -----
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion        

        //recurso 39 CREAR SOLICITUD DESPACHO JSON CON TABLA TEMPORAL (39) ==============
        #region CREAR SOLICITUD DESPACHO JSON CON TABLA TEMPORAL (39) ****************
        [HttpPost]
        [Route("SOLDESP/CREARJSON2")]
        public IHttpActionResult recurso18_TMP([FromBody] API_REQUEST_TYPE_18 REQUEST)
        {
            API_RESPONSE_TYPE_8 RESPONSE = new API_RESPONSE_TYPE_8();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/CREARJSON2";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK

                LogInfo(NombreProceso,
                        "La estructura JSON viene vacia",
                        true,
                        true,
                        "",
                        JsonConvert.SerializeObject(REQUEST).ToString());

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "La estructura JSON viene vacia";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON recibida",
                    true,
                    true,
                    (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia),
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        "ERROR: " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                LogInfo(NombreProceso,
                        "ERROR: " + ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR

                LogInfo(NombreProceso,
                        "ERROR: " + ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion
            //Guarda JSON recibido en el log ----------
            //body = JsonConvert.SerializeObject(REQUEST);
            //LogInfo(NombreProceso,
            //        "Creación SDD, JSON: " + body.ToString(),
            //        false,
            //        true,
            //        (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia),
            //        body.ToString());

            string ListadoCodigoArticulo = "";
            int limpiar;
            String Archivo = "";

            //setea variable de respuesta en OK ----
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.count = 1;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----
                REQUEST.Proceso = (REQUEST.Proceso == null ? "" : REQUEST.Proceso); //opcional
                //REQUEST.empid // se valida con el usuario
                if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                //REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                //REQUEST.tipoDocumento //opcional numerico, toma cero
                REQUEST.NumeroDocto = (REQUEST.NumeroDocto == null ? "" : REQUEST.NumeroDocto); //opcional
                REQUEST.FechaDocto = (REQUEST.FechaDocto == null ? "" : REQUEST.FechaDocto); //opcional
                REQUEST.Glosa = (REQUEST.Glosa == null ? "" : REQUEST.Glosa); //opcional
                REQUEST.Cliente = (REQUEST.Cliente == null ? "" : REQUEST.Cliente); //opcional
                REQUEST.RazonSocial = (REQUEST.RazonSocial == null ? "" : REQUEST.RazonSocial); //opcional
                REQUEST.Telefono = (REQUEST.Telefono == null ? "" : REQUEST.Telefono); //opcional
                REQUEST.Email = (REQUEST.Email == null ? "" : REQUEST.Email); //opcional
                REQUEST.Direccion = (REQUEST.Direccion == null ? "" : REQUEST.Direccion); //opcional
                REQUEST.Contacto = (REQUEST.Contacto == null ? "" : REQUEST.Contacto); //opcional
                //REQUEST.rutaDespacho = (REQUEST.rutaDespacho == null ? "" : REQUEST.rutaDespacho); //opcional numerico, toma cero
                REQUEST.Region = (REQUEST.Region == null ? "" : REQUEST.Region); //opcional
                REQUEST.Comuna = (REQUEST.Comuna == null ? "" : REQUEST.Comuna); //opcional
                REQUEST.Ciudad = (REQUEST.Ciudad == null ? "" : REQUEST.Ciudad); //opcional
                REQUEST.Vendedor = (REQUEST.Vendedor == null ? "" : REQUEST.Vendedor); //opcional
                REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional

                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional
                //REQUEST.Valor1 //opcional numerico, toma cero
                //REQUEST.Valor2 //opcional numerico, toma cero
                //REQUEST.Valor3 //opcional numerico, toma cero
                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                //REQUEST.Sucursal //opcional numerico, toma cero

                int PrimerItem = 1;

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadVenta == null || item.UnidadVenta == "") { throw new Exception("Debe informar Unidad Venta"); } //requerido
                    item.NumeroSerie = item.NumeroSerie == null ? "" : item.NumeroSerie; //opcional
                    item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido
                    //item.costoUnitario //opcional numerico, toma cero
                    //item.kilosTotales //opcional numerico, toma cero
                    //item.porcQa //opcional numerico, toma cero
                    //item.maquila //opcional numerico, toma cero
                    item.Pallet = (item.Pallet == null ? "" : item.Pallet); //opcional
                                                                            //item.itemReferencia //opcional numerico, toma cero
                    item.Dato1 = item.Dato1 == null ? "" : item.Dato1; //opcional
                    item.Dato2 = item.Dato2 == null ? "" : item.Dato2; //opcional
                    item.Dato3 = item.Dato3 == null ? "" : item.Dato3; //opcional
                    //item.Valor1 //opcional numerico, toma cero
                    //item.Valor2 //opcional numerico, toma cero
                    //item.Valor3 //opcional numerico, toma cero
                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional

                    //REQUEST.items.Count --> cantidad items que trae el json 

                    //concatena items de la solicitud con ;
                    if (PrimerItem == 1)
                    {
                        ListadoCodigoArticulo = item.CodigoArticulo;
                        PrimerItem = 0;
                    }
                    else
                    {
                        ListadoCodigoArticulo = ListadoCodigoArticulo + ";" + item.CodigoArticulo;
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        "ERROR: " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaReferencia = new DateTime(); //fechaReferencia
            DateTime FechaProceso = new DateTime(); //FechaProceso
            DateTime FechaDocto = new DateTime(); //fechaDocto
            DateTime FechaVectoLote = new DateTime(); //fechaVectoLote
            DateTime Fecha1Cab = new DateTime(); //Fecha1 cabecera
            DateTime Fecha2Cab = new DateTime(); //Fecha2 cabecera
            DateTime Fecha3Cab = new DateTime(); //Fecha3 cabecera
            DateTime Fecha1 = new DateTime(); //Fecha1
            DateTime Fecha2 = new DateTime(); //Fecha2
            DateTime Fecha3 = new DateTime(); //Fecha3

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA DATOS CABECERA JSON -----
                FechaReferencia = ValidaCampoFecha(REQUEST.FechaReferencia, "FechaReferencia");
                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "FechaProceso");
                FechaDocto = ValidaCampoFecha(REQUEST.FechaDocto, "FechaDocto");
                Fecha1Cab = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1 cabecera");
                Fecha2Cab = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2 cabecera");
                Fecha3Cab = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3 cabecera");

                //VALIDA DATOS ITEMS DETALLE JSON
                foreach (var item in REQUEST.Items.ToList())
                {
                    FechaVectoLote = ValidaCampoFecha(item.FechaVectoLote, "FechaVectoLote");
                    Fecha1 = ValidaCampoFecha(item.Fecha1, "Fecha1 detalle");
                    Fecha2 = ValidaCampoFecha(item.Fecha2, "Fecha2 detalle");
                    Fecha3 = ValidaCampoFecha(item.Fecha3, "Fecha3 detalle");
                }                
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        "ERROR: " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            //List<sp_in_API_SolDespJson_Result> SOLDESPJSON = new List<sp_in_API_SolDespJson_Result>();
            //List<sp_in_API_SolDespDetJson_Result> SOLDESPDETJSON = new List<sp_in_API_SolDespDetJson_Result>();
            //List<sp_proc_API_SolDespachoValidaJson_Result> SOLDESPVALIDA = new List<sp_proc_API_SolDespachoValidaJson_Result>();

            List<sp_in_API_TMPSolDespachoJson_Result> TMPSOLDESPACHOJSON = new List<sp_in_API_TMPSolDespachoJson_Result>();
            List<sp_proc_API_TMPSolDespachoJson_Result> SOLDESPACHO_PROCESA = new List<sp_proc_API_TMPSolDespachoJson_Result>();

            //Fecha1 = new DateTime(); //Fecha1
            //Fecha2 = new DateTime(); //Fecha2
            //Fecha3 = new DateTime(); //Fecha3

            limpiar = 1; //debe limpiar los datos anteriores de la tabla de paso para el Usuario -----

            Archivo = "SOLDESP_CREARJSON_" + REQUEST.NumeroReferencia.Trim() + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                //Fechas detalle (fechas cabecera se cargan al validar)
                FechaVectoLote = ValidaCampoFecha(item2.FechaVectoLote, "FechaVectoLote");
                Fecha1 = ValidaCampoFecha(item2.Fecha1, "Fecha1 detalle");
                Fecha2 = ValidaCampoFecha(item2.Fecha2, "Fecha2 detalle");
                Fecha3 = ValidaCampoFecha(item2.Fecha3, "Fecha3 detalle");

                try
                {
                    //Inserta linea de cabecera/detalle a tabla temporal SDD ----------------------
                    TMPSOLDESPACHOJSON = GP_ENT.sp_in_API_TMPSolDespachoJson(Archivo,
                                                                             USERNAME,
                                                                             REQUEST.Proceso,
                                                                             REQUEST.Empid,
                                                                             REQUEST.TipoSolicitud,
                                                                             REQUEST.CodigoBodega,
                                                                             REQUEST.TipoReferencia,
                                                                             REQUEST.NumeroReferencia,
                                                                             FechaReferencia,
                                                                             FechaProceso,
                                                                             REQUEST.TipoDocumento,
                                                                             REQUEST.NumeroDocto,
                                                                             FechaDocto,
                                                                             REQUEST.Glosa,
                                                                             REQUEST.Cliente,
                                                                             REQUEST.RazonSocial,
                                                                             REQUEST.Telefono,
                                                                             REQUEST.Email,
                                                                             REQUEST.Direccion,
                                                                             REQUEST.Contacto,
                                                                             REQUEST.RutaDespacho,
                                                                             REQUEST.Region,
                                                                             REQUEST.Comuna,
                                                                             REQUEST.Ciudad,
                                                                             REQUEST.Vendedor,
                                                                             REQUEST.Comprador,
                                                                             REQUEST.Dato1,
                                                                             REQUEST.Dato2,
                                                                             REQUEST.Dato3,
                                                                             REQUEST.Valor1,
                                                                             REQUEST.Valor2,
                                                                             REQUEST.Valor3,
                                                                             Fecha1Cab,
                                                                             Fecha2Cab,
                                                                             Fecha3Cab,
                                                                             REQUEST.Sucursal,
                                                                             item2.CodigoArticulo,
                                                                             item2.UnidadVenta,
                                                                             item2.NumeroSerie,
                                                                             FechaVectoLote,
                                                                             item2.Cantidad,
                                                                             item2.Estado,
                                                                             item2.CostoUnitario,
                                                                             item2.KilosTotales,
                                                                             item2.PorcQa,
                                                                             item2.Maquila,
                                                                             item2.Pallet,
                                                                             item2.ItemReferencia,
                                                                             Fecha1,
                                                                             Fecha2,
                                                                             Fecha3,
                                                                             item2.Dato1,
                                                                             item2.Dato2,
                                                                             item2.Dato3,
                                                                             item2.Valor1,
                                                                             item2.Valor2,
                                                                             item2.Valor3,
                                                                             item2.Sucursal,
                                                                             limpiar
                                                                             ).ToList();
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                    LogInfo(NombreProceso,
                            "ERROR: " + ex.Message.Trim(),
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = ex.Message.Trim();
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

                //Si retorna respuesta 
                if (TMPSOLDESPACHOJSON.Count > 0)
                {
                    if (TMPSOLDESPACHOJSON[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017); //REQUEST - ERROR PROCESO BASE DE DATOS

                        LogInfo(NombreProceso,
                                "Error generando SDD. " + TMPSOLDESPACHOJSON[0].Descripcion,
                                true,
                                true,
                                REQUEST.NumeroReferencia,
                                body.ToString());

                        RESPONSE.resultado = "ERROR";
                        RESPONSE.descripcion = "Error generando SDD. " + TMPSOLDESPACHOJSON[0].Descripcion;
                        RESPONSE.resultado_codigo = ERROR.ErrID;
                        RESPONSE.resultado_descripcion = ERROR.Mensaje;
                        RESPONSE.solDespId = 0;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = "";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

                limpiar = 0; //para que no vuelva a limpiar la tabla, solo siga agregando

            }// fin ciclo items

            //--------------------------------------------------------------------------------------------------------------------------------
            //Al finalizar OK el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
            //--------------------------------------------------------------------------------------------------------------------------------
            try
            {
                SOLDESPACHO_PROCESA = GP_ENT.sp_proc_API_TMPSolDespachoJson(Archivo,
                                                                            USERNAME
                                                                            ).ToList();
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        "ERROR: " + ex.Message.Trim() + ". " + ex.InnerException.ToString().Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim() + ". " + ex.InnerException.ToString().Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            if (SOLDESPACHO_PROCESA.Count > 0)
            {
                if (SOLDESPACHO_PROCESA[0].Resultado == "OK")
                {
                    RESPONSE.resultado = "OK";
                    RESPONSE.descripcion = SOLDESPACHO_PROCESA[0].Descripcion;
                    RESPONSE.solDespId = int.Parse(SOLDESPACHO_PROCESA[0].SolDespId.ToString()); //id SDD generada *****
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + SOLDESPACHO_PROCESA[0].Descripcion,
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = SOLDESPACHO_PROCESA[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    RESPONSE.solDespId = 0;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 40 CREAR SOLICITUD RECEPCION JSON CON TABLA TEMPORAL (40) =============
        #region CREAR SOLICITUD RECEPCION JSON CON TABLA TEMPORAL(40) *************
        [HttpPost]
        [Route("SOLRECEP/CREARJSON2")]
        public IHttpActionResult recurso17_TMP([FromBody] API_REQUEST_TYPE_17 REQUEST)
        {
            API_RESPONSE_TYPE_7 RESPONSE = new API_RESPONSE_TYPE_7();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLRECEP/CREARJSON2";

            //valida estructura json recibido sea válida y que no venga vacio ---
            #region VALIDA JSON RECIBIDO NOK

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK

                LogInfo(NombreProceso,
                        "La estructura JSON viene vacia",
                        true,
                        true,
                        "",
                        JsonConvert.SerializeObject(REQUEST).ToString());

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "La estructura JSON viene vacia";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log --------------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON recibida",
                    true,
                    true,
                    (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia),
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        "ERROR: " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Valida Token y Secret, retorna Usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                LogInfo(NombreProceso,
                        "ERROR: " + ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, "ERROR: " + ERROR.Mensaje, true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES

            //valida datos cabecera json
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan con un cero
            // - un campo string se puede dejar opcional enviando un valor por defecto 

            string ListadoCodigoArticulo = "";

            try
            {
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.TipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                //REQUEST.CodigoBodega = (REQUEST.CodigoBodega == null ? "" : REQUEST.CodigoBodega); //opcional numerico, toma cero
                REQUEST.Comprador = (REQUEST.Comprador == null ? "" : REQUEST.Comprador); //opcional
                if (REQUEST.Proveedor == null || REQUEST.Proveedor == "") { throw new Exception("Debe informar Proveedor"); } //requerido
                if (REQUEST.RazonSocial == null || REQUEST.RazonSocial == "") { throw new Exception("Debe informar Razon Social"); } //requerido
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.FechaReferencia == null || REQUEST.FechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                if (REQUEST.Glosa == null || REQUEST.Glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido
                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional

                //decimal Valor1 //opcional numerico, toma cero
                //decimal Valor2 //opcional numerico, toma cero
                //decimal Valor3 //opcional numerico, toma cero

                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional                    
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                int PrimerItem = 1;

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadCompra == null || item.UnidadCompra == "") { throw new Exception("Debe informar Unidad Compra"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    item.NumeroSerie = (item.NumeroSerie == null ? "" : item.NumeroSerie); //opcional
                    item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido

                    //item.PorcQA //opcional numerico, toma cero
                    //item.SolDespIdCD //opcional numerico, toma cero
                    //item.CantidadCD //opcional numerico, toma cero

                    item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional

                    //decimal Valor1 //opcional numerico, toma cero
                    //decimal Valor2 //opcional numerico, toma cero
                    //decimal Valor3 //opcional numerico, toma cero

                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional                    
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional

                    //Valida datos de Items json, si trae detalles de crossdocking los campos son obligatorios
                    if (item.Crossdocking != null)
                    {
                        foreach (var Crossdocking1 in item.Crossdocking)
                        {
                            if (Crossdocking1.TipoReferenciaCD == null || Crossdocking1.TipoReferenciaCD == "") { throw new Exception("Debe informar Tipo Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.NumeroReferenciaCD == null || Crossdocking1.NumeroReferenciaCD == "") { throw new Exception("Debe informar Numero Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.ItemReferenciaCD <= 0) { throw new Exception("Debe informar Item Referencia Pedido Crossdocking"); } //requerido
                            if (Crossdocking1.CantidadCD <= 0) { throw new Exception("Debe informar Cantidad Item " + Crossdocking1.ItemReferenciaCD.ToString().Trim() + " Crossdocking > 0"); } //requerido
                        }
                    }

                    //REQUEST.items.Count --> cantidad items que trae el json 

                    //concatena items de la solicitud con ;
                    if (PrimerItem == 1)
                    {
                        ListadoCodigoArticulo = item.CodigoArticulo;
                        PrimerItem = 0;
                    }
                    else
                    {
                        ListadoCodigoArticulo = ListadoCodigoArticulo + ";" + item.CodigoArticulo;
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso, "ERROR: " + ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA FORMATO FECHAS

            DateTime FechaReferencia = new DateTime(); //fechaReferencia
            DateTime fechaVectoLote = new DateTime(); //fechaVectoLote
            DateTime Fecha1Cab = new DateTime();
            DateTime Fecha2Cab = new DateTime();
            DateTime Fecha3Cab = new DateTime();
            DateTime fecha1 = new DateTime();
            DateTime fecha2 = new DateTime();
            DateTime fecha3 = new DateTime();
            DateTime FechaProceso = new DateTime();

            try
            {
                //VALIDA FECHAS CABECERA JSON -----
                FechaReferencia = ValidaCampoFecha(REQUEST.FechaReferencia, "FECHAREFERENCIA");
                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "FECHAPROCESO");
                Fecha1Cab = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1 cabecera");
                Fecha2Cab = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2 cabecera");
                Fecha3Cab = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3 cabecera");

                //VALIDA FECHAS ITEMS DETALLE JSON -----
                foreach (var item in REQUEST.Items.ToList())
                {
                    fechaVectoLote = ValidaCampoFecha(item.FechaVectoLote, "FECHAVECTOLOTE");
                    fecha1 = ValidaCampoFecha(item.Fecha1, "Fecha1 detalle");
                    fecha2 = ValidaCampoFecha(item.Fecha2, "Fecha2 detalle");
                    fecha3 = ValidaCampoFecha(item.Fecha3, "Fecha3 detalle");

                } //FIN ciclo items
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso, "ERROR: " + ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, REQUEST.NumeroReferencia, body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de validaciones iniciales realiza proceso --- 
            #region PROCESAMIENTO 

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            //List<sp_in_API_SolRecepcionJson_Result> SOLRECEP = new List<sp_in_API_SolRecepcionJson_Result>();
            //List<sp_in_API_SolRecepcionDetJson_Result> SOLRECEPDET = new List<sp_in_API_SolRecepcionDetJson_Result>();

            List<sp_in_API_TMPSolRecepcionJson_Result> TMPSOLRECEPCIONJSON = new List<sp_in_API_TMPSolRecepcionJson_Result>();
            List<sp_proc_API_TMPSolRecepcionJson_Result> SOLRECEP_PROCESA = new List<sp_proc_API_TMPSolRecepcionJson_Result>();

            string Item_crossdocking = "";
            string Archivo = "SOLRECEP_CREARJSON_" + REQUEST.NumeroReferencia.Trim() + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                Item_crossdocking = "";

                //si tiene detalle de crossdocking lo envia
                if (item2.Crossdocking != null)
                {
                    foreach (var croosdock2 in item2.Crossdocking.ToList())
                    {
                        if (Item_crossdocking.Trim() == "") //Si es el primer item del crossdocking
                        {
                            Item_crossdocking = croosdock2.TipoReferenciaCD.Trim() + ";" +
                                                croosdock2.NumeroReferenciaCD.Trim() + ";" +
                                                croosdock2.ItemReferenciaCD.ToString().Trim() + ";" +
                                                croosdock2.CantidadCD.ToString();
                        }
                        else
                        {
                            Item_crossdocking = Item_crossdocking.Trim() + "#" + croosdock2.TipoReferenciaCD.Trim() + ";" +
                                                                                    croosdock2.NumeroReferenciaCD.Trim() + ";" +
                                                                                    croosdock2.ItemReferenciaCD.ToString().Trim() + ";" +
                                                                                    croosdock2.CantidadCD.ToString();
                        }
                    }
                }
               
                //carga variables fecha item detalle (variables fechas cabecera quedaron cargadas al validar formato) -----------------
                fechaVectoLote = ValidaCampoFecha(item2.FechaVectoLote, "FECHAVECTOLOTE");
                fecha1 = ValidaCampoFecha(item2.Fecha1, "Fecha1 detalle");
                fecha2 = ValidaCampoFecha(item2.Fecha2, "Fecha2 detalle");
                fecha3 = ValidaCampoFecha(item2.Fecha3, "Fecha3 detalle");

                try
                {
                    //Inserta linea de cabecera/detalle a tabla temporal SDR ----------------------
                    TMPSOLRECEPCIONJSON = GP_ENT.sp_in_API_TMPSolRecepcionJson(Archivo,
                                                                               REQUEST.Empid,
                                                                               USERNAME,
                                                                               FechaProceso,
                                                                               REQUEST.TipoSolicitud,
                                                                               REQUEST.CodigoBodega,
                                                                               REQUEST.Comprador,
                                                                               REQUEST.Proveedor,
                                                                               REQUEST.RazonSocial,
                                                                               REQUEST.TipoReferencia,
                                                                               REQUEST.NumeroReferencia,
                                                                               FechaReferencia,
                                                                               REQUEST.Glosa,
                                                                               REQUEST.Dato1,
                                                                               REQUEST.Dato2,
                                                                               REQUEST.Dato3,
                                                                               REQUEST.Valor1,
                                                                               REQUEST.Valor2,
                                                                               REQUEST.Valor3,
                                                                               Fecha1Cab,
                                                                               Fecha2Cab,
                                                                               Fecha3Cab,
                                                                               item2.CodigoArticulo,
                                                                               item2.UnidadCompra,
                                                                               item2.Cantidad,
                                                                               item2.ItemReferencia,
                                                                               item2.CostoUnitario,
                                                                               item2.KilosTotales,
                                                                               item2.NumeroSerie,
                                                                               fechaVectoLote,
                                                                               item2.Estado,
                                                                               item2.PorcQA,
                                                                               item2.Dato1,
                                                                               item2.Dato2,
                                                                               item2.Dato3,
                                                                               item2.Valor1,
                                                                               item2.Valor2,
                                                                               item2.Valor3,
                                                                               fecha1,
                                                                               fecha2,
                                                                               fecha3,
                                                                               item2.Sucursal,
                                                                               Item_crossdocking
                                                                               ).ToList();
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                    LogInfo(NombreProceso,
                            "ERROR: " + ex.Message.Trim(),
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = ex.Message.Trim();
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

                //Si retorna respuesta 
                if (TMPSOLRECEPCIONJSON.Count > 0)
                {
                    if (TMPSOLRECEPCIONJSON[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                "ERROR: " + ERROR.Mensaje.Trim() + ". " + "Error procesando Articulo: " + item2.CodigoArticulo + ", " + TMPSOLRECEPCIONJSON[0].Descripcion,
                                true,
                                true,
                                REQUEST.NumeroReferencia,
                                body.ToString());

                        RESPONSE.resultado = "ERROR";
                        RESPONSE.descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + TMPSOLRECEPCIONJSON[0].Descripcion;
                        RESPONSE.resultado_codigo = ERROR.ErrID;
                        RESPONSE.resultado_descripcion = ERROR.Mensaje;
                        RESPONSE.solRecepId = 0;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = "";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

            } //fin ciclo items

            try
            {
                //-------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //-------------------------------------------------------------------------------------------------------------------------------
                SOLRECEP_PROCESA = GP_ENT.sp_proc_API_TMPSolRecepcionJson(Archivo,
                                                                          "INTEGRADOR_API"
                                                                          ).ToList();
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        "ERROR: " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = ex.Message.Trim();
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            if (SOLRECEP_PROCESA.Count > 0)
            {
                if (SOLRECEP_PROCESA[0].Resultado == "OK")
                {
                    RESPONSE.resultado = "OK";
                    RESPONSE.descripcion = SOLRECEP_PROCESA[0].Descripcion;
                    RESPONSE.solRecepId = int.Parse(SOLRECEP_PROCESA[0].SolRecepId.ToString()); //recupera id generado SDR
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + SOLRECEP_PROCESA[0].Descripcion,
                            true,
                            true,
                            REQUEST.NumeroReferencia,
                            body.ToString());

                    RESPONSE.resultado = "ERROR";
                    RESPONSE.descripcion = SOLRECEP_PROCESA[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    RESPONSE.resultado_descripcion = ERROR.Mensaje;
                    RESPONSE.solRecepId = 0;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.resultado = "ERROR";
                RESPONSE.descripcion = "";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion        

        //recurso 41 Cambio de Estado Lote ----------------------
        //  MOTIVO = 1  --> Cambio estado, cambio lote
        //  MOTIVO = 2  --> SOLO FECHA (todas las sucursales)
        #region CAMBIO DE ESTADO LOTE (41) ***************
        [HttpPost]
        [Route("MOVIMIENTOS/CAMBIOESTADOLOTE")]
        public IHttpActionResult recurso41([FromBody] API_REQUEST_TYPE_41_CambioEstado REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "MOVIMIENTOS/CAMBIOESTADOLOTE";

            //Valida Token y Secret, retorna Usuario
            #region VALIDA ACCESO API/RECURSO NOK

            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, ERROR.Mensaje.Trim(), true, true, "", "");

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea válida y que no venga vacio ---
            #region VALIDA JSON RECIBIDO NOK
            //1. Valida json valido
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(), true, true, "", "");

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Valida json en blanco ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso, ERROR.Mensaje.Trim() + ". " + "El json viene vacio", true, true, "", "");

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "El json viene vacio";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Cambio Estado Lote, JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            //valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                LogInfo(NombreProceso, ERROR.Mensaje, true, true, "", body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES

            //valida datos cabecera json
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan con un cero
            // - un campo string se puede dejar opcional enviando un valor por defecto 

            try
            {
                if (REQUEST.Usuario == null || REQUEST.Usuario == "") { throw new Exception("Debe informar Usuario responsable"); } //requerido
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.Motivo <= 0) { throw new Exception("Debe informar Motivo"); } //requerido
                if (REQUEST.Glosa == null || REQUEST.Glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido
                if (REQUEST.EstadoProdOrig <= 0) { throw new Exception("Debe informar estado de origen de los lotes"); } //requerido
                if (REQUEST.EstadoProdDest <= 0) { throw new Exception("Debe informar estado de destino de los lotes"); } //requerido
                REQUEST.Certificado = (REQUEST.Certificado == null ? "" : REQUEST.Certificado); //opcional 
                REQUEST.FechaCertificado = (REQUEST.FechaCertificado == null ? "" : REQUEST.FechaCertificado); //opcional 
                REQUEST.Certificado2 = (REQUEST.Certificado2 == null ? "" : REQUEST.Certificado2); //opcional 
                REQUEST.FechaCertificado2 = (REQUEST.FechaCertificado2 == null ? "" : REQUEST.FechaCertificado2); //opcional 
                //int CodigoBodega //opcional numerico, toma cero
                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional
                //decimal Valor1 //opcional numerico, toma cero
                //decimal Valor2 //opcional numerico, toma cero
                //decimal Valor3 //opcional numerico, toma cero
                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional                    
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido 
                    if (item.UnidadMedida == null || item.UnidadMedida == "") { throw new Exception("Debe informar Unidad Medida"); } //requerido 
                    if (item.NumeroLote == null || item.NumeroLote == "") { throw new Exception("Debe informar Lote original"); } //requerido 
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    if (item.NuevoLote == null || item.NuevoLote == "") { throw new Exception("Debe informar nuevo Lote"); } //requerido 
                    if (item.NuevoFecVecto == null || item.NuevoFecVecto == "") { throw new Exception("Debe informar nueva Fecha de Vencimiento del nuevo Lote"); } //requerido 
                    item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional
                    //decimal Valor1 //opcional numerico, toma cero
                    //decimal Valor2 //opcional numerico, toma cero
                    //decimal Valor3 //opcional numerico, toma cero
                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional 
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso, ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, "", body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATO FECHAS

            DateTime FechaProceso = new DateTime();
            DateTime FechaCertificado = new DateTime();
            DateTime FechaCertificado2 = new DateTime();
            DateTime NuevoFecVecto = new DateTime();
            DateTime Fecha1Cab = new DateTime(); //fecha1 cabecera
            DateTime Fecha2Cab = new DateTime(); //fecha2 cabecera
            DateTime Fecha3Cab = new DateTime(); //fecha3 cabecera
            DateTime Fecha1det = new DateTime(); //Fecha1 detalle
            DateTime Fecha2det = new DateTime(); //Fecha2 detalle
            DateTime Fecha3det = new DateTime(); //Fecha3 detalle

            try
            {
                //VALIDA FECHAS CABECERA JSON -----

                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "Fecha Proceso");
                FechaCertificado = ValidaCampoFecha(REQUEST.FechaCertificado, "Fecha Certificado");
                FechaCertificado2 = ValidaCampoFecha(REQUEST.FechaCertificado2, "Fecha Certificado2");
                Fecha1Cab = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1 cabecera");
                Fecha2Cab = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2 cabecera");
                Fecha3Cab = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3 cabecera");

                //VALIDA FECHAS ITEMS DETALLE JSON -----
                foreach (var item in REQUEST.Items.ToList())
                {
                    NuevoFecVecto = ValidaCampoFecha(item.NuevoFecVecto, "nueva Fecha vencimiento Lote");
                    Fecha1det = ValidaCampoFecha(item.Fecha1, "Fecha1 detalle");
                    Fecha2det = ValidaCampoFecha(item.Fecha2, "Fecha2 detalle");
                    Fecha3det = ValidaCampoFecha(item.Fecha3, "Fecha3 detalle");
                } //FIN ciclo items
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso, ERROR.Mensaje + ". " + ex.Message.Trim(), true, true, "", body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de validaciones iniciales realiza proceso --- 
            #region PROCESAMIENTO 

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Archivo = "CAMBIO_ESTADO_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                //carga variables fecha item detalle (variables fechas cabecera quedaron cargadas al validar formato) -----------------
                NuevoFecVecto = ValidaCampoFecha(item2.NuevoFecVecto, "nueva Fecha vencimiento Lote");
                Fecha1det = ValidaCampoFecha(item2.Fecha1, "Fecha1");
                Fecha2det = ValidaCampoFecha(item2.Fecha2, "Fecha2");
                Fecha3det = ValidaCampoFecha(item2.Fecha3, "Fecha3");

                //Inserta linea de cabecera/detalle a tabla temporal SDR ----------------------
                //Se llama a la funcion INSERTA_sp_in_API_Integraciones
                //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inerta hasta el Texto100
                //  - La funcion Permite enviar los textos como parametros opcionales

                //INTEGRACIONES = GP_ENT.sp_in_API_Integraciones(Archivo,
                INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT, 
                                                                Archivo,
                                                                USERNAME,
                                                                Fecha,
                                                                Linea,
                                                                "CAMBIO-ESTADO",
                                                                REQUEST.Empid.ToString(),
                                                                REQUEST.Usuario,
                                                                FechaProceso.ToString("yyyyMMdd"),
                                                                REQUEST.Motivo.ToString(),
                                                                REQUEST.Glosa,
                                                                REQUEST.EstadoProdOrig.ToString(),
                                                                REQUEST.EstadoProdDest.ToString(),
                                                                REQUEST.Certificado,
                                                                FechaCertificado.ToString("yyyyMMdd"),
                                                                REQUEST.Certificado2,
                                                                FechaCertificado2.ToString("yyyyMMdd"),
                                                                REQUEST.CodigoBodega.ToString(),
                                                                item2.CodigoArticulo,
                                                                item2.UnidadMedida,
                                                                item2.NumeroLote,
                                                                item2.Cantidad.ToString(),
                                                                item2.NuevoLote,
                                                                NuevoFecVecto.ToString("yyyyMMdd"),
                                                                USERNAME,
                                                                REQUEST.Dato1,
                                                                REQUEST.Dato2,
                                                                REQUEST.Dato3,
                                                                REQUEST.Valor1.ToString(),
                                                                REQUEST.Valor2.ToString(),
                                                                REQUEST.Valor3.ToString(),
                                                                Fecha1Cab.ToString("yyyyMMdd"),
                                                                Fecha2Cab.ToString("yyyyMMdd"),
                                                                Fecha3Cab.ToString("yyyyMMdd"),
                                                                item2.Dato1,
                                                                item2.Dato2,
                                                                item2.Dato3,
                                                                item2.Valor1.ToString(),
                                                                item2.Valor2.ToString(),
                                                                item2.Valor3.ToString(),
                                                                Fecha1det.ToString("yyyyMMdd"),
                                                                Fecha2det.ToString("yyyyMMdd"),
                                                                Fecha3det.ToString("yyyyMMdd")
                                                                ).ToList();
                if (INTEGRACIONES.Count > 0)
                {
                    if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + INTEGRACIONES[0].Descripcion;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

            } //fin ciclo items del JSON

            #region comentario ----
            //-------------------------------------------------------------------------------------------------------------------------------
            ////Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
            //INTEGRACIONES_PROCESA = GP_ENT.sp_proc_API_Integraciones(Archivo,
            //                                                         USERNAME).ToList();
            //if (INTEGRACIONES_PROCESA.Count > 0)
            //{
            //    if (INTEGRACIONES_PROCESA[0].Resultado == "OK")
            //    {
            //        RESPONSE.Resultado = "OK";
            //        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].Descripcion;
            //    }
            //    else
            //    {
            //        #region NOK
            //        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

            //        LogInfo(NombreProceso,
            //                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].Descripcion,
            //                true,
            //                true,
            //                "",
            //                body.ToString());

            //        RESPONSE.Resultado = "ERROR";
            //        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].Descripcion;
            //        RESPONSE.Resultado_Codigo = ERROR.ErrID;
            //        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
            //        STATUS_CODE = HttpStatusCode.BadRequest;
            //        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //        #endregion
            //    }
            //}
            //else
            //{
            //    #region NOK
            //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

            //    LogInfo(NombreProceso,
            //            ERROR.Mensaje.Trim(),
            //            true,
            //            true,
            //            "",
            //            body.ToString());

            //    RESPONSE.Resultado = "ERROR";
            //    RESPONSE.Descripcion = "";
            //    RESPONSE.Resultado_Codigo = ERROR.ErrID;
            //    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
            //    STATUS_CODE = HttpStatusCode.BadRequest;
            //    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            //    #endregion
            //}
            #endregion

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente todo el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion 

        //recurso 42 Evolutivo por Mes en Posiciones Cliente ----------
        #region Evolutivo por Mes en Posiciones Cliente (42)
        [HttpGet]
        [HttpPost]
        [Route("CLIENTE/EVOLMESCLIENTE")]
        public IHttpActionResult Recurso42([FromBody] API_REQUEST_TYPE_42_EvolutivoMesCliente REQUEST)
        {
            API_RESPONSE_TYPE_42_EVOLUTIVO_CAB RESPONSE = new API_RESPONSE_TYPE_42_EVOLUTIVO_CAB();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "CLIENTE/EVOLMESCLIENTE";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso, 
                    "Estructura JSON: " + body.ToString(), 
                    true, 
                    true, 
                    "", 
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR";
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y secret, retorna usuario 
            #region VALIDA TOKEN Y SECRET, RETORNA USUARIO 
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_6;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR";
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //valida usuario empresa ---
            #region VALIDA USUARIO EMPRESA
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR";
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES JSON

            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre vendra un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional

            try
            {
                if (REQUEST.Empid <= 0) { throw new Exception("Debe informar Empresa > 0"); } //requerido
                if ((REQUEST.Mes < 0) || (REQUEST.Mes > 12)) { throw new Exception("Debe informar Mes (0=Todos o 1 a 12)"); } //requerido
                if (REQUEST.Ejercicio <= 0) { throw new Exception("Debe informar Ejercicio"); } //requerido
                if (REQUEST.Limit < 0) { throw new Exception("Debe informar Limit"); } //requerido
                if (REQUEST.Rowset < 0) { throw new Exception("Debe informar Rowset"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); // ERROR EN CUERPO RECIBIDO
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR - " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //Luego de cumplir las validaciones realiza proceso ---
            #region PROCESAMIENTO

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 0;
            RESPONSE.Resultado = "OK";

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_sel_API_EvolutivoMesCliente_Result> EVOLUTIVO_MESES = new List<sp_sel_API_EvolutivoMesCliente_Result>();

            API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM RESPONSE_ITEMS = new API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM();

            //int CantidadOK = 0;
            //int CantidadERROR = 0;

            try
            {
                //Ejecuta consulta 
                EVOLUTIVO_MESES = GP_ENT.sp_sel_API_EvolutivoMesCliente(REQUEST.Empid,
                                                                        REQUEST.Mes,
                                                                        REQUEST.Ejercicio,
                                                                        REQUEST.Limit,
                                                                        REQUEST.Rowset).ToList();

                //Si retorna resultados carga JSON de retorno -----
                if (EVOLUTIVO_MESES.Count > 0)
                {
                    RESPONSE.Count = EVOLUTIVO_MESES.Count;
                    RESPONSE.Resultado = "OK";

                    foreach (var Evolutivo_Mes in EVOLUTIVO_MESES)
                    {
                        //Inicializa variable de Movimiento -------------
                        RESPONSE_ITEMS = new API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM();

                        RESPONSE_ITEMS.Id = int.Parse(Evolutivo_Mes.Id.ToString());
                        RESPONSE_ITEMS.EmpId = int.Parse(Evolutivo_Mes.EmpId.ToString());
                        RESPONSE_ITEMS.RazonSocial = Evolutivo_Mes.RazonSocial.Trim();
                        RESPONSE_ITEMS.Fecha = DateTime.Parse(Evolutivo_Mes.Fecha.ToString()).ToString("dd-MM-yyyy"); // "30-05-2022";
                        RESPONSE_ITEMS.Piso = int.Parse(Evolutivo_Mes.Piso.ToString());
                        RESPONSE_ITEMS.Estantes = int.Parse(Evolutivo_Mes.Estantes.ToString());
                        RESPONSE_ITEMS.Rack = int.Parse(Evolutivo_Mes.Rack.ToString());
                        RESPONSE_ITEMS.Total = int.Parse(Evolutivo_Mes.Total.ToString());

                        RESPONSE.Items.Add(RESPONSE_ITEMS);
                    }
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); //REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Count = 0;
                RESPONSE.Resultado = "ERROR - " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;

                //Limpia lista de articulos que estaba creando para no retornar un listado incompleto ---
                RESPONSE.Items = new List<API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM>();

                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
        }
        #endregion

        //recurso 43 Genera Consumo, tambien genera AJUSTES (Cuando NombreProceso = "AJUSTE" --------
        #region Genera Consumo (43), tambien genera AJUSTES 
        [HttpPost]
        [Route("MOVIMIENTOS/GENERACONSUMO")]
        public IHttpActionResult recurso43([FromBody] API_REQUEST_TYPE_43_GeneraConsumo REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "MOVIMIENTOS/GENERACONSUMO";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                if (REQUEST.TipoTransaccion <= 0) { throw new Exception("Debe informar Tipo Transaccion > 0"); } //requerido
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.CodigoBodega <= 0) { throw new Exception("Debe informar Bodega > 0"); } //requerido
                if (REQUEST.TipoReferencia == null || REQUEST.TipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.NumeroReferencia == null || REQUEST.NumeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                REQUEST.Glosa = (REQUEST.Glosa == null ? "" : REQUEST.Glosa); //opcional

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.Linea <= 0) { throw new Exception("Debe informar Linea > 0"); } //requerido
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadMedida == null || item.UnidadMedida == "") { throw new Exception("Debe informar Unidad Medida"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    item.Lote = item.Lote == null ? "" : item.Lote; //opcional
                    item.FechaVencimiento = (item.FechaVencimiento == null ? "" : item.FechaVencimiento); //opcional
                    //if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //opcional
                    item.CodigoUbicacion = (item.CodigoUbicacion == null ? "" : item.CodigoUbicacion); //opcional

                    //REQUEST.items.Count --> cantidad items que trae el json 
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaProceso = new DateTime(); //FechaProceso
            DateTime FechaVencimiento = new DateTime(); //fechaVectoLote

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA DATOS CABECERA JSON -----
                FechaProceso = ValidaCampoFecha(REQUEST.FechaProceso, "FechaProceso");

                //VALIDA DATOS ITEMS DETALLE JSON
                foreach (var item in REQUEST.Items.ToList())
                {
                    FechaVencimiento = ValidaCampoFecha(item.FechaVencimiento, "FechaVencimiento");
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "";

            if (REQUEST.NombreProceso == null || REQUEST.NombreProceso == "") 
            {
                Proceso = "INT-MOV-CONSUMO";
            } 
            else
            {
                Proceso = REQUEST.NombreProceso;
            }


            string Archivo = "MOVIMIENTO_CONSUMO_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + REQUEST.TipoTransaccion.ToString();
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            USERNAME = "INTEGRADOR_API";

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                //carga variables fecha item detalle (variables fechas cabecera quedaron cargadas al validar formato) -----------------
                FechaVencimiento = ValidaCampoFecha(item2.FechaVencimiento, "FechaVencimiento");

                //Inserta linea de cabecera/detalle a tabla temporal ----------------------
                //Se llama a la funcion INSERTA_sp_in_API_Integraciones
                //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
                //  - La funcion Permite enviar los textos como parametros opcionales

                INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                                Archivo,
                                                                USERNAME,
                                                                Fecha,
                                                                Linea,
                                                                Proceso.Trim(),
                                                                REQUEST.Empid.ToString(),
                                                                REQUEST.TipoTransaccion.ToString(),
                                                                FechaProceso.ToString("yyyyMMdd"),
                                                                REQUEST.CodigoBodega.ToString(),
                                                                REQUEST.TipoReferencia,
                                                                REQUEST.NumeroReferencia,
                                                                REQUEST.Glosa,
                                                                item2.Linea.ToString(),
                                                                item2.CodigoArticulo,
                                                                item2.UnidadMedida,
                                                                item2.Cantidad.ToString(),
                                                                item2.Lote,
                                                                FechaVencimiento.ToString("yyyyMMdd"),
                                                                item2.Estado.ToString(),
                                                                item2.CodigoUbicacion.Trim()
                                                                ).ToList();
                if (INTEGRACIONES.Count > 0)
                {
                    if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + INTEGRACIONES[0].Descripcion;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

            } //fin ciclo items del JSON

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      USERNAME,
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumeroReferencia,
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 44 Consulta Bulto Roboris -----------------
        #region Consulta Bulto Roboris (44)
        [HttpGet]
        [HttpPut]
        [Route("ROBORIS/CONSULTABULTO")]
        public IHttpActionResult recurso44(string NumeroBulto)
        {
            ERROR = new API_RESPONSE_ERRORS();

            #region RESPONSE OK

            List<sp_sel_API_ConsultaBultoRoboris_Result> CONSULTA = new List<sp_sel_API_ConsultaBultoRoboris_Result>();
            int Respuesta = 0;

            CONSULTA = GP_ENT.sp_sel_API_ConsultaBultoRoboris(NumeroBulto).ToList();

            if (CONSULTA.Count > 0)
            {
                Respuesta = CONSULTA[0].Respuesta;
            }

            return new HttpActionResult(Request, HttpStatusCode.OK, Respuesta);
            #endregion

        }
        #endregion

        //recurso 45 Creacion de Recetas --------
        #region Creacion de Recetas (45)
        [HttpPost]
        [Route("RECETA/CREARJSON")]
        public IHttpActionResult recurso45([FromBody] API_REQUEST_TYPE_45_CreaReceta REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "RECETA/CREARJSON";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + "La estructura JSON viene vacia",
                        true,
                        true,
                        "",
                        "");

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + "Problemas en el formato del JSON enviado. " + MensajeError.Trim(),
                        true,
                        true,
                        "",
                        "");

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                if (REQUEST.NombreReceta == null || REQUEST.NombreReceta == "") { throw new Exception("Debe informar nombre receta"); } //requerido
                if (REQUEST.TipoProduccion <= 0) { throw new Exception("Debe informar Tipo Produccion > 0"); } //requerido
                if (REQUEST.CodigoArticulo == null || REQUEST.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo de la receta"); } //requerido
                if (REQUEST.UnidadMedida == null || REQUEST.UnidadMedida == "") { throw new Exception("Debe informar Unidad Medida articulo de la receta"); } //requerido
                if (REQUEST.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                //Kilos --> opcional
                if (REQUEST.Version == null || REQUEST.Version == "") { throw new Exception("Debe informar Version de la receta"); } //requerido
                if (REQUEST.VigenciaDesde == null || REQUEST.VigenciaDesde == "") { throw new Exception("Debe informar Fecha Vigencia Desde"); } //requerido
                if (REQUEST.VigenciaHasta == null || REQUEST.VigenciaHasta == "") { throw new Exception("Debe informar Fecha Vigencia Hasta"); } //requerido 
                //BodegaDestino --> opcional 
                if (REQUEST.UbicacionDestino == null || REQUEST.UbicacionDestino == "") { throw new Exception("Debe informar Ubicacion Destino"); } //requerido

                REQUEST.Fecha1 = (REQUEST.Fecha1 == null ? "" : REQUEST.Fecha1); //opcional
                REQUEST.Fecha2 = (REQUEST.Fecha2 == null ? "" : REQUEST.Fecha2); //opcional
                REQUEST.Fecha3 = (REQUEST.Fecha3 == null ? "" : REQUEST.Fecha3); //opcional
                //Valor1 --> opcional
                //Valor2 --> opcional
                //Valor3 --> opcional

                REQUEST.Dato1 = (REQUEST.Dato1 == null ? "" : REQUEST.Dato1); //opcional
                REQUEST.Dato2 = (REQUEST.Dato2 == null ? "" : REQUEST.Dato2); //opcional
                REQUEST.Dato3 = (REQUEST.Dato3 == null ? "" : REQUEST.Dato3); //opcional

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    if (item.Linea <= 0) { throw new Exception("Debe informar Linea > 0"); } //requerido
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadMedida == null || item.UnidadMedida == "") { throw new Exception("Debe informar Unidad Medida"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0"); } //requerido
                    //BodegaOrigen --> opcional 
                    //IndPermiteReemplazo --> opcional 
                    //IndTienePicking --> opcional 

                    item.Dato1 = (item.Dato1 == null ? "" : item.Dato1); //opcional
                    item.Dato2 = (item.Dato2 == null ? "" : item.Dato2); //opcional
                    item.Dato3 = (item.Dato3 == null ? "" : item.Dato3); //opcional
                    item.Fecha1 = (item.Fecha1 == null ? "" : item.Fecha1); //opcional
                    item.Fecha2 = (item.Fecha2 == null ? "" : item.Fecha2); //opcional
                    item.Fecha3 = (item.Fecha3 == null ? "" : item.Fecha3); //opcional
                    //Valor1 --> opcional
                    //Valor2 --> opcional
                    //Valor3 --> opcional

                    //REQUEST.items.Count --> cantidad items que trae el json 
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime VigenciaDesde = new DateTime(); //VigenciaDesde
            DateTime VigenciaHasta = new DateTime(); //VigenciaHasta
            DateTime Fecha1 = new DateTime(); //Fecha1
            DateTime Fecha2 = new DateTime(); //Fecha2
            DateTime Fecha3 = new DateTime(); //Fecha3
            DateTime Fecha1Det = new DateTime(); //Fecha1 detalle
            DateTime Fecha2Det = new DateTime(); //Fecha2 detalle
            DateTime Fecha3Det = new DateTime(); //Fecha3 detalle

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA FECHAS CABECERA JSON -----
                VigenciaDesde = ValidaCampoFecha(REQUEST.VigenciaDesde, "VigenciaDesde");
                VigenciaHasta = ValidaCampoFecha(REQUEST.VigenciaHasta, "VigenciaHasta");
                Fecha1 = ValidaCampoFecha(REQUEST.Fecha1, "Fecha1");
                Fecha2 = ValidaCampoFecha(REQUEST.Fecha2, "Fecha2");
                Fecha3 = ValidaCampoFecha(REQUEST.Fecha3, "Fecha3");

                //VALIDA FECHAS ITEMS DETALLE JSON -----
                foreach (var item in REQUEST.Items.ToList())
                {
                    Fecha1Det = ValidaCampoFecha(item.Fecha1, "Fecha1 detalle");
                    Fecha2Det = ValidaCampoFecha(item.Fecha2, "Fecha2 detalle");
                    Fecha3Det = ValidaCampoFecha(item.Fecha3, "Fecha3 detalle");

                } //FIN ciclo items
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-RECETA-CREAR";
            string Archivo = "RECETA_CREAR_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                //carga variables fecha item detalle (variables fechas cabecera quedaron cargadas al validar formato) -----------------
                Fecha1Det = ValidaCampoFecha(item2.Fecha1, "Fecha1 detalle");
                Fecha2Det = ValidaCampoFecha(item2.Fecha2, "Fecha2 detalle");
                Fecha3Det = ValidaCampoFecha(item2.Fecha3, "Fecha3 detalle");

                //Inserta linea de cabecera/detalle a tabla temporal ----------------------
                //Se llama a la funcion INSERTA_sp_in_API_Integraciones
                //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
                //  - La funcion Permite enviar los textos como parametros opcionales
                //  - En el procedimiento indicar la cantidad de textos que envia la integracion para optimzar la inserción 

                INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                                Archivo,
                                                                USERNAME,
                                                                Fecha,
                                                                Linea,
                                                                Proceso.Trim(),
                                                                REQUEST.Empid.ToString(),
                                                                REQUEST.NombreReceta.Trim(),
                                                                REQUEST.TipoProduccion.ToString(),
                                                                REQUEST.CodigoArticulo.Trim(),
                                                                REQUEST.UnidadMedida.Trim(),
                                                                REQUEST.Cantidad.ToString(),
                                                                REQUEST.Kilos.ToString(),
                                                                REQUEST.Version.ToString(),
                                                                VigenciaDesde.ToString("yyyyMMdd"),
                                                                VigenciaHasta.ToString("yyyyMMdd"),
                                                                REQUEST.BodegaDestino.ToString(),
                                                                REQUEST.UbicacionDestino.Trim(),
                                                                Fecha1.ToString("yyyyMMdd"),
                                                                Fecha2.ToString("yyyyMMdd"),
                                                                Fecha3.ToString("yyyyMMdd"),
                                                                REQUEST.Valor1.ToString(),
                                                                REQUEST.Valor2.ToString(),
                                                                REQUEST.Valor3.ToString(),
                                                                REQUEST.Dato1.Trim(),
                                                                REQUEST.Dato2.Trim(),
                                                                REQUEST.Dato3.Trim(),
                                                                item2.Linea.ToString(),
                                                                item2.CodigoArticulo.Trim(),
                                                                item2.UnidadMedida.Trim(),
                                                                item2.Cantidad.ToString(),
                                                                item2.Estado.ToString(),
                                                                item2.BodegaOrigen.ToString(),
                                                                item2.IndPermiteReemplazo.ToString(),
                                                                item2.IndTienePicking.ToString(),
                                                                item2.Dato1.Trim(),
                                                                item2.Dato2.Trim(),
                                                                item2.Dato3.Trim(),
                                                                Fecha1Det.ToString("yyyyMMdd"),
                                                                Fecha2Det.ToString("yyyyMMdd"),
                                                                Fecha3Det.ToString("yyyyMMdd"),
                                                                item2.Valor1.ToString(),
                                                                item2.Valor2.ToString(),
                                                                item2.Valor3.ToString()
                                                                ).ToList();
                if (INTEGRACIONES.Count > 0)
                {
                    if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = "Error procesando receta: " + REQUEST.NombreReceta.Trim() + ", Linea " + item2.Linea + ", " + INTEGRACIONES[0].Descripcion;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }

            } //fin ciclo items del JSON

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      USERNAME,
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NombreReceta.Trim(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 46 Confirma Cierre de Inventario --------
        #region Confirma Cierre de Inventario (46)
        [HttpPost]
        [Route("INVENTARIO/CONFIRMACIERRE")]
        public IHttpActionResult recurso46([FromBody] API_REQUEST_TYPE_46_ConfirmaCierreInventario REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "INVENTARIO/CONFIRMACIERRE";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                if (REQUEST.NumInventario <= 0) { throw new Exception("Debe informar Numero Inventario > 0"); } //requerido
                if (REQUEST.FechaProceso == null || REQUEST.FechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.Usuario == null || REQUEST.Usuario == "") { throw new Exception("Debe informar Usuario"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-CONFIRMA-INVENTARIO";

            string Archivo = "CONFIRMA_INVENTARIO_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.NumInventario.ToString(),
                                                            REQUEST.FechaProceso.Trim(),
                                                            REQUEST.Usuario.Trim()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Inventario: " + REQUEST.NumInventario.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      USERNAME,
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.NumInventario.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 47 Cambio Estado RDM --------
        #region Cambio Estado RDM (47)
        [HttpPost]
        [Route("RECEPCION/CAMBIOESTADORDM")]
        public IHttpActionResult recurso47([FromBody] API_REQUEST_TYPE_47_CambioEstadoRDM REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "RECEPCION/CAMBIOESTADORDM";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                if (REQUEST.RecepcionId <= 0) { throw new Exception("Debe informar una Recepcion > 0"); } //requerido
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional
                if (REQUEST.Estado <= 0) { throw new Exception("Debe informar un Estado");    } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-CAMBIOESTADO-RDM";

            string Archivo = "CAMBIOESTADO_RDM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.RecepcionId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            REQUEST.Estado.ToString()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Recepcion: " + REQUEST.RecepcionId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.RecepcionId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 48 Actualiza Fecha Entrega SDD --------
        #region Actualiza Fecha Entrega SDD (48)
        [HttpPost]
        [Route("SOLDESP/ACTFECENTREGA")]
        public IHttpActionResult recurso48([FromBody] API_REQUEST_TYPE_48_ActualizaFechaEntregaSDD REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/ACTFECENTREGA";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.SolDespId <= 0) { throw new Exception("Debe informar Id de Solicitud de Despacho > 0"); } //opcional
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional
                if (REQUEST.FechaEntrega == null || REQUEST.FechaEntrega == "") { throw new Exception("Debe informar nueva Fecha Entrega"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaEntrega = new DateTime(); //FechaEntrega

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA FECHAS CABECERA JSON -----
                FechaEntrega = ValidaCampoFecha(REQUEST.FechaEntrega, "FechaEntrega");
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-ACT-FEC-ENTREGA";

            string Archivo = "ACT_FEC_ENTREGA_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.SolDespId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            FechaEntrega.ToString("yyyyMMdd")
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Solicitud: " + REQUEST.SolDespId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolDespId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 49 Guarda datos L_Forecast_ProduccionNB --------
        #region Guarda datos L_Forecast_ProduccionNB (49)
        [HttpPost]
        [Route("FORECASTPRODUCCIONNB")]
        public IHttpActionResult recurso49([FromBody] API_REQUEST_TYPE_49_Forecast_ProduccionNB REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "FORECASTPRODUCCIONNB";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario

                //Valida que el json tenga items ---
                if (REQUEST.Items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.Items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.Items)
                {
                    item.CodigoArticulo = (item.CodigoArticulo == null ? "" : item.CodigoArticulo); //opcional
                    //Canal1 
                    //Canal2 
                    //Canal3 
                    //Canal4 
                    item.Periodo = (item.Periodo == null ? "" : item.Periodo); //opcional        
                    item.FechaDesde = (item.FechaDesde == null ? "" : item.FechaDesde); //opcional        
                    item.FechaHasta = (item.FechaHasta == null ? "" : item.FechaHasta); //opcional        
                    item.Usuario = (item.Usuario == null ? "" : item.Usuario); //opcional
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaDesde = new DateTime(); //FechaDesde
            DateTime FechaHasta = new DateTime(); //FechaHasta

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA FECHAS CABECERA JSON -----

                //VALIDA FECHAS ITEMS DETALLE JSON -----
                foreach (var item in REQUEST.Items.ToList())
                {
                    FechaDesde = ValidaCampoFecha(item.FechaDesde, "FechaDesde");
                    FechaHasta = ValidaCampoFecha(item.FechaHasta, "FechaHasta");

                } //FIN ciclo items
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-FORECAST-PRD-NB";

            string Archivo = "FORECAST_PRD_NB_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //recorre items json
            foreach (var item2 in REQUEST.Items.ToList())
            {
                //Inserta linea de cabecera/detalle a tabla temporal ----------------------
                //Se llama a la funcion INSERTA_sp_in_API_Integraciones
                //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
                //  - La funcion Permite enviar los textos como parametros opcionales

                INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                                Archivo,
                                                                USERNAME,
                                                                Fecha,
                                                                Linea,
                                                                Proceso.Trim(),
                                                                REQUEST.Empid.ToString(),
                                                                item2.CodigoArticulo.Trim(),
                                                                item2.Canal1.ToString(),
                                                                item2.Canal2.ToString(),
                                                                item2.Canal3.ToString(),
                                                                item2.Canal4.ToString(),
                                                                item2.Periodo.Trim(),
                                                                FechaDesde.ToString("yyyyMMdd"),
                                                                FechaHasta.ToString("yyyyMMdd"),
                                                                item2.Usuario.Trim()
                                                                ).ToList();
                if (INTEGRACIONES.Count > 0)
                {
                    if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = "Error procesando Forecast: " + item2.CodigoArticulo.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            } //FIN ciclo recorreo Items

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //Webhook Estandar que inserta en tabla de Webhook (copia de metodo llamado WebhookBSale)
        #region WebhookGP (50)
        [HttpPost]
        [HttpGet]
        [Route("WEBHOOKGP")]
        public IHttpActionResult recurso50(object data)
        {

            API_RESPONSE_TYPE_17 RESPONSE = new API_RESPONSE_TYPE_17();
            ERROR = new API_RESPONSE_ERRORS();

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            #region PROCESO
            STATUS_CODE = HttpStatusCode.OK;
            try
            {
                List<sp_in_API_Webhook_Bsale_Result> WBSL = GP_ENT.sp_in_API_Webhook_Bsale(data.ToString()).ToList();

                RESPONSE.Resultado = WBSL.ElementAt(0).Resultado;
                RESPONSE.Descripcion = WBSL.ElementAt(0).Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje + ". " + ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.InternalServerError;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            #endregion
        }
        #endregion

        //recurso 51 Adjunta Archivo a la SDR en base 64 --------
        #region Adjunta Archivo a la SDR en base 64 (51)
        [HttpGet]
        [HttpPost]
        [Route("SOLRECEP/ADJUNTARARCHIVO")]
        public IHttpActionResult recurso51([FromBody] API_REQUEST_TYPE_51_AdjuntaArchivoSDR REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLRECEP/ADJUNTARARCHIVO";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----
                                
                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.SolRecepId <= 0) { throw new Exception("Debe informar una Solicitud de Recepcion > 0"); } //opcional
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional

                if (REQUEST.SolRecepId == 0 && REQUEST.TipoReferencia == "" && REQUEST.NumeroReferencia == "")
                { 
                    throw new Exception("Debe informar Solicitud de Recepcion o Tipo y Numero referencia"); 
                } 

                if (REQUEST.ArchivoBase64 == null || REQUEST.ArchivoBase64 == "") { throw new Exception("Debe informar Archivo en formato base64"); } //requerido
                if (REQUEST.NombreArchivo == null || REQUEST.NombreArchivo == "") { throw new Exception("Debe informar nombre del archivo y su extension"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            try
            { 
                //Guarda archivo base64 como documento en el servidor
                string RutaArchivo = ConfigurationManager.AppSettings["RutaDoctosAdjuntos"].ToString() + @"\SDR\" + REQUEST.SolRecepId.ToString();

                if (Directory.Exists(RutaArchivo) == false)
                {
                    Directory.CreateDirectory(RutaArchivo);
                }

                string base64BinaryStr = REQUEST.ArchivoBase64.Trim();
                byte[] bytes = Convert.FromBase64String(base64BinaryStr);

                //crea el archivo
                File.WriteAllBytes(RutaArchivo + @"\" + REQUEST.NombreArchivo.Trim(), bytes);
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolRecepId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-ADJUNTAR-ARCHIVO-SDR";

            string Archivo = "ADJUNTAR_ARCHIVO_SDR_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.SolRecepId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            @"uploads/SDR/" + REQUEST.SolRecepId.ToString() + @"/" + REQUEST.NombreArchivo.Trim(),
                                                            REQUEST.NombreArchivo.Trim()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando SDR: " + REQUEST.SolRecepId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolRecepId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 52 Adjunta Archivo a la SDD en Base64 --------
        #region Adjunta Archivo a la SDD en Base64 (52)
        [HttpGet]
        [HttpPost]
        [Route("SOLDESP/ADJUNTARARCHIVO")]
        public IHttpActionResult recurso52([FromBody] API_REQUEST_TYPE_52_AdjuntaArchivoSDD REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/ADJUNTARARCHIVO";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.SolDespId <= 0) { throw new Exception("Debe informar una Solicitud de Despacho > 0"); } //opcional
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional

                if (REQUEST.SolDespId == 0 && REQUEST.TipoReferencia == "" && REQUEST.NumeroReferencia == "")
                {
                    throw new Exception("Debe informar Solicitud de Despacho o Tipo y Numero referencia");
                }

                if (REQUEST.ArchivoBase64 == null || REQUEST.ArchivoBase64 == "") { throw new Exception("Debe informar Archivo en formato base64"); } //requerido
                if (REQUEST.NombreArchivo == null || REQUEST.NombreArchivo == "") { throw new Exception("Debe informar nombre del archivo y su extension"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            try
            {
                //Guarda archivo base64 como documento en el servidor
                string RutaArchivo = ConfigurationManager.AppSettings["RutaDoctosAdjuntos"].ToString() + @"\SDD\" + REQUEST.SolDespId.ToString();

                if (Directory.Exists(RutaArchivo) == false)
                {
                    Directory.CreateDirectory(RutaArchivo);
                }

                string base64BinaryStr = REQUEST.ArchivoBase64.Trim();
                byte[] bytes = Convert.FromBase64String(base64BinaryStr);

                //crea el archivo
                File.WriteAllBytes(RutaArchivo + @"\" + REQUEST.NombreArchivo.Trim(), bytes);
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolDespId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-ADJUNTAR-ARCHIVO-SDD";

            string Archivo = "ADJUNTAR_ARCHIVO_SDD_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.SolDespId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            @"SDD/" + REQUEST.SolDespId.ToString() + @"/" + REQUEST.NombreArchivo.Trim(),
                                                            REQUEST.NombreArchivo.Trim()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando SDR: " + REQUEST.SolDespId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolDespId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 53 Anula Solicitud Traslado --------
        #region Anula Solicitud Traslado (53)
        [HttpPost]
        [Route("SOLDESP/ANULASOLTRASLADO")]
        public IHttpActionResult recurso53([FromBody] API_REQUEST_TYPE_53_AnulaSolTraslado REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/ANULASOLTRASLADO";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.SolDespId <= 0) { throw new Exception("Debe informar Id de Solicitud de Despacho > 0"); } //requerido
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional

                if (REQUEST.SolDespId == 0 && REQUEST.TipoReferencia == "" && REQUEST.NumeroReferencia == "")
                {
                    throw new Exception("Debe informar Solicitud de Despacho o Tipo y Numero referencia");
                }

                REQUEST.FechaEntrega = (REQUEST.FechaEntrega == null ? "" : REQUEST.FechaEntrega); //opcional
                if (REQUEST.Motivo <= 0) { throw new Exception("Debe informar motivo anula traslado"); } //requerido
                if (REQUEST.GlosaAnula == null || REQUEST.GlosaAnula == "") { throw new Exception("Debe informar glosa anulacion"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaEntrega = new DateTime(); //FechaEntrega

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA FECHAS CABECERA JSON -----
                FechaEntrega = ValidaCampoFecha(REQUEST.FechaEntrega, "FechaEntrega");
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-ANULA-SOL-TRAS";

            string Archivo = "ANULA_SOL_TRAS_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.SolDespId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            FechaEntrega.ToString("yyyyMMdd"),
                                                            REQUEST.Motivo.ToString(),
                                                            REQUEST.GlosaAnula.Trim()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Solicitud: " + REQUEST.SolDespId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolDespId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 54 Anula Solicitud Despacho --------
        #region Anula Solicitud Despacho (54)
        [HttpPost]
        [Route("SOLDESP/ANULASOLDESPACHO")]
        public IHttpActionResult recurso54([FromBody] API_REQUEST_TYPE_54_AnulaSolDespacho REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "SOLDESP/ANULASOLDESPACHO";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.SolDespId <= 0) { throw new Exception("Debe informar Id de Solicitud de Despacho > 0"); } //opcional
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional

                if (REQUEST.SolDespId == 0 && REQUEST.TipoReferencia == "" && REQUEST.NumeroReferencia == "")
                {
                    throw new Exception("Debe informar Solicitud de Despacho o Tipo y Numero referencia");
                }

                REQUEST.FechaEntrega = (REQUEST.FechaEntrega == null ? "" : REQUEST.FechaEntrega); //opcional
                if (REQUEST.Motivo <= 0) { throw new Exception("Debe informar motivo anula traslado"); } //requerido
                if (REQUEST.GlosaAnula == null || REQUEST.GlosaAnula == "") { throw new Exception("Debe informar glosa anulacion"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            #region VALIDA FORMATOS DE FECHAS

            DateTime FechaEntrega = new DateTime(); //FechaEntrega

            try
            {
                //valida las fechas pasando el contenido string a una variable fecha, si se cae es que la fecha no es valida ----

                //VALIDA FECHAS CABECERA JSON -----
                FechaEntrega = ValidaCampoFecha(REQUEST.FechaEntrega, "FechaEntrega");
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016); //REQUEST - VALIDACION CONSISTENCIA DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim(); //mensaje error concreto
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje; //Descripcion error generico
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-ANULA-SOL-DESP";

            string Archivo = "ANULA_SOL_DESP_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.SolDespId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            FechaEntrega.ToString("yyyyMMdd"),
                                                            REQUEST.Motivo.ToString(),
                                                            REQUEST.GlosaAnula.Trim()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Solicitud: " + REQUEST.SolDespId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.SolDespId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //Webhook que recibe info de "Webhook Rutas" de DRIVIN 
        #region Webhook Ruta DRIVIN (55)
        [HttpPost]
        [HttpGet]
        [Route("WEBHOOK/DRIVINRUTA")]
        public IHttpActionResult recurso55([FromBody] API_REQUEST_TYPE_55_WebhookRutaDRIVIN REQUEST)
        {

            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "WEBHOOK/DRIVINRUTA";

            STATUS_CODE = HttpStatusCode.OK; //Por defecto status OK

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    false,
                    "",
                    body.ToString());

            //===============================================================================================================
            //===============================================================================================================
            //===============================================================================================================

            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-WEBHOOK-DRIVINRUTA";
            string Archivo = "WEBHOOK_DRIVINRUTA_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //recorre estructura json del Webhook, insertamos campos importantes

            if (REQUEST.trips != null)
            {
                foreach (var trips in REQUEST.trips.ToList())
                {
                    if (trips.results != null)
                    {
                        foreach (var results in trips.results.ToList())
                        {
                            if (results.orders != null)
                            {
                                foreach (var orders in results.orders.ToList())
                                {
                                    //Inserta linea de cabecera/detalle a tabla temporal ----------------------
                                    //Se llama a la funcion INSERTA_sp_in_API_Integraciones
                                    //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
                                    //  - La funcion Permite enviar los textos como parametros opcionales
                                    //  - En el procedimiento indicar la cantidad de textos que envia la integracion para optimizar la inserción 

                                    //controla nulos
                                    REQUEST.vehicle = (REQUEST.vehicle == null ? "" : REQUEST.vehicle);
                                    REQUEST.vehicle_detail = (REQUEST.vehicle_detail == null ? "" : REQUEST.vehicle_detail);
                                    //REQUEST.route_id = (REQUEST.route_id == null ? "" : REQUEST.route_id);
                                    REQUEST.route_code = (REQUEST.route_code == null ? "" : REQUEST.route_code);
                                    REQUEST.description = (REQUEST.description == null ? "" : REQUEST.description);
                                    REQUEST.deploy_date = (REQUEST.deploy_date == null ? "" : REQUEST.deploy_date);
                                    REQUEST.supplier_code = (REQUEST.supplier_code == null ? "" : REQUEST.supplier_code);
                                    REQUEST.scenario_token = (REQUEST.scenario_token == null ? "" : REQUEST.scenario_token);
                                    REQUEST.schema_code = (REQUEST.schema_code == null ? "" : REQUEST.schema_code);
                                    REQUEST.schema_name = (REQUEST.schema_name == null ? "" : REQUEST.schema_name);
                                    REQUEST.approved_at = (REQUEST.approved_at == null ? "" : REQUEST.approved_at);
                                    REQUEST.platform = (REQUEST.platform == null ? "" : REQUEST.platform);
                                    REQUEST.started_at = (REQUEST.started_at == null ? "" : REQUEST.started_at);
                                    REQUEST.fleet_sequence = (REQUEST.fleet_sequence == null ? "" : REQUEST.fleet_sequence);
                                    REQUEST.login_url = (REQUEST.login_url == null ? "" : REQUEST.login_url);
                                    REQUEST.driver.email = (REQUEST.driver.email == null ? "" : REQUEST.driver.email);
                                    REQUEST.driver.full_name = (REQUEST.driver.full_name == null ? "" : REQUEST.driver.full_name);
                                    REQUEST.driver.phone = (REQUEST.driver.phone == null ? "" : REQUEST.driver.phone);
                                    //REQUEST.summary.total_trips = (REQUEST.summary.total_trips == null ? "" : REQUEST.summary.total_trips);
                                    //REQUEST.summary.total_orders = (REQUEST.summary.total_orders == null ? "" : REQUEST.summary.total_orders);
                                    //REQUEST.summary.total_addresses = (REQUEST.summary.total_addresses == null ? "" : REQUEST.summary.total_addresses);
                                    //REQUEST.summary.total_distance = (REQUEST.summary.total_distance == null ? "" : REQUEST.summary.total_distance);
                                    //REQUEST.summary.total_time = (REQUEST.summary.total_time == null ? "" : REQUEST.summary.total_time);
                                    //trips.summary.trip_number = (trips.summary.trip_number == null ? "" : trips.summary.trip_number);
                                    //trips.summary.total_orders = (trips.summary.total_orders == null ? "" : trips.summary.total_orders);
                                    //trips.summary.total_addresses = (trips.summary.total_addresses == null ? "" : trips.summary.total_addresses);
                                    //trips.summary.total_distance = (trips.summary.total_distance == null ? "" : trips.summary.total_distance);
                                    //trips.summary.total_time = (trips.summary.total_time == null ? "" : trips.summary.total_time);
                                    results.deposit = (results.deposit == null ? "" : results.deposit);
                                    //results.position = (results.position == null ? "" : results.position);
                                    //results.eta_mode = (results.eta_mode == null ? "" : results.eta_mode);
                                    results.eta = (results.eta == null ? "" : results.eta);
                                    results.eta_approved = (results.eta_approved == null ? "" : results.eta_approved);
                                    results.eta_started = (results.eta_started == null ? "" : results.eta_started);
                                    results.stop.code = (results.stop.code == null ? "" : results.stop.code);
                                    results.stop.name = (results.stop.name == null ? "" : results.stop.name);
                                    results.stop.client = (results.stop.client == null ? "" : results.stop.client);
                                    results.stop.address_type = (results.stop.address_type == null ? "" : results.stop.address_type);
                                    results.stop.address = (results.stop.address == null ? "" : results.stop.address);
                                    results.stop.reference = (results.stop.reference == null ? "" : results.stop.reference);
                                    results.stop.city = (results.stop.city == null ? "" : results.stop.city);
                                    results.stop.country = (results.stop.country == null ? "" : results.stop.country);
                                    results.stop.lat = (results.stop.lat == null ? "" : results.stop.lat);
                                    results.stop.lng = (results.stop.lng == null ? "" : results.stop.lng);
                                    results.stop.postal_code = (results.stop.postal_code == null ? "" : results.stop.postal_code);
                                    //results.stop.service_time = (results.stop.service_time == null ? "" : results.stop.service_time);
                                    //results.stop.priority = (results.stop.priority == null ? "" : results.stop.priority);
                                    orders.idx = (orders.idx == null ? "" : orders.idx);
                                    orders.code = (orders.code == null ? "" : orders.code);
                                    orders.alt_code = (orders.alt_code == null ? "" : orders.alt_code);
                                    orders.delivery_date = (orders.delivery_date == null ? "" : orders.delivery_date);
                                    orders.supplier_code = (orders.supplier_code == null ? "" : orders.supplier_code);
                                    orders.supplier_name = (orders.supplier_name == null ? "" : orders.supplier_name);
                                    orders.client_code = (orders.client_code == null ? "" : orders.client_code);
                                    orders.client_name = (orders.client_name == null ? "" : orders.client_name);
                                    //orders.units_1 = (orders.units_1 == null ? "" : orders.units_1);
                                    //orders.units_2 = (orders.units_2 == null ? "" : orders.units_2);
                                    //orders.units_3 = (orders.units_3 == null ? "" : orders.units_3);
                                    orders.custom_1 = (orders.custom_1 == null ? "" : orders.custom_1);
                                    orders.custom_2 = (orders.custom_2 == null ? "" : orders.custom_2);
                                    orders.custom_3 = (orders.custom_3 == null ? "" : orders.custom_3);
                                    orders.custom_4 = (orders.custom_4 == null ? "" : orders.custom_4);
                                    orders.custom_5 = (orders.custom_5 == null ? "" : orders.custom_5);
                                    orders.custom_6 = (orders.custom_6 == null ? "" : orders.custom_6);
                                    orders.custom_7 = (orders.custom_7 == null ? "" : orders.custom_7);
                                    orders.custom_8 = (orders.custom_8 == null ? "" : orders.custom_8);
                                    orders.custom_9 = (orders.custom_9 == null ? "" : orders.custom_9);
                                    orders.custom_10 = (orders.custom_10 == null ? "" : orders.custom_10);
                                    orders.custom_11 = (orders.custom_11 == null ? "" : orders.custom_11);
                                    orders.number_1 = (orders.number_1 == null ? "" : orders.number_1);
                                    orders.number_2 = (orders.number_2 == null ? "" : orders.number_2);
                                    orders.number_3 = (orders.number_3 == null ? "" : orders.number_3);
                                    orders.number_4 = (orders.number_4 == null ? "" : orders.number_4);

                                    INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                                                    Archivo,
                                                                                    USERNAME,
                                                                                    Fecha,
                                                                                    Linea,
                                                                                    Proceso.Trim(),
                                                                                    REQUEST.vehicle,
                                                                                    REQUEST.vehicle_detail,
                                                                                    REQUEST.route_id.ToString(),
                                                                                    REQUEST.route_code,
                                                                                    REQUEST.description,
                                                                                    REQUEST.deploy_date,
                                                                                    REQUEST.supplier_code,
                                                                                    REQUEST.scenario_token,
                                                                                    REQUEST.schema_code,
                                                                                    REQUEST.schema_name,
                                                                                    REQUEST.approved_at,
                                                                                    REQUEST.platform,
                                                                                    REQUEST.started_at,
                                                                                    REQUEST.fleet_sequence,
                                                                                    REQUEST.login_url,
                                                                                    REQUEST.driver.email.Trim(),
                                                                                    REQUEST.driver.full_name.Trim(),
                                                                                    REQUEST.driver.phone.Trim(),
                                                                                    REQUEST.summary.total_trips.ToString(),
                                                                                    REQUEST.summary.total_orders.ToString(),
                                                                                    REQUEST.summary.total_addresses.ToString(),
                                                                                    REQUEST.summary.total_distance.ToString(),
                                                                                    REQUEST.summary.total_time.ToString(),
                                                                                    trips.summary.trip_number.ToString(),
                                                                                    trips.summary.total_orders.ToString(),
                                                                                    trips.summary.total_addresses.ToString(),
                                                                                    trips.summary.total_distance.ToString(),
                                                                                    trips.summary.total_time.ToString(),
                                                                                    results.deposit.Trim(),
                                                                                    results.position.ToString(),
                                                                                    results.eta_mode.ToString(),
                                                                                    results.eta,
                                                                                    results.eta_approved,
                                                                                    results.eta_started,
                                                                                    results.stop.code,
                                                                                    results.stop.name,
                                                                                    results.stop.client,
                                                                                    results.stop.address_type,
                                                                                    results.stop.address,
                                                                                    results.stop.reference,
                                                                                    results.stop.city,
                                                                                    results.stop.country,
                                                                                    results.stop.lat,
                                                                                    results.stop.lng,
                                                                                    results.stop.postal_code,
                                                                                    results.stop.service_time.ToString(),
                                                                                    results.stop.priority.ToString(),
                                                                                    orders.idx,
                                                                                    orders.code,
                                                                                    orders.alt_code,
                                                                                    orders.delivery_date,
                                                                                    orders.supplier_code,
                                                                                    orders.supplier_name,
                                                                                    orders.client_code,
                                                                                    orders.client_name,
                                                                                    orders.units_1.ToString(),
                                                                                    orders.units_2.ToString(),
                                                                                    orders.units_3.ToString(),
                                                                                    orders.custom_1,
                                                                                    orders.custom_2,
                                                                                    orders.custom_3,
                                                                                    orders.custom_4,
                                                                                    orders.custom_5,
                                                                                    orders.custom_6,
                                                                                    orders.custom_7,
                                                                                    orders.custom_8,
                                                                                    orders.custom_9,
                                                                                    orders.custom_10,
                                                                                    orders.custom_11,
                                                                                    orders.number_1,
                                                                                    orders.number_2,
                                                                                    orders.number_3,
                                                                                    orders.number_4
                                                                                    ).ToList();
                                    if (INTEGRACIONES.Count > 0)
                                    {
                                        if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                                        {
                                            #region NOK
                                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                                            LogInfo(NombreProceso,
                                                    ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                                                    true,
                                                    false,
                                                    "",
                                                    body.ToString());

                                            RESPONSE.Resultado = "ERROR";
                                            RESPONSE.Descripcion = "Error procesando Webhook";
                                            RESPONSE.Resultado_Codigo = ERROR.ErrID;
                                            RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                                            STATUS_CODE = HttpStatusCode.BadRequest;
                                            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region NOK
                                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                                        LogInfo(NombreProceso,
                                                ERROR.Mensaje.Trim(),
                                                true,
                                                false,
                                                "",
                                                body.ToString());

                                        RESPONSE.Resultado = "ERROR";
                                        RESPONSE.Descripcion = "";
                                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                                        STATUS_CODE = HttpStatusCode.BadRequest;
                                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                                        #endregion
                                    }
                                }
                            }                            
                        }
                    }                    

                } //fin ciclo items del JSON
            }            

            try
            {
                USERNAME = "INTEGRADOR_API";

                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      USERNAME,
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                false,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            false,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        false,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //Webhook que recibe info de "Webhook Tracking" de ENVIAME
        #region Webhook Tracking ENVIAME (56)
        [HttpPost]
        [HttpGet]
        [Route("WEBHOOK/ENVIAMETRACKING")]
        public IHttpActionResult recurso56([FromBody] API_REQUEST_TYPE_56_WebhookTrackingENVIAME REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();

            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "WEBHOOK/ENVIAMETRACKING";

            STATUS_CODE = HttpStatusCode.OK; //Por defecto status OK

            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_10;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "Error";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    false,
                    "",
                    body.ToString());

            //===============================================================================================================
            //===============================================================================================================
            //===============================================================================================================

            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            USERNAME = "INTEGRADOR_API";
            string Proceso = "INT-WEBHOOK-ENVIAMETRACKING";
            string Archivo = "WEBHOOK_ENVIAMETRACKING_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //recorre estructura json del Webhook, insertamos campos importantes
            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales
            //  - En el procedimiento indicar la cantidad de textos que envia la integracion para optimizar la inserción 

            //controla nulos
            REQUEST.identifier = (REQUEST.identifier == null ? "" : REQUEST.identifier); // (número único de Envíame),   
            REQUEST.order_number = (REQUEST.order_number == null ? "" : REQUEST.order_number); // (referencia o número de pedido del seller)",
            REQUEST.tracking_number = (REQUEST.tracking_number == null ? "" : REQUEST.tracking_number); // (orden de transporte proporcionado por el carrier)",
            REQUEST.carrier_tracking_number = (REQUEST.carrier_tracking_number == null ? "" : REQUEST.carrier_tracking_number); // (N/A),
            REQUEST.carrier_name = (REQUEST.carrier_name == null ? "" : REQUEST.carrier_name); // (Nombre del carrier)",
            REQUEST.carrier_code = (REQUEST.carrier_code == null ? "" : REQUEST.carrier_code); // (Código del carrier en Envíame)",
            REQUEST.dead_line_date = (REQUEST.dead_line_date == null ? "" : REQUEST.dead_line_date); // (fecha de compromiso de entrega, si aplica)",
            REQUEST.status_id = (REQUEST.status_id == null ? "" : REQUEST.status_id); // (id interno del estado del envío),
            REQUEST.status_name = (REQUEST.status_name == null ? "" : REQUEST.status_name); // (Nombre del estado en Envíame)",
            REQUEST.status_information = (REQUEST.status_information == null ? "" : REQUEST.status_information); // (Comentario del estado)",
            REQUEST.status_date = (REQUEST.status_date == null ? "" : REQUEST.status_date); // (Fecha del último estado en formato aaaa-mm-dd hh:mm)",
            REQUEST.tracking_url = (REQUEST.tracking_url == null ? "" : REQUEST.tracking_url);
            REQUEST.reference2 = (REQUEST.reference2 == null ? "" : REQUEST.reference2);

            try
            { 
                INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                                Archivo,
                                                                USERNAME,
                                                                Fecha,
                                                                Linea,
                                                                Proceso.Trim(),
                                                                REQUEST.identifier.ToString(),
                                                                REQUEST.order_number.ToString(),
                                                                REQUEST.tracking_number.ToString(),
                                                                REQUEST.carrier_tracking_number.ToString(),
                                                                REQUEST.carrier_name.ToString(),
                                                                REQUEST.carrier_code.ToString(),
                                                                REQUEST.dead_line_date.ToString(),
                                                                REQUEST.status_id.ToString(),
                                                                REQUEST.status_name.ToString(),
                                                                REQUEST.status_information.ToString(),
                                                                REQUEST.status_date.ToString(),
                                                                REQUEST.tracking_url.ToString(),
                                                                REQUEST.reference2.ToString()
                                                                ).ToList();
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        false,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            false,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Webhook";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        false,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      USERNAME,
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                false,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            false,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        false,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //recurso 57 Cambio Estado Articulo Por RDM --------
        #region Cambio Estado Articulo Por RDM (57)
        [HttpPost]
        [Route("RECEPCION/CAMBIOESTADOARTRDM")]
        public IHttpActionResult recurso57([FromBody] API_REQUEST_TYPE_47_CambioEstadoRDM REQUEST)
        {
            API_RESPONSE_TYPE_0 RESPONSE = new API_RESPONSE_TYPE_0();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();
            string NombreProceso = "RECEPCION/CAMBIOESTADOARTRDM";

            //Valida estructura JSON recibido sea valida 
            #region VALIDACIONES JSON RECIBIDO NOK

            //Si no se envia json ---
            if (REQUEST == null)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "La estructura JSON viene vacia";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Guarda JSON recibido en el log ----------
            var body = JsonConvert.SerializeObject(REQUEST);
            LogInfo(NombreProceso,
                    "Estructura JSON: " + body.ToString(),
                    true,
                    true,
                    "",
                    body.ToString());

            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                //Rescata lista de errores del ModelState y lo agrega al mensaje de error 
                List<string> ErroresModelo = ErroresModelo = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception.Message).ToList();
                string MensajeError = "";

                //Si retorna resultados carga JSON de retorno
                if (ErroresModelo.Count > 0)
                {
                    foreach (var ErrorModelo in ErroresModelo)
                    {
                        MensajeError = MensajeError.Trim() + " " + ErrorModelo.ToString().Trim() + ".";
                    }
                }

                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "Problemas en el formato del JSON enviado. " + MensajeError.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            #endregion

            //valida token y retorna usuario
            #region VALIDA ACCESO API/RECURSO NOK
            CONFIGURATION.RESOURCES_ID = (long)CONFIG_RESOURCES.RESO_9;
            if (!ACCESS.RESOLVE_ACCESS(this.Request, CONFIGURATION, out USERNAME, out ERROR))
            {
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
            }
            #endregion

            //Valida Usuario - Empresa
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.Empid, USERNAME, out ERROR))
            {
                #region RESPONSE ERROR
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Setea salida en OK ---------------
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.Count = 1;
            RESPONSE.Resultado = "OK";
            RESPONSE.Descripcion = "";
            RESPONSE.Resultado_Codigo = 0;
            RESPONSE.Resultado_Descripcion = "";

            #region VALIDA CAMPOS REQUERIDOS Y OPCIONALES
            // Consideraciones:
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan en cero, o sea siempre tomará un valor independiente se envie o no
            // - un campo string se puede dejar opcional dandole un valor por defecto 
            // - un campo numerico como traera siempre un valor, es por defecto opcional 

            try
            {
                //Valida datos cabecera json ----

                //REQUEST.Empid // se valida con el usuario
                //if (REQUEST.RecepcionId <= 0) { throw new Exception("Debe informar una Recepcion > 0"); } //requerido
                REQUEST.TipoReferencia = (REQUEST.TipoReferencia == null ? "" : REQUEST.TipoReferencia); //opcional
                REQUEST.NumeroReferencia = (REQUEST.NumeroReferencia == null ? "" : REQUEST.NumeroReferencia); //opcional

                if (REQUEST.RecepcionId == 0 && REQUEST.TipoReferencia == "" && REQUEST.NumeroReferencia == "")
                {
                    throw new Exception("Debe informar id Recepcion o Tipo y Numero referencia");
                }

                if (REQUEST.Estado <= 0) { throw new Exception("Debe informar un Estado"); } //requerido
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }
            #endregion

            //Luego de cumplir las validaciones iniciales realiza el proceso ---
            #region PROCESAMIENTO

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();
            List<Sp_Proc_IntegracionApi_Result> INTEGRACIONES_PROCESA = new List<Sp_Proc_IntegracionApi_Result>();

            string Proceso = "INT-CAMBIOESTADOART-PORRDM";

            string Archivo = "CAMBIOESTADOART_PORRDM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime Fecha = DateTime.Now;
            int Linea = 1;

            //Inserta linea de cabecera/detalle a tabla temporal ----------------------
            //Se llama a la funcion INSERTA_sp_in_API_Integraciones
            //  - Esta funcion llama al procedimiento almacenado sp_in_API_Integraciones, que inserta hasta el Texto100
            //  - La funcion Permite enviar los textos como parametros opcionales

            INTEGRACIONES = INSERTA_sp_in_API_Integraciones(GP_ENT,
                                                            Archivo,
                                                            USERNAME,
                                                            Fecha,
                                                            Linea,
                                                            Proceso.Trim(),
                                                            REQUEST.Empid.ToString(),
                                                            REQUEST.RecepcionId.ToString(),
                                                            REQUEST.TipoReferencia.Trim(),
                                                            REQUEST.NumeroReferencia.Trim(),
                                                            REQUEST.Estado.ToString()
                                                            ).ToList();
            if (INTEGRACIONES.Count > 0)
            {
                if (INTEGRACIONES[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim() + ". " + INTEGRACIONES[0].Descripcion,
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "Error procesando Recepcion: " + REQUEST.RecepcionId.ToString() + ", " + INTEGRACIONES[0].Descripcion;
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim(),
                        true,
                        true,
                        "",
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = "";
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------
                //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                //--------------------------------------------------------------------------------------------------------------------------------
                INTEGRACIONES_PROCESA = GP_ENT.Sp_Proc_IntegracionApi(Archivo,
                                                                      "INTEGRADOR_API",
                                                                      Fecha).ToList();
                if (INTEGRACIONES_PROCESA.Count > 0)
                {
                    if (INTEGRACIONES_PROCESA[0].Generacion == 1)
                    {
                        RESPONSE.Resultado = "OK";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                    }
                    else
                    {
                        #region NOK
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO

                        LogInfo(NombreProceso,
                                ERROR.Mensaje.Trim() + ". " + INTEGRACIONES_PROCESA[0].GlosaEstado,
                                true,
                                true,
                                "",
                                body.ToString());

                        RESPONSE.Resultado = "ERROR";
                        RESPONSE.Descripcion = INTEGRACIONES_PROCESA[0].GlosaEstado;
                        RESPONSE.Resultado_Codigo = ERROR.ErrID;
                        RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                        STATUS_CODE = HttpStatusCode.BadRequest;
                        return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                        #endregion
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO

                    LogInfo(NombreProceso,
                            ERROR.Mensaje.Trim(),
                            true,
                            true,
                            "",
                            body.ToString());

                    RESPONSE.Resultado = "ERROR";
                    RESPONSE.Descripcion = "";
                    RESPONSE.Resultado_Codigo = ERROR.ErrID;
                    RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                    STATUS_CODE = HttpStatusCode.BadRequest;
                    return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR PROCESO BASE DE DATOS

                LogInfo(NombreProceso,
                        ERROR.Mensaje.Trim() + ". " + ex.Message.Trim(),
                        true,
                        true,
                        REQUEST.RecepcionId.ToString(),
                        body.ToString());

                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ex.Message.Trim();
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.BadRequest;
                return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);
                #endregion
            }

            //Si llega hasta aca significa que realizó correctamente el proceso -------------------------------------
            return new HttpActionResult(Request, (ConfigurationManager.AppSettings["InformaStatusAPI"].ToString() == "SI" ? STATUS_CODE : HttpStatusCode.OK), RESPONSE);

            #endregion
        }
        #endregion

        //Inserta en tabla L_Integraciones, los parametros Texto2 al Texto100 son opcionales ---------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------
        public static List<sp_in_API_Integraciones_Result> INSERTA_sp_in_API_Integraciones(GP_ENT GP_ENT, string Archivo, string UserName, DateTime FechaProceso, int Linea, string Texto1, string Texto2 = "", string Texto3 = "", string Texto4 = "", string Texto5 = "", string Texto6 = "", string Texto7 = "", string Texto8 = "", string Texto9 = "", string Texto10 = "", string Texto11 = "", string Texto12 = "", string Texto13 = "", string Texto14 = "", string Texto15 = "", string Texto16 = "", string Texto17 = "", string Texto18 = "", string Texto19 = "", string Texto20 = "", string Texto21 = "", string Texto22 = "", string Texto23 = "", string Texto24 = "", string Texto25 = "", string Texto26 = "", string Texto27 = "", string Texto28 = "", string Texto29 = "", string Texto30 = "", string Texto31 = "", string Texto32 = "", string Texto33 = "", string Texto34 = "", string Texto35 = "", string Texto36 = "", string Texto37 = "", string Texto38 = "", string Texto39 = "", string Texto40 = "", string Texto41 = "", string Texto42 = "", string Texto43 = "", string Texto44 = "", string Texto45 = "", string Texto46 = "", string Texto47 = "", string Texto48 = "", string Texto49 = "", string Texto50 = "", string Texto51 = "", string Texto52 = "", string Texto53 = "", string Texto54 = "", string Texto55 = "", string Texto56 = "", string Texto57 = "", string Texto58 = "", string Texto59 = "", string Texto60 = "", string Texto61 = "", string Texto62 = "", string Texto63 = "", string Texto64 = "", string Texto65 = "", string Texto66 = "", string Texto67 = "", string Texto68 = "", string Texto69 = "", string Texto70 = "", string Texto71 = "", string Texto72 = "", string Texto73 = "", string Texto74 = "", string Texto75 = "", string Texto76 = "", string Texto77 = "", string Texto78 = "", string Texto79 = "", string Texto80 = "", string Texto81 = "", string Texto82 = "", string Texto83 = "", string Texto84 = "", string Texto85 = "", string Texto86 = "", string Texto87 = "", string Texto88 = "", string Texto89 = "", string Texto90 = "", string Texto91 = "", string Texto92 = "", string Texto93 = "", string Texto94 = "", string Texto95 = "", string Texto96 = "", string Texto97 = "", string Texto98 = "", string Texto99 = "", string Texto100 = "")
        {
            List<sp_in_API_Integraciones_Result> INTEGRACIONES = new List<sp_in_API_Integraciones_Result>();

            INTEGRACIONES = GP_ENT.sp_in_API_Integraciones(Archivo,
                                                           UserName,
                                                           FechaProceso,
                                                           Linea,
                                                           Texto1,
                                                           Texto2,
                                                           Texto3,
                                                           Texto4,
                                                           Texto5,
                                                           Texto6,
                                                           Texto7,
                                                           Texto8,
                                                           Texto9,
                                                           Texto10,
                                                           Texto11,
                                                           Texto12,
                                                           Texto13,
                                                           Texto14,
                                                           Texto15,
                                                           Texto16,
                                                           Texto17,
                                                           Texto18,
                                                           Texto19,
                                                           Texto20,
                                                           Texto21,
                                                           Texto22,
                                                           Texto23,
                                                           Texto24,
                                                           Texto25,
                                                           Texto26,
                                                           Texto27,
                                                           Texto28,
                                                           Texto29,
                                                           Texto30,
                                                           Texto31,
                                                           Texto32,
                                                           Texto33,
                                                           Texto34,
                                                           Texto35,
                                                           Texto36,
                                                           Texto37,
                                                           Texto38,
                                                           Texto39,
                                                           Texto40,
                                                           Texto41,
                                                           Texto42,
                                                           Texto43,
                                                           Texto44,
                                                           Texto45,
                                                           Texto46,
                                                           Texto47,
                                                           Texto48,
                                                           Texto49,
                                                           Texto50,
                                                           Texto51,
                                                           Texto52,
                                                           Texto53,
                                                           Texto54,
                                                           Texto55,
                                                           Texto56,
                                                           Texto57,
                                                           Texto58,
                                                           Texto59,
                                                           Texto60,
                                                           Texto61,
                                                           Texto62,
                                                           Texto63,
                                                           Texto64,
                                                           Texto65,
                                                           Texto66,
                                                           Texto67,
                                                           Texto68,
                                                           Texto69,
                                                           Texto70,
                                                           Texto71,
                                                           Texto72,
                                                           Texto73,
                                                           Texto74,
                                                           Texto75,
                                                           Texto76,
                                                           Texto77,
                                                           Texto78,
                                                           Texto79,
                                                           Texto80,
                                                           Texto81,
                                                           Texto82,
                                                           Texto83,
                                                           Texto84,
                                                           Texto85,
                                                           Texto86,
                                                           Texto87,
                                                           Texto88,
                                                           Texto89,
                                                           Texto90,
                                                           Texto91,
                                                           Texto92,
                                                           Texto93,
                                                           Texto94,
                                                           Texto95,
                                                           Texto96,
                                                           Texto97,
                                                           Texto98,
                                                           Texto99,
                                                           Texto100
                                                           ).ToList();
            return INTEGRACIONES;
        }

        //Graba log en archivo txt, depende de la key RegistroArchivoLog, si esta en SI, guardará el mensaje en el log
        //en caso que la key este en NO, el metodo tiene un parametro opcional MostrarSiempre, que guardara el mensaje independiente del valor de la key. Por defecto es NO
        #region Funcion LogInfo para grabar archivo de log
        public static void LogInfo(string NombreProceso, string Mensaje, bool GrabarSiempre = false, bool GuardaEnBD = false, string Referencia = "", string EstructuraJSON = "")
        {
            try
            {
                //Si la key indica grabar log o los mensajes debe mostrarlos siempre, graba el log
                if (ConfigurationManager.AppSettings["RegistroArchivoLog"].ToString() == "SI" || GrabarSiempre == true)
                {
                    StringBuilder html = new StringBuilder();
                    string FilePath = ConfigurationManager.AppSettings["PathLogITEC"].ToString() + "\\Log_API_" + DateTime.Now.ToString("MMdd") + ".txt";

                    html.Append("[" + NombreProceso.ToString() + "]. Fecha: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ". " + Mensaje.Trim());
                    html.Append(Environment.NewLine);

                    StreamWriter strStreamWriter = File.AppendText(FilePath);
                    strStreamWriter.Write(html.ToString());
                    strStreamWriter.Close();
                }

                //Si la key indica grabar log o los mensajes debe mostrarlos siempre, graba el log =================
                if (GuardaEnBD == true && ConfigurationManager.AppSettings["RegistroBDLog"].ToString() == "SI")
                {
                    LogInfoBD(NombreProceso, Mensaje, Referencia, EstructuraJSON);
                }
            }
            catch (Exception e)
            {
                LogInfo(e.Message, "Creación de Log.");
            }
        }
        #endregion

        //Graba log en base de datos, depende de la key RegistroBDLog, si esta en SI, guardará el mensaje en el log
        //en caso que la key este en NO, el metodo tiene un parametro opcional MostrarSiempre, que guardara el mensaje independiente del valor de la key. Por defecto es NO
        #region Funcion LogInfoBD para grabar archivo de log en la Base de datos
        public static void LogInfoBD(string NombreProceso, string Mensaje, string Referencia, string EstructuraJSON = "", string Username = "")
        {
            GP_ENT GP_ENT2 = new GP_ENT();

            try
            {
                List<sp_in_API_LogAPI_Result> LogAPI = GP_ENT2.sp_in_API_LogAPI(NombreProceso, 
                                                                                Referencia, 
                                                                                Mensaje, 
                                                                                EstructuraJSON,
                                                                                Username
                                                                                ).ToList();
                string Resultado;
                Resultado = LogAPI.ElementAt(0).Resultado;

                //RESPONSE.resultado_codigo = estsdd.ElementAt(0).resultado_codigo;
            }
            catch (Exception e)
            {
                LogInfo(e.Message, "Creación de Log BD", true);
            }
        }
        #endregion

        #region Funcion ValidaCampoFecha: Valida formato fecha, si es error gatilla un error excepcion con un mensaje para el usuario
        private DateTime ValidaCampoFecha(string Fecha, string NombreCampoMensaje)
        {
            try
            {
                if (Fecha.Equals(""))
                {
                    return DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                }
                else
                {
                    return DateTime.ParseExact(Fecha, "dd-MM-yyyy", null);
                }
            }
            catch { throw new Exception("FORMATO INCORRECTO EN " + NombreCampoMensaje.Trim() + ". DEBE SER FORMATO dd-MM-yyyy (por ejemplo: 31-12-2028)"); }
        }
        #endregion
    }
}