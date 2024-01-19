using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace API_GP_LOGISTICO
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "API/{controller}/{action}/{id}",
                  defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional }
            );
        }
    }
}
