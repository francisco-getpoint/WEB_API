﻿using System.Web;
using System.Web.Mvc;

namespace API_GP_LOGISTICO
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
