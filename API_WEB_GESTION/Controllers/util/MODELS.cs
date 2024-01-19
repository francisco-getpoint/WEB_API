using API_LIB.Model.API.API_CLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_WEB_GESTION.Controllers.util
{
    public static class MODELS
    {
        public partial class BASE_ERRORS
        {
            public Exception EX { get; set; }
            public string PARAMS { get; set; }
            public string MODULE { get; set; }

            public BASE_ERRORS(Exception EX, string PARAMS, string MODULE)
            {
                this.EX = EX;
                this.PARAMS = PARAMS;
                this.MODULE = MODULE;
            }
        }

        public static List<API_PROF_ACCESOS> GET_LIST_API_PROF_ACCESOS()
        {
            List<API_PROF_ACCESOS> response = new List<API_PROF_ACCESOS>();
            API_CLS API_CLS = new API_CLS();
            response = API_CLS.API_PROF_ACCESOS.OrderBy(m => m.AccOrden).ToList();
            API_CLS.Dispose();
            return response;
        }
    }
}