using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using static API_GP_LOGISTICO.Controllers.util.MODELS;
using API_LIB.Model.API.API_CLS;
using API_LIB.Model.GET_POINT.GP_CLS;
using System.Data.Entity;

namespace API_GP_LOGISTICO.Controllers.util
{
    /// <summary>
    /// SE VALIDA HEADERS TOKEN Y SECRET VALIDOS Y SE OBTIENE USERNAME, METODO VALIDATE_HEADERS
    /// SE VALIDA SI EL USUARIO TIENE ACCESO A LA API o API/RECURSO, METODO VALIDATE_USUARIO_API
    /// </summary>
    public class ACCESS
    {
        public bool RESOLVE_ACCESS(HttpRequestMessage REQUEST,CONFIGURATION CONFIGURATION, out string USERNAME,out API_RESPONSE_ERRORS ERROR)
        {
            bool response = false;           
            ERROR = new API_RESPONSE_ERRORS();
            USERNAME = "";

            if (VALIDATE_HEADERS(REQUEST,out ERROR, out USERNAME))//SE VALIDA HEADERS 
            {
                if (VALIDATE_USUARIO_API(CONFIGURATION, USERNAME,out ERROR)) {
                    response = true;
                }
            }
            return response;
        }
        private bool VALIDATE_HEADERS(HttpRequestMessage request, out API_RESPONSE_ERRORS ERROR, out string USERNAME)
        {
            bool response = false;
            API_CLS API_CLS = new API_CLS();
            ERROR = new API_RESPONSE_ERRORS();
            USERNAME = "";
            string TOKEN = "";
            string SECRET = "";

            try
            {
                try 
                { 
                    TOKEN = request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_TOKEN.ToUpper()).Value.FirstOrDefault(); 
                }
                catch (Exception ex) 
                { 
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1001);
                    return false; 
                }   //REQUEST - SIN TOKEN

                try 
                { 
                    SECRET = request.Headers.First(header => header.Key.ToUpper() == VARS.VARS_HEADER_SECRET.ToUpper()).Value.FirstOrDefault(); 
                }
                catch (Exception ex) 
                { 
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1002); 
                    return false; 
                } //REQUEST - SIN SECRET

                API_USUARIOS API_USUARIOS = new API_USUARIOS();
                try
                {
                    API_USUARIOS = API_CLS.API_USUARIOS.Where(m => m.API_TOKEN == TOKEN && m.API_SECRET == SECRET).FirstOrDefault();
                    if (API_USUARIOS == null){throw new Exception();}
                    USERNAME = API_USUARIOS.Username.ToUpper();               
                }
                catch (Exception ex) 
                { 
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1003); 
                    return false; 
                }     //REQUEST - TOKEN o SECRET NO VALIDO

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
                                response = true;
                                return response;
                            }
                            else 
                            { 
                                throw new Exception(); 
                            }
                        }
                        else 
                        { 
                            throw new Exception(); 
                        }
                    }
                    else //VALIDA SOLO ESTADO
                    {
                        if (API_USUARIOS.Estado == 1)
                        {
                            response = true;
                            return response;
                        }
                        else 
                        { 
                            throw new Exception(); 
                        }     
                    }
                }
                catch (Exception ex) 
                { 
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1014); 
                    return false; 
                }     //REQUEST - TOKEN o SECRET NO VALIDO
               
                //API_CLS.Dispose();
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); 
                return false;//REQUEST - ERROR NO ESPECIFICADO
            }

            //return response;
        }

        private bool VALIDATE_USUARIO_API(CONFIGURATION CONFIGURATION, string USERNAME, out API_RESPONSE_ERRORS ERROR)
        {
            bool response = false;
            API_CLS API_CLS = new API_CLS();
            ERROR = new API_RESPONSE_ERRORS();
            try
            {
                try {
                    API_USUARIOS_PROY API_USUARIOS_PROY = API_CLS.API_USUARIOS_PROY
                    .Where(m => 
                    m.Username == USERNAME && 
                    m.ProyID== CONFIGURATION.PROY_ID)
                    .FirstOrDefault();
                    if (API_USUARIOS_PROY == null) { throw new Exception(); }
                }
                catch (Exception ex) { ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1010); return false; }   //REQUEST - API NO ASIGNADA

                try
                {
                    API_USUARIOS_PROY_DET API_USUARIOS_PROY_DET = API_CLS.API_USUARIOS_PROY_DET
                     .Where(m =>
                     m.Username == USERNAME &&
                     m.ProyID == CONFIGURATION.PROY_ID && 
                     m.DetID==CONFIGURATION.RESOURCES_ID
                     )
                     .FirstOrDefault();
                    if (API_USUARIOS_PROY_DET == null) { throw new Exception(); }
                    response = true;
                }
                catch (Exception ex) { ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1011); return false; }   //REQUEST - API/RECURSO NO ASIGNADO

                API_CLS.Dispose();
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); return false;//REQUEST - ERROR NO ESPECIFICADO
            }
            return response;
        }


        public bool VALIDATE_USUARIO_EMPRESA(int EMPID, string USERNAME, out API_RESPONSE_ERRORS ERROR)
        {
            //bool response = false;
            API_CLS API_CLS = new API_CLS();
            GP_CLS GP_CLS = new GP_CLS();
            ERROR = new API_RESPONSE_ERRORS();
            try
            {
                try
                {
                    if (GP_CLS.UsuarioEmpresa.Where(m => m.EmpId == EMPID && m.UserName.Trim().ToUpper() == USERNAME.Trim().ToUpper()).Count() > 0){
                        return true;
                    }
                    else 
                    {
                        ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1012); 
                        return false;//REQUEST - EMPRESA NO ASIGNADA
                    }
                }
                catch (Exception ex) 
                { 
                    ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1012); 
                    return false; 
                }  //REQUEST - EMPRESA NO ASIGNADA

                //API_CLS.Dispose();
                //GP_CLS.Dispose();
            }
            catch (Exception ex)
            {
                ERROR = API_CLS.API_RESPONSE_ERRORS.Find(1000); 
                return false;//REQUEST - ERROR NO ESPECIFICADO
            }
            //return response;
        }
    }
}