using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_LIB.Model.API.API_CLS
{

    [MetadataType(typeof(API_PROY_DETMD))]
    public partial class API_PROY_DET
    {
        internal sealed class API_PROY_DETMD
        {
            public long ProyID { get; set; }
            public int DetID { get; set; }

            [Required(ErrorMessage = "Ingresa Nombre")]
            public string Nombre { get; set; }
            public string Descrip { get; set; }
            public string URL { get; set; }
            public long HAtrID { get; set; }
            public long CTAtrID { get; set; }
            public string Request { get; set; }
            public string Response { get; set; }
            public int Estado { get; set; }
            public string UsuarioDIG { get; set; }
            public System.DateTime FechaDIG { get; set; }
        
        }
    }
}
