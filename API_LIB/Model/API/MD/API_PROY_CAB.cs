using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_LIB.Model.API.API_CLS
{
    [MetadataType(typeof(API_PROY_CABMD))]
    public partial class API_PROY_CAB
    {
        internal sealed class API_PROY_CABMD
        {
            public long ProyID { get; set; }

            [Required(ErrorMessage = "Ingresa Nombre")]
            public string Nombre { get; set; }

            public string Descrip { get; set; }
            public string URL { get; set; }
            public string UsuarioDIG { get; set; }
            public System.DateTime FechaDIG { get; set; }
            public int Estado { get; set; }
        }
    }
}
