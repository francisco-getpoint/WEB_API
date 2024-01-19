using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace API_GP_LOGISTICO.Controllers.util
{
    public static class CONFIGS
    {

        public static long APP_KEY_API_ID = int.Parse(ConfigurationManager.AppSettings["API_ID"]);
        public static string APP_KEY_API_VERSION = ConfigurationManager.AppSettings["API_VERSION"];
    }
}