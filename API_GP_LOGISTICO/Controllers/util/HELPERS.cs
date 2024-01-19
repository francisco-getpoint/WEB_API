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

namespace API_GP_LOGISTICO.Controllers.util
{
    public class StringValueAttribute : System.Attribute
    {
        private string _value;
        public StringValueAttribute(string value)
        {
            _value = value;
        }
        public string Value
        {
            get { return _value; }
        }
    }
    public class HttpActionResult : IHttpActionResult
    {
        public HttpActionResult(HttpRequestMessage request) : this(request, HttpStatusCode.OK)
        {
        }
        public HttpActionResult(HttpRequestMessage request, HttpStatusCode code) : this(request, code, null)
        {
        }
        public HttpActionResult(HttpRequestMessage request, HttpStatusCode code, object result)
        {
            Request = request;
            Code = code;
            Result = result;
        }
        public HttpRequestMessage Request { get; }
        public HttpStatusCode Code { get; }
        public object Result { get; }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Request.CreateResponse(Code, Result));
        }
    }

    public class HELPERS
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