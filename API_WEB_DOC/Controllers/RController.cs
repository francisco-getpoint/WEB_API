using API_LIB.Model.API.API_CLS;
using API_WEB_DOC.Controllers.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace API_WEB_DOC.Controllers
{
    public class R_PARAMS
    {
        public List<Tuple<long, long>> Items { get; set; }
        public R_PARAMS()
        {
            Items = new List<Tuple<long, long>>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in Items)
            {
                sb.Append(a.Item1 + ", " + "\r\n");
            }
            return sb.ToString();
        }
    }
    public partial class R_MODEL
    {
        public API_PROY_CAB API_PROY_CAB { get; set; }
        public API_PROY_DET API_PROY_DET { get; set; }
        public List<API_ATTR_HTTP> API_ATTR_HTTP { get; set; }
        public List<API_ATTR_CONT_TYPE> API_ATTR_CONT_TYPE { get; set; }
    }
    public class RController : Controller
    {
        API_CLS API_CLS = new API_CLS();
        public string GLOBAL_SESSION = "GLOBAL_R";

        public ActionResult I()
        {
            try
            {
                R_PARAMS PARAMS = (R_PARAMS)Session[GLOBAL_SESSION];
                long ID = PARAMS.Items.ElementAt(0).Item1;
                long ID2 = PARAMS.Items.ElementAt(0).Item2;

                R_MODEL R_MODEL = new R_MODEL();
                API_PROY_CAB API_PROY_CAB = API_CLS.API_PROY_CAB.Find(ID);
                API_PROY_DET API_PROY_DET = API_CLS.API_PROY_DET.Find(ID, ID2);
                List<API_ATTR_HTTP> API_ATTR_HTTP = API_CLS.API_ATTR_HTTP.ToList();
                List<API_ATTR_CONT_TYPE> API_ATTR_CONT_TYPE = API_CLS.API_ATTR_CONT_TYPE.ToList();

                R_MODEL.API_PROY_CAB = API_PROY_CAB;
                R_MODEL.API_PROY_DET = API_PROY_DET;
                R_MODEL.API_ATTR_HTTP = API_ATTR_HTTP;
                R_MODEL.API_ATTR_CONT_TYPE = API_ATTR_CONT_TYPE;

                return View(R_MODEL);
            }
            catch (Exception ex)
            {
                Session[VARS.VARS_SESSION_ERR] = new MODELS.BASE_ERRORS(ex, "", Request.Url.AbsoluteUri);
                return RedirectToAction("I", "LS");
            }
            //return RedirectToAction("I", "TO");
        }
    }
}