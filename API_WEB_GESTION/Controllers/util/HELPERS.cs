using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API_WEB_GESTION.Controllers.util
{
   
    public static class HELPERS
    {
        public static void TrimModelProperties(Type type, object obj)
        {
            var propertyInfoArray = type.GetProperties(
                                            BindingFlags.Public |
                                            BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfoArray)
            {
                var propValue = propertyInfo.GetValue(obj, null);
                if (propValue == null)
                    continue;
                if (propValue.GetType().Name == "String")
                    propertyInfo.SetValue(
                                     obj,
                                     ((string)propValue).Trim(),
                                     null);
            }
        }
    }


    }