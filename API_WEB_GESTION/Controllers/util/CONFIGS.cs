using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace API_WEB_GESTION.Controllers.util
{
    public static class CONFIGS
    {

        public static string APP_KEY_PHRASE = ConfigurationManager.AppSettings["APP_KEY_PHRASE"];
    }
}