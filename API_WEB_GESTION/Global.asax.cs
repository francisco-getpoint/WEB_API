using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace API_WEB_GESTION
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 404)
            {
                Response.Clear();
                var rd = new RouteData();
                rd.DataTokens["area"] = "AreaName";
                rd.Values["controller"] = "LS";
                rd.Values["action"] = "ERR_404";
                IController c = new API_WEB_GESTION.Controllers.util_base.LSController();
                c.Execute(new RequestContext(new HttpContextWrapper(Context), rd));
            }

        }
    }
}
