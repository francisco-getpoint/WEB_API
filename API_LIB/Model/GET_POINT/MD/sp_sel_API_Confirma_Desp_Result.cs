using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace API_LIB.Model.GET_POINT.MD
{
    [DataContract]
    public partial class sp_sel_API_Confirma_Desp_Result_MODEL
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public Nullable<int> ColaPickId { get; set; }

        [DataMember(Order = 3)]
        public string INT_NAME { get; set; }

        [DataMember(Order = 4)]
        public string FECHA_HORA { get; set; }

        [DataMember(Order = 5)]
        public long SolDespId { get; set; }

        [DataMember(Order = 6)]
        public string FechaProceso { get; set; }

        [DataMember(Order = 7)]
        public int TipoDocumento { get; set; }

        [DataMember(Order = 8)]
        public string NumeroDocto { get; set; }

        [DataMember(Order = 9)]
        public string FechaDocto { get; set; }

        [DataMember(Order = 10)]
        public string TipoReferencia { get; set; }

        [DataMember(Order = 11)]
        public string NumeroReferencia { get; set; }

        [DataMember(Order = 12)]
        public string FechaReferencia { get; set; }

        [DataMember(Order = 13)]
        public string RutCliente { get; set; }

        //[DataMember(Order = 20)]
        //public Nullable<int> Counter { get; set; }

        [DataMember(Order = 99)]
        public List<sp_sel_API_Confirma_Desp_Result_MODEL_DET> Items = new List<sp_sel_API_Confirma_Desp_Result_MODEL_DET>();

    }
    public partial class sp_sel_API_Confirma_Desp_Result_MODEL_DET
    {
        public int Linea { get; set; }
        public string CodigoArticulo { get; set; }
        public string CodigoOriginal { get; set; }
        public string UnidadVenta { get; set; }
        public decimal CantidadSolicitada { get; set; }
        public int ItemReferencia { get; set; }
        public string LoteSerieSol { get; set; }
        public string FecVenctoSol { get; set; }
        public decimal CantidadDespachada { get; set; }
        public string LoteSerieDesp { get; set; }
        public string FecVectoDesp { get; set; }
    }
}
