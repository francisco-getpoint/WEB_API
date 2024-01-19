using API_LIB.Model.GET_POINT.GP_CLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_WEB_DOC.Controllers.util
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

        public partial class BASE_LOGIN
        {
            public Empresa EMPRESA { get; set; }
            public Usuario USUARIO { get; set; }
        }
    }
}