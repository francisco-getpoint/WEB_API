using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_LIB.Model.API.API_CLS
{
    [MetadataType(typeof(API_RESPONSE_ERRORSMD))]
    public partial class API_RESPONSE_ERRORS
    {
        internal sealed class API_RESPONSE_ERRORSMD
        {
            [Required(ErrorMessage = "Ingresa Código"), Range(1, long.MaxValue, ErrorMessage = "Código no valido")]
            public long ErrID { get; set; }

            [Required(ErrorMessage = "Ingresa Nombre")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Ingresa Mensaje")]
            public string Mensaje { get; set; }
            public bool IndTipo { get; set; }
            public byte Estado { get; set; }
        }
    }
}
