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
            API_REPONSE_TYPE_2 RESPONSE = new API_REPONSE_TYPE_2();
            API_REPONSE_TYPE_12 RESPONSEDET = new API_REPONSE_TYPE_12();

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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSEDET.resultado = false;
                RESPONSEDET.resultado_descripcion = ERROR.Mensaje;
                RESPONSEDET.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_PickUp_Result> PICKUP = GP_ENT.sp_in_API_PickUp(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.empid
            , REQUEST.numeropedido
            , REQUEST.fechacompra
            , REQUEST.rutcliente
            , REQUEST.nombrecliente
            , REQUEST.email
            , REQUEST.tipodocumento
            , REQUEST.numerodocto
            , REQUEST.fechadocto
            , (decimal)REQUEST.montocompra
            , REQUEST.ruttercero
            , REQUEST.nombretercero
            , REQUEST.emailtercero
            , REQUEST.usuariodig
            , REQUEST.tiposolicitud
            , REQUEST.direccion
            , REQUEST.comuna
            , REQUEST.glosa
            , REQUEST.telefono).ToList();
            if (PICKUP.Where(m => m.GenPickId > 0).Count() > 0)
            {
                RESPONSE.pickUP = (long)PICKUP.ElementAt(0).GenPickId;
                RESPONSEDET.PickId = (int)RESPONSE.pickUP;
                foreach (var item in REQUEST.items.ToList())
                {
                    List<sp_in_API_PickUpDet_Result> PICKUP_DET = GP_ENT.sp_in_API_PickUpDet(
                      this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
                      , REQUEST.empid
                      , (int)RESPONSE.pickUP
                      , item.linea
                      , item.codigoarticulo
                      , item.descripart
                      , item.cantidad
                      , item.HUId
                      , item.peso
                      , item.volumen).ToList();

                    RESPONSEDET.items = PICKUP_DET.Select(m => new
                    {
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
        [Route("EMPRESA/LISTAR")]
        public IHttpActionResult recurso2(API_REQUEST_TYPE_1 REQUEST)
        {
            API_REPONSE_TYPE_1 RESPONSE = new API_REPONSE_TYPE_1();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = true;
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            List<sp_sel_API_EmpresasList_Result> model = GP_ENT.sp_sel_API_EmpresasList(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.limit
            , REQUEST.rowset
            ).ToList();

            foreach (var item in model) { HELPERS.TrimModelProperties(typeof(sp_sel_API_EmpresasList_Result), item); }
            if (model.Count() > 0) { RESPONSE.count = (int)model.ElementAt(0).count; }
            else { RESPONSE.count = 0; }

            RESPONSE.items = model
            .Select(m => new
            {
                Id = m.Id,
                EmpId = m.EmpId,
                RutEmpresa = m.RutEmpresa,
                CodigoExt = m.CodigoExt,
                RazonSocial = m.RazonSocial,
                NombreFantasia = m.NombreFantasia,
                Direccion = m.Direccion,
                Email = m.Email
            })
            .Cast<dynamic>().ToList();
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region LISTAR CONFIRMACIÓN DE RECEPCIÓN (3)
        [HttpGet]
        [Route("CONFRECEPCION/LISTAR")]
        public IHttpActionResult recurso3(API_REQUEST_TYPE_3 REQUEST)
        {
            API_REPONSE_TYPE_3 RESPONSE = new API_REPONSE_TYPE_3();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empID, USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion
            #endregion

            #region AGRUPACION Y SALIDA DE DATOS
            List<sp_sel_API_Confirma_Recep_Result> model_response = GP_ENT.sp_sel_API_Confirma_Recep(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.empID
            , fd
            , fh
            , REQUEST.solRecepID
            , REQUEST.tipoReferencia
            , REQUEST.numeroReferencia
            , REQUEST.rutProveedor
            , REQUEST.limit
            , REQUEST.rowset
            ).ToList();

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
                    SolRecepId = item_recepciones.ElementAt(0).SolRecepId,
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
                        HuId = item_recepciones_det.HuId
                    };
                    HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Recep_Result_MODEL_DET), DET);
                    CAB.items.Add(DET);
                }
                CABECERAS.Add(CAB);

            }

            RESPONSE.cabeceras = CABECERAS.Cast<dynamic>().ToList();
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR CONFIRMACIÓN DE DESPACHO (4)
        [HttpGet]
        [Route("CONFDESPACHO/LISTAR")]
        public IHttpActionResult recurso4(API_REQUEST_TYPE_4 REQUEST)
        {
            API_REPONSE_TYPE_3 RESPONSE = new API_REPONSE_TYPE_3();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empID, USERNAME, out ERROR))
            {
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion
            #endregion

            #region AGRUPACION Y SALIDA DE DATOS
            List<sp_sel_API_Confirma_Desp_Result> model_response = GP_ENT.sp_sel_API_Confirma_Desp(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.empID
            , fd
            , fh
            , REQUEST.SolDespId
            , REQUEST.tipoReferencia
            , REQUEST.numeroReferencia
            , REQUEST.rutProveedor
            , REQUEST.limit
            , REQUEST.rowset
            ).ToList();


            if (model_response.Count() > 0) { RESPONSE.count = (long)model_response.ElementAt(0).Counter; }
            else { RESPONSE.count = 0; }

            List<sp_sel_API_Confirma_Desp_Result_MODEL> CABECERAS = new List<sp_sel_API_Confirma_Desp_Result_MODEL>();
            foreach (var item_despachos in model_response.GroupBy(m => m.ColaPickId).Distinct().ToList())
            {
                sp_sel_API_Confirma_Desp_Result_MODEL CAB = new sp_sel_API_Confirma_Desp_Result_MODEL()
                {
                    Id = item_despachos.ElementAt(0).Id,
                    ColaPickId = item_despachos.ElementAt(0).ColaPickId,
                    INT_NAME = item_despachos.ElementAt(0).INT_NAME,
                    FECHA_HORA = item_despachos.ElementAt(0).FECHA_HORA,
                    SolDespId = item_despachos.ElementAt(0).SolDespId,
                    FechaProceso = item_despachos.ElementAt(0).FechaProceso,
                    TipoDocumento = item_despachos.ElementAt(0).TipoDocumento,
                    NumeroDocto = item_despachos.ElementAt(0).NumeroDocto,
                    FechaDocto = item_despachos.ElementAt(0).FechaDocto,
                    TipoReferencia = item_despachos.ElementAt(0).TipoReferencia,
                    NumeroReferencia = item_despachos.ElementAt(0).NumeroReferencia,
                    FechaReferencia = item_despachos.ElementAt(0).FechaReferencia,
                    //  Counter = item_despachos.ElementAt(0).Counter,
                    RutCliente = item_despachos.ElementAt(0).RutCliente
                };
                HELPERS.TrimModelProperties(typeof(sp_sel_API_Confirma_Desp_Result_MODEL), CAB);
                CAB.items = new List<sp_sel_API_Confirma_Desp_Result_MODEL_DET>();
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
                    CAB.items.Add(DET);
                }
                CABECERAS.Add(CAB);

            }

            RESPONSE.cabeceras = CABECERAS.Cast<dynamic>().ToList();
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region CREAR INTEGRACIONES (5)
        [HttpPost]
        [Route("INTEGRACIONES/CREAR")]
        public IHttpActionResult recurso5([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_REPONSE_TYPE_4_OK RESPONSE_OK = new API_REPONSE_TYPE_4_OK();
            API_REPONSE_TYPE_4_ERROR RESPONSE_ERROR = new API_REPONSE_TYPE_4_ERROR();
            ERROR = new API_RESPONSE_ERRORS();
            string USERNAME = "";
            HttpStatusCode STATUS_CODE = new HttpStatusCode();

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
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
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

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE_OK.resultado = true;
            RESPONSE_OK.resultado_descripcion = "";
            RESPONSE_OK.resultado_codigo = 0;
            RESPONSE_OK.count = 1;

            #region PROCESAMIENTO
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
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion
            var model = GP_ENT.sp_in_API_Integracion(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.empid
            , REQUEST.archivo
            , REQUEST.txt).ToList();

            var serialize = JsonConvert.SerializeObject(model);

            List<sp_in_API_Integracion_Result> INTEGRACIONES = JsonConvert.DeserializeObject<List<sp_in_API_Integracion_Result>>(serialize);



            //List<sp_in_API_Integracion_Result> INTEGRACIONES = null;

            if (INTEGRACIONES.Count == 0)
            {
                ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, Estado = 1, IndTipo = false, Mensaje = "Error al obtener información", Nombre = "ERROR PERSONALIZADO" };
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
            }

            else if (INTEGRACIONES.Where(m => m.Generacion > 0).Count() > 0)
            //if (INTEGRACIONES[0].Generacion > 0)
            {
                RESPONSE_OK.id = (int)INTEGRACIONES.ElementAt(0).Generacion;
                RESPONSE_OK.descripcion = (string)INTEGRACIONES.ElementAt(0).GlosaEstado;
            }
            else if (INTEGRACIONES.Where(m => m.Generacion == 0).Count() > 0)
            //else if (INTEGRACIONES[0].Generacion == 0)
            {
                #region NOK
                ERROR = new API_RESPONSE_ERRORS() { ErrID = 2000, Estado = 1, IndTipo = false, Mensaje = INTEGRACIONES.ElementAt(0).GlosaEstado.ToUpper(), Nombre = "ERROR PERSONALIZADO" };
                // ERROR = API_CLS.API_RESPONSE_ERRORS.Find(2000); //REQUEST - PICKUP ERROR DUPLICIDAD
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = "ERROR";
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                RESPONSE_ERROR.count = 0;
                STATUS_CODE = HttpStatusCode.Unauthorized;

                foreach (var item in INTEGRACIONES) { HELPERS.TrimModelProperties(typeof(sp_in_API_Integracion_Result), item); }
                //if (model.Count() > 0) { RESPONSE.count = (int)model.ElementAt(0).count; }
                //else { RESPONSE.count = 0; }

                RESPONSE_ERROR.errores = INTEGRACIONES
                .Select(m => new
                {
                    Descripcion = m.GlosaEstado
                })
                .Cast<dynamic>().ToList();


                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
                #endregion
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE_ERROR.resultado = false;
                RESPONSE_ERROR.resultado_descripcion = ERROR.Mensaje;
                RESPONSE_ERROR.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE_ERROR);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE_OK);
            #endregion
        }
        #endregion

        #region CREAR PRODUCTO (6)
        [HttpPost]
        [Route("PRODUCTO/CREAR")]
        public IHttpActionResult recurso6([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_REPONSE_TYPE_5 RESPONSE = new API_REPONSE_TYPE_5();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = false;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR PRODUCTO (7)
        [HttpGet]
        [Route("PRODUCTO/LISTAR")]
        public IHttpActionResult recurso7(API_REQUEST_TYPE_7 REQUEST)
        {
            API_REPONSE_TYPE_6 RESPONSE = new API_REPONSE_TYPE_6();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            List<sp_sel_API_ListarProducto_Result> model = GP_ENT.sp_sel_API_ListarProducto(
            REQUEST.empid,
            REQUEST.codigoarticulo,
            REQUEST.descripcion,
            REQUEST.lineaproducto,
            REQUEST.tipoproducto,
            REQUEST.ean13,
            REQUEST.dun14,
            REQUEST.limit,
            REQUEST.rowset
            ).ToList();

            foreach (var item in model) { HELPERS.TrimModelProperties(typeof(sp_sel_API_ListarProducto_Result), item); }
            if (model.Count() > 0) { RESPONSE.count = (int)model.ElementAt(0).Count; }
            else { RESPONSE.count = 0; }

            RESPONSE.items = model
            .Select(m => new
            {
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
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region CREAR SOLICITUD RECEPCIÓN (8)
        [HttpPost]
        [Route("SOLRECEP/CREAR")]
        public IHttpActionResult recurso8([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_REPONSE_TYPE_7 RESPONSE = new API_REPONSE_TYPE_7();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion
            List<sp_in_API_SolRecep_Result> SOLRECEP = GP_ENT.sp_in_API_SolRecep(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , REQUEST.empid
            , REQUEST.txt).ToList();

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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region CREAR SOLICITUD DESPACHO (9)
        [HttpPost]
        [Route("SOLDESP/CREAR")]
        public IHttpActionResult recurso9([FromBody] API_REQUEST_TYPE_6 REQUEST)
        {
            API_REPONSE_TYPE_8 RESPONSE = new API_REPONSE_TYPE_8();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region LISTAR TRACKING SDD (10)
        [HttpGet]
        [Route("TRACKINGSDD/LISTAR")]
        public IHttpActionResult recurso10(API_REQUEST_TYPE_8 REQUEST)
        {
            API_REPONSE_TYPE_6 RESPONSE = new API_REPONSE_TYPE_6();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fi = new DateTime();
            DateTime ft = new DateTime();

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.limit = REQUEST.limit;
            RESPONSE.rowset = REQUEST.rowset;

            #region VALIDACIONES DE CAMPOS
            REQUEST.fechaInicio = (REQUEST.fechaInicio == null ? "" : REQUEST.fechaInicio);
            REQUEST.fechaTermino = (REQUEST.fechaTermino == null ? "" : REQUEST.fechaTermino);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
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

                if (REQUEST.tipoReferencia.Trim().Length > 80) { throw new Exception("ERROR - TIPOREFERENCIA > QUE EL LARGO PERMITIDO 80"); }
                if (REQUEST.rutCliente.Trim().Length > 12) { throw new Exception("ERROR - NOMBRECLIENTE > QUE EL LARGO PERMITIDO 12"); }
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion
            #endregion

            List<sp_in_API_TrackingSDD_Result> TRACKINGSDD = GP_ENT.sp_in_API_TrackingSDD(
            REQUEST.empId,
            fi,
            ft,
            REQUEST.solDespId,
            REQUEST.tipoReferencia,
            REQUEST.numeroReferencia,
            REQUEST.rutCliente,
            REQUEST.limit,
            REQUEST.rowset
            ).ToList();

            foreach (var item in TRACKINGSDD) { HELPERS.TrimModelProperties(typeof(sp_in_API_TrackingSDD_Result), item); }
            if (TRACKINGSDD.Count() > 0) { RESPONSE.count = (int)TRACKINGSDD.ElementAt(0).Count; }
            else { RESPONSE.count = 0; }

            RESPONSE.items = TRACKINGSDD
            .Select(m => new
            {
                SolDespId = m.SolDespId,
                TipoReferencia = m.TipoReferencia,
                NumeroReferencia = m.NumeroReferencia,
                RutCliente = m.RutCliente,
                Estado = m.Estado,
                EstadoGlosa = m.EstadoGlosa,
                FechaEstado = m.FechaEstado
            })
            .Cast<dynamic>().ToList();
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR TRACKING PICKING (20)
        [HttpGet]
        [Route("TRACKINGCOURIER/LISTAR")]
        public IHttpActionResult recurso21(API_REQUEST_TYPE_16 REQUEST)
        {
            API_REPONSE_TYPE_14 RESPONSE = new API_REPONSE_TYPE_14();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                        API_REPONSE_TYPE_14_CAB responseCab = new API_REPONSE_TYPE_14_CAB();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
            API_REPONSE_TYPE_10 RESPONSE = new API_REPONSE_TYPE_10();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                }
                BULTOS = GP_ENT.sp_in_API_Confirma_Recep_Bultos(
                    REQUEST.empId,
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
            //      this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA CONFORME CLIENTE (12)
        [HttpPost]
        [Route("ENTREGACONFORMECLIENTE/CREAR")]
        public IHttpActionResult recurso12([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_REPONSE_TYPE_10 RESPONSE = new API_REPONSE_TYPE_10();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA NO CONFORME CLIENTE (13)
        [HttpPost]
        [Route("ENTREGANOCONFORMECLIENTE/CREAR")]
        public IHttpActionResult recurso13([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_REPONSE_TYPE_10 RESPONSE = new API_REPONSE_TYPE_10();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region CONFIRMAR ENTREGA CONFORME TIENDA (14)
        [HttpPost]
        [Route("ENTREGACONFORMETIENDA/CREAR")]
        public IHttpActionResult recurso14([FromBody] API_REQUEST_TYPE_11_CAB REQUEST)
        {
            API_REPONSE_TYPE_10 RESPONSE = new API_REPONSE_TYPE_10();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion

        }
        #endregion

        #region LISTAR CONSULTA STOCK (15)
        [HttpGet]
        [Route("CONSTOCK/LISTAR")]
        public IHttpActionResult recurso15(API_REQUEST_TYPE_9 REQUEST)
        {
            API_REPONSE_TYPE_9 RESPONSE = new API_REPONSE_TYPE_9();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.tipoProducto = (REQUEST.tipoProducto == null ? "" : REQUEST.tipoProducto);
            REQUEST.codigoArticulo = (REQUEST.codigoArticulo == null ? "" : REQUEST.codigoArticulo);
            #region LARGOS PERMITIDOS
            #endregion
            #endregion

            List<sp_sel_API_Consulta_Stock_Result> STOCK = GP_ENT.sp_sel_API_Consulta_Stock(
            REQUEST.empId
            , REQUEST.lineaProducto
            , REQUEST.tipoProducto
            , REQUEST.codigoArticulo
            , USERNAME
            ).ToList();

            foreach (var item in STOCK) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Result), item); }
            if (STOCK.Count() > 0) { RESPONSE.count = (int)STOCK.ElementAt(0).Count; }
            else { RESPONSE.count = 0; }

            RESPONSE.items = STOCK
            .Select(m => new
            {
                serie = m.NumeroSerie,
                codigoArticulo = m.CodigoArticulo,
                stock = m.Stock
            })
            .Cast<dynamic>().ToList();

            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region GeneracionRutas_Beetrack (16)
        [HttpPost]
        [Route("GeneracionRutas_Beetrack")] //CAMBIAR NOMBER
        public IHttpActionResult recurso16(API_REQUEST_TYPE_12 REQUEST)
        {
            API_REPONSE_TYPE_10 RESPONSE = new API_REPONSE_TYPE_10();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            DateTime createAt = new DateTime(); //???

            DateTime minDeliveryTime = new DateTime(); //???
            DateTime maxDeliveryTime = new DateTime(); //???

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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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

            List<sp_in_API_Integracion_Result> STOPS = GP_ENT.sp_in_API_Integracion(
            this.Request.Headers.First(header => header.Key == VARS.VARS_HEADER_TOKEN).Value.FirstOrDefault()
            , 1
            , nombreArchivo
            , newTxt
            ).ToList();

            RESPONSE.count = 1;
            RESPONSE.resultado = "OK";
            RESPONSE.descripcion = "Integración realizada correctamente";
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR CONSULTA STOCK BODEGA (GET)(17)
        [HttpGet]
        [Route("CONSTOCKBODEGA/LISTAR")]
        public IHttpActionResult Recurso16(API_REQUEST_TYPE_9 REQUEST)
        {
            API_REPONSE_TYPE_9 RESPONSE = new API_REPONSE_TYPE_9();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.tipoProducto = (REQUEST.tipoProducto == null ? "" : REQUEST.tipoProducto);
            REQUEST.codigoArticulo = (REQUEST.codigoArticulo == null ? "" : REQUEST.codigoArticulo);
            #region LARGOS PERMITIDOS
            #endregion
            #endregion

            List<sp_sel_API_Consulta_Stock_Bodega_Result> STOCK = GP_ENT.sp_sel_API_Consulta_Stock_Bodega(
            REQUEST.empId
            , REQUEST.lineaProducto
            , REQUEST.tipoProducto
            , REQUEST.codigoArticulo
            , USERNAME
            ).ToList();

            foreach (var item in STOCK) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Result), item); }
            if (STOCK.Count() > 0) { RESPONSE.count = (int)STOCK.ElementAt(0).Count; }
            else { RESPONSE.count = 0; }

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

            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region LISTAR CONSULTA STOCK BODEGA (POST)(18)
        [HttpPost]
        [Route("CONSTOCKBODEGA/LISTAR")]
        public IHttpActionResult recurso16_2(API_REQUEST_TYPE_9 REQUEST)
        {
            API_REPONSE_TYPE_9 RESPONSE = new API_REPONSE_TYPE_9();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;

            #region VALIDACIONES DE CAMPOS
            REQUEST.tipoProducto = (REQUEST.tipoProducto == null ? "" : REQUEST.tipoProducto);
            REQUEST.codigoArticulo = (REQUEST.codigoArticulo == null ? "" : REQUEST.codigoArticulo);
            #region LARGOS PERMITIDOS
            #endregion
            #endregion

            List<sp_sel_API_Consulta_Stock_Bodega_Result> STOCK = GP_ENT.sp_sel_API_Consulta_Stock_Bodega(
            REQUEST.empId
            , REQUEST.lineaProducto
            , REQUEST.tipoProducto
            , REQUEST.codigoArticulo
            , USERNAME
            ).ToList();

            foreach (var item in STOCK) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Result), item); }
            if (STOCK.Count() > 0) { RESPONSE.count = (int)STOCK.ElementAt(0).Count; }
            else { RESPONSE.count = 0; }

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

            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        // INICIO recurso 17 CREAR SOLICITUD RECEPCION JSON (17) **********************************
        #region CREAR SOLICITUD RECEPCION JSON (17)
        [HttpPost]
        [Route("SOLRECEP/CREARJSON")]
        public IHttpActionResult recurso17([FromBody] API_REQUEST_TYPE_14 REQUEST)
        {
            API_REPONSE_TYPE_7 RESPONSE = new API_REPONSE_TYPE_7();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            //valida estructura json recibido sea valida ---
            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion

            //valida usuario empresa ---
            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empid, USERNAME, out ERROR))
            {
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime FechaReferencia = new DateTime(); //fechaReferencia
            DateTime fechaVectoLote = new DateTime(); //fechaVectoLote
            DateTime FechaProceso = new DateTime();
            string ListadoCodigoArticulo = "";

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO 

            #region CONTROL DE CAMPOS REQUERIDOS Y OPCIONALES

            //valida datos cabecera json
            // - campos string       : si no vienen en el JSON quedan null
            // - campos numericos    : si no vienen en el JSON quedan con un cero
            // - un campo string se puede dejar opcional enviando un valor por defecto 

            try
            {
                if (REQUEST.fechaProceso == null || REQUEST.fechaProceso == "") { throw new Exception("Debe informar Fecha Proceso"); } //requerido
                if (REQUEST.tipoSolicitud <= 0) { throw new Exception("Debe informar Tipo Solicitud > 0"); } //requerido
                REQUEST.comprador = (REQUEST.comprador == null ? "" : REQUEST.comprador); //opcional
                if (REQUEST.proveedor == null || REQUEST.proveedor == "") { throw new Exception("Debe informar Proveedor"); } //requerido
                if (REQUEST.razonSocial == null || REQUEST.razonSocial == "") { throw new Exception("Debe informar Razon Social"); } //requerido
                if (REQUEST.tipoReferencia == null || REQUEST.tipoReferencia == "") { throw new Exception("Debe informar Tipo Referencia"); } //requerido
                if (REQUEST.numeroReferencia == null || REQUEST.numeroReferencia == "") { throw new Exception("Debe informar Número Referencia"); } //requerido
                if (REQUEST.fechaReferencia == null || REQUEST.fechaReferencia == "") { throw new Exception("Debe informar Fecha Referencia"); } //requerido
                if (REQUEST.glosa == null || REQUEST.glosa == "") { throw new Exception("Debe informar Glosa"); } //requerido

                int PrimerItem = 1;

                //Valida que el json tenga items ---
                if (REQUEST.items == null) { throw new Exception("Debe informar Items"); } //requerido
                if (REQUEST.items.Count == 0) { throw new Exception("Debe informar Items"); } //requerido

                //Valida datos de Items json
                foreach (var item in REQUEST.items)
                {
                    if (item.CodigoArticulo == null || item.CodigoArticulo == "") { throw new Exception("Debe informar Codigo Articulo"); } //requerido
                    if (item.UnidadCompra == null || item.UnidadCompra == "") { throw new Exception("Debe informar Unidad Compra"); } //requerido
                    if (item.Cantidad <= 0) { throw new Exception("Debe informar Cantidad > 0"); } //requerido
                    item.NumeroSerie = (item.NumeroSerie == null ? "" : item.NumeroSerie); //opcional
                    item.FechaVectoLote = (item.FechaVectoLote == null ? "" : item.FechaVectoLote); //opcional
                    if (item.Estado <= 0) { throw new Exception("Debe informar Estado > 0 "); } //requerido

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
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.descripcion = ex.Message.Trim();
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }

            #endregion

            #region VALIDA LARGOS PERMITIDOS Y TIPOS DE DATOS
            try
            {
                #region VALIDA CAMPOS CABECERA JSON 
                try
                {
                    if (REQUEST.fechaReferencia.Equals(""))
                    {
                        FechaReferencia = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        FechaReferencia = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }

                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        FechaProceso = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        FechaProceso = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAPROCESO DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }

                #endregion

                #region VALIDA ITEMS DETALLE JSON
                foreach (var item in REQUEST.items.ToList())
                {
                    try
                    {
                        if (item.FechaVectoLote.Equals(""))
                        {
                            fechaVectoLote = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fechaVectoLote = DateTime.ParseExact(item.FechaVectoLote, "dd-MM-yyyy", null);
                        }
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
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }            
            #endregion

            //crea variables de lista para recuperar retorno de los procedimientos almacenados
            List<sp_in_API_SolRecepcionJson_Result> SOLRECEP = new List<sp_in_API_SolRecepcionJson_Result>();
            List<sp_in_API_SolRecepcionDetJson_Result> SOLRECEPDET = new List<sp_in_API_SolRecepcionDetJson_Result>();
            List<sp_proc_API_SolRecepcionValidaJson_Result> SOLRECEPVALIDA = new List<sp_proc_API_SolRecepcionValidaJson_Result>();

            //Inserta cabecera solicitud recepcion 
            SOLRECEP = GP_ENT.sp_in_API_SolRecepcionJson(REQUEST.empid,
                                                         USERNAME,
                                                         FechaProceso,
                                                         REQUEST.tipoSolicitud,
                                                         REQUEST.comprador,
                                                         REQUEST.proveedor,
                                                         REQUEST.razonSocial,
                                                         REQUEST.tipoReferencia,
                                                         REQUEST.numeroReferencia,
                                                         FechaReferencia,
                                                         REQUEST.glosa
                                                         ).ToList();
            if (SOLRECEP.Count > 0)
            {
                if (SOLRECEP[0].Count > 0) //pregunta por el campo Count, si es mayor a cero procesó OK la cabecera
                {
                    RESPONSE.solRecepId = (int)SOLRECEP[0].SolRecepId;
                    RESPONSE.descripcion = SOLRECEP[0].Descripcion;

                    //recorre items json
                    foreach (var item2 in REQUEST.items.ToList())
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
                                                                           item2.PorcQA
                                                                           ).ToList();
                        if (SOLRECEPDET.Count > 0)
                        {
                            if (SOLRECEPDET[0].Count == 0) //pregunta por el CAMPO Count, si es mayor a cero procesó OK el detalle
                            {
                                #region NOK
                                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO
                                RESPONSE.resultado = "ERROR";
                                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                                RESPONSE.resultado_codigo = ERROR.ErrID;
                                RESPONSE.solRecepId = 0;
                                RESPONSE.descripcion = "Error procesando Articulo: " + item2.CodigoArticulo + ", " + SOLRECEPDET[0].Descripcion;
                                STATUS_CODE = HttpStatusCode.Unauthorized;
                                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                                #endregion
                            }
                        }
                    }

                    //Si finalizo ok el ciclo de los items, llama a procedimiento que realiza validacion de informacion generada encabecera y detalle
                    SOLRECEPVALIDA = GP_ENT.sp_proc_API_SolRecepcionValidaJson(RESPONSE.solRecepId,
                                                                               ListadoCodigoArticulo,
                                                                               REQUEST.items.Count
                                                                               ).ToList();
                    if (SOLRECEPVALIDA.Count > 0)
                    {
                        if (SOLRECEPVALIDA[0].Resultado != "OK")
                        {
                            #region NOK
                            ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1016);//REQUEST - ERROR NO ESPECIFICADO
                            RESPONSE.resultado = "ERROR";
                            RESPONSE.resultado_descripcion = ERROR.Mensaje;
                            RESPONSE.resultado_codigo = ERROR.ErrID;
                            RESPONSE.solRecepId = 0;
                            RESPONSE.descripcion = SOLRECEPVALIDA[0].Descripcion;
                            STATUS_CODE = HttpStatusCode.Unauthorized;
                            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                            #endregion
                        }
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1017);//REQUEST - ERROR NO ESPECIFICADO
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = SOLRECEP[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion

            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion
        // fin recurso 17 CREAR SOLICITUD RECEPCION JSON (17) **********************************

        #region CREAR SOLICITUD DESPACHO JSON (18)
        [HttpPost]
        [Route("SOLDESP/CREARJSON")]
        public IHttpActionResult recurso18([FromBody] API_REQUEST_TYPE_13 REQUEST)
        {
            API_REPONSE_TYPE_8 RESPONSE = new API_REPONSE_TYPE_8();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            DateTime fRef = new DateTime(); //fechaReferencia
            DateTime fPro = new DateTime(); //fechaRequerida
            DateTime fDoc = new DateTime(); //fechaDocto
            DateTime fVecLot = new DateTime(); //fechaVectoLote

            STATUS_CODE = HttpStatusCode.OK;
            RESPONSE.resultado = "OK";
            RESPONSE.resultado_descripcion = "";
            RESPONSE.resultado_codigo = 0;
            RESPONSE.count = 1;

            #region PROCESAMIENTO
            #region VALIDACIONES DE CAMPOS
            REQUEST.proceso = (REQUEST.proceso == null ? "" : REQUEST.proceso);
            REQUEST.tipoReferencia = (REQUEST.tipoReferencia == null ? "" : REQUEST.tipoReferencia);
            REQUEST.fechaReferencia = (REQUEST.fechaReferencia == null ? "" : REQUEST.fechaReferencia);
            REQUEST.fechaProceso = (REQUEST.fechaProceso == null ? "" : REQUEST.fechaProceso);
            REQUEST.fechaDocto = (REQUEST.fechaDocto == null ? "" : REQUEST.fechaDocto);
            REQUEST.glosa = (REQUEST.glosa == null ? "" : REQUEST.glosa);
            REQUEST.cliente = (REQUEST.cliente == null ? "" : REQUEST.cliente);
            REQUEST.razonSocial = (REQUEST.razonSocial == null ? "" : REQUEST.razonSocial);
            REQUEST.telefono = (REQUEST.telefono == null ? "" : REQUEST.telefono);
            REQUEST.email = (REQUEST.email == null ? "" : REQUEST.email);
            REQUEST.direccion = (REQUEST.direccion == null ? "" : REQUEST.direccion);
            REQUEST.contacto = (REQUEST.contacto == null ? "" : REQUEST.contacto);

            foreach (var item in REQUEST.items)
            {
                item.codigoArticulo = (item.codigoArticulo == null ? "" : item.codigoArticulo);
                item.unidadVenta = (item.unidadVenta == null ? "" : item.unidadVenta);
                item.numeroSerie = (item.numeroSerie == null ? "" : item.numeroSerie);
                item.fechaVectoLote = (item.fechaVectoLote == null ? "" : item.fechaVectoLote);
            }
            #region LARGOS PERMITIDOS
            try
            {
                try
                {
                    if (REQUEST.fechaReferencia.Equals(""))
                    {
                        fRef = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fRef = DateTime.ParseExact(REQUEST.fechaReferencia, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREFERENCIA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaProceso.Equals(""))
                    {
                        fPro = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fPro = DateTime.ParseExact(REQUEST.fechaProceso, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREQUERIDA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                try
                {
                    if (REQUEST.fechaDocto.Equals(""))
                    {
                        fDoc = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                    }
                    else
                    {
                        fDoc = DateTime.ParseExact(REQUEST.fechaDocto, "dd-MM-yyyy", null);
                    }
                }
                catch { throw new Exception("ERROR - FECHAREQUERIDA DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion

            #endregion

            List<sp_in_API_SolDespJson_Result> SOLDESPJSON = new List<sp_in_API_SolDespJson_Result>();
            foreach (var item in REQUEST.items.ToList())
            {
                try
                {
                    try
                    {
                        if (item.fechaVectoLote.Equals(""))
                        {
                            fVecLot = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", null);
                        }
                        else
                        {
                            fVecLot = DateTime.ParseExact(item.fechaVectoLote, "dd-MM-yyyy", null);
                        }
                    }
                    catch { throw new Exception("ERROR - FECHAVECTOLOTE DEBE SER FORMATO FECHA dd-MM-yyyy, por ejemplo 06-11-2020"); }
                }
                catch (Exception ex)
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1013);//REQUEST - CAMPOS RECIBIDOS NO COINCIDEN CON LARGOS PERMITIDOS
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = ERROR.Mensaje + "(" + ex.Message.Trim() + ")";
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                    #endregion
                }
            }

            SOLDESPJSON = GP_ENT.sp_in_API_SolDespJson(
                REQUEST.proceso
                , REQUEST.empid
                , REQUEST.tipoSolicitud
                , REQUEST.tipoReferencia
                , REQUEST.numeroReferencia
                , fRef
                , fPro
                , REQUEST.tipoDocumento
                , REQUEST.numeroDocto
                , fDoc
                , REQUEST.glosa
                , REQUEST.cliente
                , REQUEST.razonSocial
                , REQUEST.telefono
                , REQUEST.email
                , REQUEST.direccion
                , REQUEST.contacto
                , REQUEST.rutaDespacho
                , REQUEST.region
                , REQUEST.comuna
                , REQUEST.ciudad
                , REQUEST.vendedor
                ).ToList();

            if (SOLDESPJSON.Count > 0)
            {
                if (SOLDESPJSON[0].Count > 0)
                {
                    RESPONSE.solDespId = (int)SOLDESPJSON[0].SolDespId;
                    RESPONSE.descripcion = SOLDESPJSON[0].Descripcion;
                    foreach (var item2 in REQUEST.items.ToList())
                    {
                        var SOLDESPJSONJSON = GP_ENT.sp_in_API_SolDespDetJson(
                         RESPONSE.solDespId,
                         item2.codigoArticulo,
                         item2.unidadVenta,
                         item2.numeroSerie,
                         fVecLot,
                         item2.cantidad,
                         item2.estado,
                         item2.costoUnitario,
                         item2.kilosTotales,
                         item2.porcQa,
                         item2.maquila,
                         item2.pallet,
                         item2.itemReferencia);

                        //if(SOLDESPJSONJSON[0].Count > 0)
                        //{
                        //    #region NOK
                        //    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                        //    RESPONSE.resultado = "ERROR";
                        //    RESPONSE.resultado_descripcion = SOLDESPJSONJSON[0].Descripcion;
                        //    RESPONSE.resultado_codigo = ERROR.ErrID;
                        //    STATUS_CODE = HttpStatusCode.Unauthorized;
                        //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                        //    #endregion
                        //}
                    }
                }
                else
                {
                    #region NOK
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                    RESPONSE.resultado = "ERROR";
                    RESPONSE.resultado_descripcion = SOLDESPJSON[0].Descripcion;
                    RESPONSE.resultado_codigo = ERROR.ErrID;
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                    #endregion
                }
            }
            else
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region SUBIR IMAGEN (21)
        [HttpPost]
        [Route("SUBIRIMG")]
        public IHttpActionResult recurso19([FromBody] API_REQUEST_TYPE_15 REQUEST)
        {
            API_REPONSE_TYPE_11 RESPONSE = new API_REPONSE_TYPE_11();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA DATOS RECIBIDOS NOK
            if (!ModelState.IsValid)
            {
                #region RESPONSE NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1015); //REQUEST - ERROR EN CUERPO RECIBIDO
                RESPONSE.resultado = "ERROR";
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            //    #endregion
            //}
            #endregion

            #endregion

            List<sp_in_API_Image_Result> IMG = GP_ENT.sp_in_API_Image(
            USERNAME
            , REQUEST.empid
            , REQUEST.nombreArchivo
            ).ToList();

            if (IMG.ElementAt(0).Resultado > 0)
            {
                try
                {
                    if (Directory.Exists(ConfigurationSettings.AppSettings["ImgCliente"]))
                    {
                        Console.WriteLine("That path exists already.");
                    }
                    else
                    {
                        Directory.CreateDirectory(ConfigurationSettings.AppSettings["ImgCliente"]);
                    }

                    //string nombrearchivo1 = "";
                    string image1 = "";
                    string extencion = "";
                    extencion = ConfigurationSettings.AppSettings["Extension"].ToString();

                    byte[] dataBuffer = Convert.FromBase64String(REQUEST.imgBase64);
                    //nombrearchivo1 = SGA_MVMovilEvalAdj.UserName.Trim() + "_" + SGA_MVMovilEvalAdj.Proyecto + "_" + SGA_MVMovilEvalAdj.TareaId + "_" + SGA_MVMovilEvalAdj.Pregunta + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + extencion.Trim();

                    image1 = ConfigurationSettings.AppSettings["ImgCliente"] + REQUEST.nombreArchivo + extencion;

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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }

            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
            #endregion

        }
        #endregion

        #region CREAR ADJ SDD (22)
        [HttpPost]
        [Route("SOLDESPACHOADJ/CREAR")]
        public IHttpActionResult recurso22(API_REQUEST_TYPE_17 REQUEST)
        {
            API_REPONSE_TYPE_13 RESPONSE = new API_REPONSE_TYPE_13();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            //#region VALIDA EMPRESA NOK
            //if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            //{
            //    RESPONSE.resultado = 0;
            //    RESPONSE.resultado_descripcion = ERROR.Mensaje;
            //    RESPONSE.resultado_codigo = ERROR.ErrID;
            //    STATUS_CODE = HttpStatusCode.Unauthorized;
            //    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region ACTUALIZA ESTADO SDD (23)
        [HttpGet]
        [Route("ESTADOSDD/ACTUALIZAR")]
        public IHttpActionResult recurso23(API_REQUEST_TYPE_18 REQUEST)
        {
            API_REPONSE_TYPE_15 RESPONSE = new API_REPONSE_TYPE_15();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                List<sp_upd_cambEstado_SDD_Api_Result> estsdd = GP_ENT.sp_upd_cambEstado_SDD_Api(
                REQUEST.empId,
                USERNAME,
                REQUEST.numeroReferencia,
                REQUEST.sdd,
                REQUEST.estado
                ).ToList();

                RESPONSE.resultado_codigo = estsdd.ElementAt(0).resultado_codigo;
                RESPONSE.resultado_descripcion = estsdd.ElementAt(0).resultado_descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region ACTUALIZA ESTADO SDR (24)
        [HttpGet]
        [Route("ESTADOSDR/ACTUALIZAR")]
        public IHttpActionResult recurso24(API_REQUEST_TYPE_19 REQUEST)
        {
            API_REPONSE_TYPE_15 RESPONSE = new API_REPONSE_TYPE_15();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                List<sp_upd_cambEstado_SDR_Api_Result> estsdr = GP_ENT.sp_upd_cambEstado_SDR_Api(
                REQUEST.empId,
                USERNAME,
                REQUEST.numeroReferencia,
                REQUEST.sdr,
                REQUEST.estado
                ).ToList();

                RESPONSE.resultado_codigo = estsdr.ElementAt(0).resultado_codigo;
                RESPONSE.resultado_descripcion = estsdr.ElementAt(0).resultado_descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region CONSULTA STOCK BODEGA UBICACIÓN (25)
        [HttpGet]
        [Route("STOCKBODEGAUBI/LISTAR")]
        public IHttpActionResult recurso25(API_REQUEST_TYPE_20 REQUEST)
        {
            API_REPONSE_TYPE_16 RESPONSE = new API_REPONSE_TYPE_16();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.empId, USERNAME, out ERROR))
            {
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                RESPONSE.resultado_codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                List<sp_sel_API_Consulta_Stock_Bodega_Ubicacion_Result> csbur = GP_ENT.sp_sel_API_Consulta_Stock_Bodega_Ubicacion(
                REQUEST.empId,
                REQUEST.lineaProducto,
                REQUEST.tipoProducto,
                REQUEST.CodigoArticulo,
                USERNAME
                ).ToList();

                if (csbur.Count != 0)
                {
                    RESPONSE.count = csbur.ElementAt(0).Count;
                    RESPONSE.resultado = csbur.ElementAt(0).Resultado;
                    RESPONSE.codigoBodega = csbur.ElementAt(0).CodigoBodega;
                    RESPONSE.glosaBodega = csbur.ElementAt(0).GlosaBodega;

                    //foreach (var item in csbur) { HELPERS.TrimModelProperties(typeof(sp_sel_API_Consulta_Stock_Bodega_Ubicacion_Result), item); }

                    RESPONSE.detalle = csbur
                    .Select(m => new
                    {
                        codigoArticulo = m.CodigoArticulo,
                        codigoUbicacion = m.CodigoUbicacion,
                        descripcion = m.Descripcion,
                        stock = m.Stock
                    })
                    .Cast<dynamic>().ToList();
                }
                else
                {
                    RESPONSE.count = 0;
                    RESPONSE.resultado = "Vacío";
                    RESPONSE.resultado_codigo = 0;
                    RESPONSE.resultado_descripcion = "No se han encontrado registros";
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                }

            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.resultado_codigo = ERROR.ErrID;
                RESPONSE.resultado_descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion

        #region INTEGRACIÓN CONFIRMACIONES JSON (26)
        [HttpPost]
        [Route("integracion_confirmaciones_json/LISTAR")]
        public IHttpActionResult recurso27(API_REQUEST_TYPE_22 REQUEST)
        {
            API_REPONSE_TYPE_18 RESPONSE = new API_REPONSE_TYPE_18();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            #region VALIDACIONES DE CAMPOS
            #endregion
            try
            {
                var model = GP_ENT.sp_sel_API_IntegraConfirmacionesJson(REQUEST.EmpId, REQUEST.NombreProceso, REQUEST.Limit, REQUEST.RowSet).ToList();
                if(model.Count() == 0)
                {
                    RESPONSE.Count = 0;
                    RESPONSE.Resultado = "Vacío";
                    RESPONSE.Resultado_Codigo = 0;
                    RESPONSE.Resultado_Descripcion = "No se han encontrado registros";
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);

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
                                                      Items = from Items in model
                                                              join its in Resultado
                                                                  on Items.Linea equals its.Linea
                                                              group Confirmacion by new
                                                              {
                                                                  Items.Linea,
                                                                  Items.ItemReferencia,
                                                                  Items.Cadena,
                                                                  Items.CodigoArticulo,
                                                                  Items.NroSerieDesp,
                                                                  Items.FechaVectoDesp,
                                                                  Items.UnidadMedida,
                                                                  Items.Cantidad,
                                                                  Items.CantidadProc,
                                                              } into Items
                                                              select new
                                                              {
                                                                  Linea = Items.Key.Linea,
                                                                  ItemReferencia = Items.Key.ItemReferencia,
                                                                  Cadena = Items.Key.Cadena,
                                                                  CodigoArticulo = Items.Key.CodigoArticulo,
                                                                  NroSerieDesp = Items.Key.NroSerieDesp,
                                                                  FechaVectoDesp = Items.Key.FechaVectoDesp,
                                                                  UnidadMedida = Items.Key.UnidadMedida,
                                                                  Cantidad = Items.Key.Cantidad,
                                                                  CantidadProc = Items.Key.CantidadProc,
                                                              }
                                                  }

                               };
                return Ok(consulta.ElementAt(0));
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
        }
        #endregion

        #region INTEGRACIÓN CONFIRMACIONES CSV (27)
        [HttpPost]
        [Route("integracion_confirmaciones_csv/LISTAR")]
        public IHttpActionResult recurso28(API_REQUEST_TYPE_22 REQUEST)
        {
            API_REPONSE_TYPE_18 RESPONSE = new API_REPONSE_TYPE_18();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region VALIDA EMPRESA NOK
            if (!ACCESS.VALIDATE_USUARIO_EMPRESA(REQUEST.EmpId, USERNAME, out ERROR))
            {
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
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
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);

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
                                                      Items = from Items in model
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
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
        }
        #endregion

        #region INTEGRACIÓN CONFIRMACIONES UPD (28)
        [HttpPut]
        [Route("integracion_confirmaciones_upd")]
        public IHttpActionResult recurso29(API_REQUEST_TYPE_23 REQUEST)
        {
            API_REPONSE_TYPE_18 RESPONSE = new API_REPONSE_TYPE_18();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion

            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            #region VALIDACIONES DE CAMPOS
            #endregion
            try
            {
                var model = GP_ENT.sp_upd_API_IntegraConfirmaciones(REQUEST.IntId, USERNAME).ToList();
                var aux = model.Count();
                if (model.Count() == 0)
                {
                    RESPONSE.Count = 0;
                    RESPONSE.Resultado = "Vacío";
                    RESPONSE.Resultado_Codigo = 0;
                    RESPONSE.Resultado_Descripcion = "No se han encontrado registros";
                    STATUS_CODE = HttpStatusCode.Unauthorized;
                    return new HttpActionResult(Request, STATUS_CODE, RESPONSE);

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
                               };
                return Ok(consulta.ElementAt(0));
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado_Codigo = ERROR.ErrID;
                RESPONSE.Resultado_Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
            }
            #endregion
        }
        #endregion

        #region BSALE WEBHOOK (29)
        [HttpPost]
        [Route("WebhookBsale")]
        //public IHttpActionResult recurso26(string data)
        public IHttpActionResult recurso26(object data)
        {

            API_REPONSE_TYPE_17 RESPONSE = new API_REPONSE_TYPE_17();
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
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            }
            #endregion
            #region RESPONSE OK
            STATUS_CODE = HttpStatusCode.OK;
            try
            {
                ///fun.log(Convert.ToString(data));

                List<sp_in_API_Webhook_Bsale_Result> WBSL = GP_ENT.sp_in_API_Webhook_Bsale(
                data.ToString()).ToList();

                RESPONSE.Resultado = WBSL.ElementAt(0).Resultado;
                RESPONSE.Descripcion = WBSL.ElementAt(0).Descripcion;
            }
            catch (Exception ex)
            {
                #region NOK
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000);//REQUEST - ERROR NO ESPECIFICADO
                RESPONSE.Resultado = "ERROR";
                RESPONSE.Descripcion = ERROR.Mensaje;
                STATUS_CODE = HttpStatusCode.Unauthorized;
                return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
                #endregion
                //}
            }
            return new HttpActionResult(Request, STATUS_CODE, RESPONSE);
            #endregion
        }
        #endregion
    }
}