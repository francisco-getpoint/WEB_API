using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace API_GP_LOGISTICO.Controllers.util
{
    public class MODELS
    {
        #region CONFIG
        public class CONFIGURATION
        {
            public long PROY_ID = CONFIGS.APP_KEY_API_ID;
            public long RESOURCES_ID = 0;
        }
        public enum CONFIG_RESOURCES : long
        {
            RESO_1 = 1,
            RESO_2 = 2,
            RESO_3 = 3,
            RESO_4 = 4,
            RESO_5 = 5,
            RESO_6 = 6,
            RESO_7 = 7,
            RESO_8 = 8,
            RESO_9 = 9,
            RESO_10 = 10,
            RESO_11 = 11,
            RESO_12 = 12,
            RESO_13 = 13,
            RESO_14 = 14,
            RESO_15 = 15,
            RESO_16 = 16,
            RESO_17 = 17,
            RESO_18 = 18,
            RESO_19 = 19,
            RESO_20 = 20,
        }
        #endregion

        #region REQUESTS MODELS
        public class API_REQUEST_TYPE_1
        {
            public int limit { get; set; }
            public int rowset { get; set; }
        }
        public class API_REQUEST_TYPE_2_PICKUP
        {
            public int empid { get; set; }
            public string numeropedido { get; set; }
            public DateTime fechacompra { get; set; }
            public string rutcliente { get; set; }
            public string nombrecliente { get; set; }
            public string email { get; set; }
            public int tipodocumento { get; set; }
            public int numerodocto { get; set; }
            public DateTime fechadocto { get; set; }
            public double montocompra { get; set; }
            public string ruttercero { get; set; }
            public string nombretercero { get; set; }
            public string emailtercero { get; set; }
            public string usuariodig { get; set; }
            [JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)]
            public string direccion { get; set; }
            [JsonProperty("comuna", NullValueHandling = NullValueHandling.Ignore)]
            public string comuna { get; set; }
            [JsonProperty("telefono", NullValueHandling = NullValueHandling.Ignore)]
            public string telefono { get; set; }
            [JsonProperty("glosa", NullValueHandling = NullValueHandling.Ignore)]
            public string glosa { get; set; }
            [JsonProperty("tiposolicitud", NullValueHandling = NullValueHandling.Ignore)]
            public int? tiposolicitud { get; set; }
            public List<API_REQUEST_TYPE_2_PICKUP_ITEM> items { get; set; }
        }
        public class API_REQUEST_TYPE_2_PICKUP_ITEM
        {
            public int linea { get; set; }
            public string codigoarticulo { get; set; }
            public string descripart { get; set; }
            public int cantidad { get; set; }
            public decimal? volumen { get; set; }
            public decimal? peso { get; set; }
            public int HUId { get; set; }
        }
        public class API_REQUEST_TYPE_3
        {
            public int empID { get; set; }
            public string fechaInicio { get; set; }
            public string fechaTermino { get; set; }
            public int solRecepID { get; set; }
            public string tipoReferencia { get; set; }
            public string numeroReferencia { get; set; }
            public string rutProveedor { get; set; }
            public int limit { get; set; }
            public int rowset { get; set; }

        }
        public class API_REQUEST_TYPE_4
        {
            public int empID { get; set; }
            public string fechaInicio { get; set; }
            public string fechaTermino { get; set; }
            public int SolDespId { get; set; }
            public string tipoReferencia { get; set; }
            public string numeroReferencia { get; set; }
            public string rutProveedor { get; set; }
            public int limit { get; set; }
            public int rowset { get; set; }

        }
        public class API_REQUEST_TYPE_5
        {
            public string key { get; set; }
            public int empid { get; set; }
            public string txt { get; set; }
        }
        public class API_REQUEST_TYPE_6
        {
            public int empid { get; set; }
            public string archivo { get; set; }
            public string txt { get; set; }
        }
        public class API_REQUEST_TYPE_7
        {
            public int empid { get; set; }
            public string codigoarticulo { get; set; }
            public string descripcion { get; set; }
            public int lineaproducto { get; set; }
            public int tipoproducto { get; set; }
            public string ean13 { get; set; }
            public string dun14 { get; set; }
            public int limit { get; set; }
            public int rowset { get; set; }
        }
        public class API_REQUEST_TYPE_8
        {
            public int empId { get; set; }
            public string fechaInicio { get; set; }
            public string fechaTermino { get; set; }
            public int solDespId { get; set; }
            public string tipoReferencia { get; set; }
            public int numeroReferencia { get; set; }
            public string rutCliente { get; set; }
            public int limit { get; set; }
            public int rowset { get; set; }
        }
        public class API_REQUEST_TYPE_9
        {
            public int EmpId { get; set; }
            public int LineaProducto { get; set; }
            public string TipoProducto { get; set; }
            public string CodigoArticulo { get; set; }
            public int CodigoBodega { get; set; }
        }
        public class API_REQUEST_TYPE_10_CAB
        {
            public int empId { get; set; }
            public int recepcionId { get; set; }
            public string intName { get; set; }
            public string fechaHora { get; set; }
            public int solRecepId { get; set; }
            public string fechaProceso { get; set; }
            public int TipoDocumento { get; set; }
            public int NumeroDocto { get; set; }
            public string fechaDocto { get; set; }
            public string TipoReferencia { get; set; }
            public int NumeroReferencia { get; set; }
            public string FechaReferencia { get; set; }
            public List<API_REQUEST_TYPE_10_DET> Detalles { get; set; }
        }
        public class API_REQUEST_TYPE_10_DET
        {
            public int Linea { get; set; }
            public string CodigoArticulo { get; set; }
            public string CodigoOriginal { get; set; }
            public string UnidadCompra { get; set; }
            public int CantidadSolicitada { get; set; }
            public int ItemReferencia { get; set; }
            public string LoteSerie { get; set; }
            public string FechaVencto { get; set; }
            public int CantRecibida { get; set; }
            public string HU { get; set; }
        }
        public class API_REQUEST_TYPE_11_CAB
        {
            public int empId { get; set; }
            public int colaPickId { get; set; }
            public string intName { get; set; }
            public string fechaHora { get; set; }
            public int solDespId { get; set; }
            public string fechaProceso { get; set; }
            public int tipoDocumento { get; set; }
            public int numeroDocto { get; set; }
            public string fechaDocto { get; set; }
            public string tipoReferencia { get; set; }
            public string numeroReferencia { get; set; }
            public string fechaReferencia { get; set; }
            public List<API_REQUEST_TYPE_11_DET> detalles { get; set; }
        }
        public class API_REQUEST_TYPE_11_DET
        {
            public int linea { get; set; }
            public string codigoArticulo { get; set; }
            public string codigoOriginal { get; set; }
            public string unidadVenta { get; set; }
            public int cantidadSolicitada { get; set; }
            public int itemReferencia { get; set; }
            public string loteSerieSol { get; set; }
            public string fechaVenctoSol { get; set; }
            public int cantidadDespachada { get; set; }
            public string loteSerieDesp { get; set; }
            public string fechaVenctoDesp { get; set; }
            public int huId { get; set; }
        }
        public class API_REQUEST_TYPE_12
        {
            public int route_id { get; set; }
            public int account_id { get; set; }
            public int user_id { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public API_REQUEST_TYPE_12_vehicle vehicle { get; set; }
            public string created_at { get; set; }
            public List<API_REQUEST_TYPE_12_stops> stops { get; set; }
            public int plan_id { get; set; }
        }
        public class API_REQUEST_TYPE_12_vehicle
        {
            public string identifier { get; set; }
        }
        public class API_REQUEST_TYPE_12_stops
        {
            public int stop_group_id { get; set; }
            public int stop_id { get; set; }
            public string identifier { get; set; }
            public string address { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public int arrival_time { get; set; }
            public int driving_period { get; set; }
            public int service_time { get; set; }
            public List<API_REQUEST_TYPE_12_delivery_times> delivery_times { get; set; }
        }
        public class API_REQUEST_TYPE_12_delivery_times
        {
            public string min_delivery_time { get; set; }
            public string max_delivery_time { get; set; }
        }

        [DataContract]
        public class API_REQUEST_TYPE_15_ConsultaStock
        {
            [DataMember(Order = 1)]
            public int EmpId { get; set; }

            [DataMember(Order = 2)]
            public int LineaProducto { get; set; }

            [DataMember(Order = 3)]
            public string TipoProducto { get; set; }

            [DataMember(Order = 4)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 5)]
            public int CodigoBodega { get; set; }

            [DataMember(Order = 99)]

            public List<API_REQUEST_TYPE_15_ConsultaStock_Det> Articulos = new List<API_REQUEST_TYPE_15_ConsultaStock_Det>();
        }

        [DataContract]
        public class API_REQUEST_TYPE_15_ConsultaStock_Det
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }
        }

        //define estructura de cabecera del JSON Solicitud Recepcion
        [DataContract]
        public class API_REQUEST_TYPE_17
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 3)]
            public int TipoSolicitud { get; set; }

            [DataMember(Order = 4)]
            public int CodigoBodega { get; set; }

            [JsonProperty("comprador", NullValueHandling = NullValueHandling.Ignore), DataMember(Order = 5)]
            public string Comprador { get; set; }

            [DataMember(Order = 6)]
            public string Proveedor { get; set; }

            [DataMember(Order = 7)]
            public string RazonSocial { get; set; }

            [DataMember(Order = 8)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 9)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 10)]
            public string FechaReferencia { get; set; }

            [DataMember(Order = 11)]
            public string Glosa { get; set; }

            [DataMember(Order = 12)]
            public string Dato1 { get; set; }

            [DataMember(Order = 13)]
            public string Dato2 { get; set; }

            [DataMember(Order = 14)]
            public string Dato3 { get; set; }

            [DataMember(Order = 15)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 17)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 18)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 19)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 20)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 21)]
            public List<API_REQUEST_TYPE_17_DETALLES> Items { get; set; }
        }

        //define estructura de items del JSON Solicitud Recepcion
        [DataContract]
        public class API_REQUEST_TYPE_17_DETALLES
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public string UnidadCompra { get; set; }

            [DataMember(Order = 3)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 4)]
            public int ItemReferencia { get; set; }

            [DataMember(Order = 5)]
            public decimal CostoUnitario { get; set; }

            [DataMember(Order = 6)]
            public decimal KilosTotales {get; set;}

            [DataMember(Order = 7)]
            public string NumeroSerie { get; set; }

            [DataMember(Order = 8)]
            public string FechaVectoLote { get; set; }

            [DataMember(Order = 9)]
            public int Estado { get; set; }

            [DataMember(Order = 10)]
            public decimal PorcQA { get; set; }

            [DataMember(Order = 11)]
            public string Dato1 { get; set; }

            [DataMember(Order = 12)]
            public string Dato2 { get; set; }

            [DataMember(Order = 13)]
            public string Dato3 { get; set; }

            [DataMember(Order = 14)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 15)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 17)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 18)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 19)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 20)]
            public int Sucursal { get; set; }

            [DataMember(Order = 99)]

            public List<API_REQUEST_TYPE_17_CROSSDOCKING> Crossdocking = new List<API_REQUEST_TYPE_17_CROSSDOCKING>();
        }

        //define estructura de detalle Crossdocking para items del JSON Solicitud Recepcion
        [DataContract]
        public class API_REQUEST_TYPE_17_CROSSDOCKING
        {
            [DataMember(Order = 1)]
            public string TipoReferenciaCD { get; set; }

            [DataMember(Order = 2)]
            public string NumeroReferenciaCD { get; set; }

            [DataMember(Order = 3)]
            public int ItemReferenciaCD { get; set; }

            [DataMember(Order = 4)]
            public decimal CantidadCD { get; set; }
        }

        //define estructura de cabecera del JSON Solicitud Despacho
        [DataContract]
        public class API_REQUEST_TYPE_18
        {
            [DataMember(Order = 1)]
            public string Proceso { get; set; }

            [DataMember(Order = 2)]
            public int Empid { get; set; }

            [DataMember(Order = 3)]
            public int TipoSolicitud { get; set; }

            [DataMember(Order = 4)]
            public int CodigoBodega { get; set; }

            [DataMember(Order = 5)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 6)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 7)]
            public string FechaReferencia { get; set; }

            [DataMember(Order = 8)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 9)]
            public int TipoDocumento { get; set; }

            [DataMember(Order = 10)]
            public string NumeroDocto { get; set; }

            [DataMember(Order = 11)]
            public string FechaDocto { get; set; }

            [DataMember(Order = 12)]
            public string Glosa { get; set; }

            [DataMember(Order = 13)]
            public string Cliente { get; set; }

            [DataMember(Order = 14)]
            public string RazonSocial { get; set; }

            [DataMember(Order = 15)]
            public string Telefono { get; set; }

            [DataMember(Order = 16)]
            public string Email { get; set; }

            [DataMember(Order = 17)]
            public string Direccion { get; set; }

            [DataMember(Order = 18)]
            public string Contacto { get; set; }

            [DataMember(Order = 19)]
            public int RutaDespacho { get; set; }

            [DataMember(Order = 20)]
            public string Region { get; set; }

            [DataMember(Order = 21)]
            public string Comuna { get; set; }

            [DataMember(Order = 22)]
            public string Ciudad { get; set; }

            [DataMember(Order = 23)]
            public string Vendedor { get; set; }

            [DataMember(Order = 24)]
            public string Comprador { get; set; }

            [DataMember(Order = 25)]
            public string Dato1 { get; set; }

            [DataMember(Order = 26)]
            public string Dato2 { get; set; }

            [DataMember(Order = 27)]
            public string Dato3 { get; set; }

            [DataMember(Order = 28)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 29)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 30)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 31)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 32)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 33)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 34)]
            public int Sucursal { get; set; }

            [DataMember(Order = 35)]
            public List<API_REQUEST_TYPE_18_DETALLES> Items { get; set; }
        }

        //define estructura de items del JSON Solicitud Despacho
        [DataContract]
        public class API_REQUEST_TYPE_18_DETALLES
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public string UnidadVenta { get; set; }

            [DataMember(Order = 3)]
            public string NumeroSerie { get; set; }

            [DataMember(Order = 4)]
            public string FechaVectoLote { get; set; }

            [DataMember(Order = 5)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 6)]
            public int Estado { get; set; }

            [DataMember(Order = 7)]
            public decimal CostoUnitario { get; set; }

            [DataMember(Order = 8)]
            public decimal KilosTotales { get; set; }

            [DataMember(Order = 9)]
            public decimal PorcQa { get; set; }

            [DataMember(Order = 10)]
            public int Maquila { get; set; }

            [DataMember(Order = 11)]
            public string Pallet { get; set; }

            [DataMember(Order = 12)]
            public int ItemReferencia { get; set; }

            [DataMember(Order = 13)]
            public string Dato1 { get; set; }

            [DataMember(Order = 14)]
            public string Dato2 { get; set; }

            [DataMember(Order = 15)]
            public string Dato3 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 17)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 18)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 19)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 20)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 21)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 22)]
            public int Sucursal { get; set; }
        }

        public class API_REQUEST_TYPE_15
        {
            public int empid { get; set; }
            public string nombreArchivo { get; set; }
            public string imgBase64 { get; set; }
        }
        public class API_REQUEST_TYPE_16
        {
            public int empId { get; set; }
            public string userName { get; set; }
            public string fechaInicio { get; set; }
            public string fechaTermino { get; set; }
            public int estado { get; set; }
            public int numeroRef { get; set; }
            public int numeroDoc { get; set; }
            public string rutCliente { get; set; }
        }
        public class API_REQUEST_TYPE_19
        {
            public int empId { get; set; }
            public int numeroReferencia { get; set; }
            public int sdr { get; set; }
            public int estado { get; set; }
        }
        public class API_REQUEST_TYPE_20
        {
            public int empId { get; set; }
            public int lineaProducto { get; set; }
            public string tipoProducto { get; set; }
            public string CodigoArticulo { get; set; }
        }
        public class API_REQUEST_TYPE_21
        {
            public int cpnId { get; set; }
            public string resource { get; set; }
            public string resourceId { get; set; }
            public string topic { get; set; }
            public string action { get; set; }
            public string officeId { get; set; }
        }
        public class API_REQUEST_TYPE_22
        {
            public int solDespId { get; set; }
            public string archivo { get; set; }
        }
        public class API_REQUEST_TYPE_23
        {
            public int empId { get; set; }
            public int numeroReferencia { get; set; }
            public int sdd { get; set; }
            public int estado { get; set; }
        }
        public class API_REQUEST_TYPE_27
        {
            public int EmpId { get; set; }
            public string NombreProceso { get; set; }
            public int Limit { get; set; }
            public int RowSet { get; set; }
        }
        public class API_REQUEST_TYPE_29
        {
            public int IntId { get; set; }
        }
        public class API_REQUEST_TYPE_30
        {
            public int EmpId { get; set; }
            public List<API_REQUEST_TYPE_30_DETALLES> Articulos { get; set; }
        }

        [DataContract]
        public class API_REQUEST_TYPE_30_DETALLES
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public string DescripArt { get; set; }

            [DataMember(Order = 3)]
            public string DescripTecnica { get; set; }

            [DataMember(Order = 4)]
            public string DescripCorta { get; set; }

            [DataMember(Order = 5)]
            public int LineaProducto { get; set; }

            //L_Articulos -> TipoModelo = Tipo Producto

            [DataMember(Order = 6)]
            public string TipoProducto { get; set; }

            //L_Articulos -> Tipo = Tipo Articulo

            [DataMember(Order = 7)]
            public int TipoArticulo { get; set; }

            [DataMember(Order = 8)]
            public string VigenciaDesde { get; set; }

            [DataMember(Order = 9)]
            public string VigenciaHasta { get; set; }

            [DataMember(Order = 10)]
            public string Rotacion { get; set; }

            [DataMember(Order = 11)]
            public string CodigoFabrica { get; set; }

            [DataMember(Order = 12)]
            public int UsaSerie { get; set; }

            [DataMember(Order = 13)]
            public int UsaLote { get; set; }

            [DataMember(Order = 14)]
            public string EAN13 { get; set; }

            [DataMember(Order = 15)]
            public string DUN14 { get; set; }

            [DataMember(Order = 16)]
            public string UnidadMedidaCompra { get; set; }

            [DataMember(Order = 17)]
            public string UnidadMedidaVenta { get; set; }

            [DataMember(Order = 18)]
            public string CodigoProveedor { get; set; }

            [DataMember(Order = 19)]
            public string Marca { get; set; }

            [DataMember(Order = 20)]
            public string CodigoExt { get; set; }

            [DataMember(Order = 21)]
            public string GlosaLineaProducto { get; set; }

            [DataMember(Order = 22)]
            public string Origen {  get; set; }

            [DataMember(Order = 23)]
            public string Dato1 { get; set; }

            [DataMember(Order = 24)]
            public string Dato2 { get; set; }

            [DataMember(Order = 25)]
            public string Dato3 { get; set; }

            [DataMember(Order = 26)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 27)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 28)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 29)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 30)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 31)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 98)]
            public List<API_REQUEST_TYPE_30_DETALLES_UM> UnidadMedida { get; set; }

            [DataMember(Order = 99)]
            public List<API_REQUEST_TYPE_30_DETALLES_KIT> Kit = new List<API_REQUEST_TYPE_30_DETALLES_KIT>();

            //campos L_Articulos
            //CodigoDig int	4
            //Version char	20
            //Origen char	20
            //Rotacion char	10
            //CodigoFabrica char	20
            //Marca char	20
            //UsaSerie tinyint	1
            //UsaLote tinyint	1
            //EAN13 char	20
            //DUN14 char	20
            //FotoChica varchar	100
            //FotoGrande varchar	100
            //Foto3 varchar	100
            //Foto4 varchar	100
            //FechaCreacion datetime	8
            //Usuario char	15
            //FechaAct datetime	8
            //UsuarioAct char	15
            //FechaAprob datetime	8
            //UsuarioAprob char	15
            //Fecha1 datetime	8
            //Fecha2 datetime	8
            //Fecha3 datetime	8
            //Dato1 varchar	250
            //Dato2 varchar	250
            //Dato3 varchar	250
            //Valor1 money	8
            //Valor2 money	8
            //Valor3 money	8
            //UnidadMedidaCompra char	10
            //UnidadMedidaVenta char	10
            //CodigoProveedor char	20
            //BodegaDefecto int	4
            //UbicacionDefecto char	6
            //RegistroSanitario char	20
            //IndicadorCertif tinyint	1
            //CodigoBarra char	50
        }

        [DataContract]
        public class API_REQUEST_TYPE_30_DETALLES_UM
        {
            [DataMember(Order = 1)]
            public string CodigoUM { get; set; }

            [DataMember(Order = 2)]
            public decimal Factor { get; set; }

            [DataMember(Order = 3)]
            public decimal PesoNeto { get; set; }

            [DataMember(Order = 4)]
            public decimal PesoBruto { get; set; }

            [DataMember(Order = 5)]
            public decimal Ancho { get; set; }

            [DataMember(Order = 6)]
            public decimal Alto { get; set; }

            [DataMember(Order = 7)]
            public decimal Profundidad { get; set; }

            [DataMember(Order = 8)]
            public decimal Volumen { get; set; }

            [DataMember(Order = 9)]
            public int UNBase { get; set; }

            [DataMember(Order = 10)]
            public decimal UNTotal { get; set; }

            [DataMember(Order = 11)]
            public int TiempoRecep { get; set; }

            [DataMember(Order = 12)]
            public int TiempoDesp { get; set; }
        }

        [DataContract]
        public class API_REQUEST_TYPE_30_DETALLES_KIT
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public decimal CantidadRequerida { get; set; }
        }
        public class API_REQUEST_TYPE_31
        {
            public int EmpId { get; set; }
            public string CodigoArticulo { get; set; }
            public string DescripArt { get; set; }
            public int Limit { get; set; }
            public int Rowset { get; set; }
        }
        public class API_REQUEST_TYPE_33
        {
            public int EmpId { get; set; }
            public string CodigoArticulo { get; set; }
        }
        public class API_REQUEST_TYPE_34
        {
            public int EmpId { get; set; }
            public string CodigoArticulo { get; set; }
            public int Limit { get; set; }
            public int Rowset { get; set; }
        }

        //Cabecera estructura Asigna Documento a Despacho
        public class API_REQUEST_TYPE_35
        {
            public int EmpId { get; set; }
            public string Usuario { get; set; }
            public string TipoReferencia { get; set; }
            public string NumeroReferencia { get; set; }
            public int ColaPickId { get; set; }
            public int TipoDocumento { get; set; }
            public string NumeroDocto { get; set; }
            public decimal Monto { get; set; }

            public List<API_REQUEST_TYPE_35_DOCUMENTOS> Documentos = new List<API_REQUEST_TYPE_35_DOCUMENTOS>();
        }

        //Detalle de Documentos asociados Asigna Documento a Despacho

        public class API_REQUEST_TYPE_35_DOCUMENTOS
        {
            public int TipoDocumento { get; set; }
            public string NumeroDocto { get; set; }
            public decimal Monto { get; set; }
        }

        //define estructura de cabecera del JSON Asocia OC con SDR TOPAS para toyota
        public class API_REQUEST_TYPE_36
        {
            public int Empid { get; set; }
            public string FechaProceso { get; set; }
            public int TipoSolicitud { get; set; }
            public int CodigoBodega { get; set; }

            [JsonProperty("comprador", NullValueHandling = NullValueHandling.Ignore)]
            public string Comprador { get; set; }
            public string Proveedor { get; set; }
            public string RazonSocial { get; set; }
            public string TipoReferencia { get; set; }
            public string NumeroReferencia { get; set; }
            public string FechaReferencia { get; set; }
            public string TipoReferencia2 { get; set; }
            public string NumeroReferencia2 { get; set; }
            public string FechaReferencia2 { get; set; }
            public string Glosa { get; set; }
            public string Dato1 { get; set; }
            public string Dato2 { get; set; }
            public string Dato3 { get; set; }
            public List<API_REQUEST_TYPE_36_DETALLES> Items { get; set; }
        }

        //define estructura de Items del JSON Asocia OC con SDR TOPAS para toyota
        public class API_REQUEST_TYPE_36_DETALLES
        {
            public string CodigoArticulo { get; set; }
            public string UnidadCompra { get; set; }
            public decimal Cantidad { get; set; }
            public int ItemReferencia { get; set; }
            public int ItemReferencia2 { get; set; }
        }

        //define estructura de cabecera del JSON Solicitud ASN
        [DataContract]
        public class API_REQUEST_TYPE_38
        {
            [DataMember(Order = 1)]
            public string Proceso { get; set; }

            [DataMember(Order = 2)]
            public int Empid { get; set; }

            [DataMember(Order = 3)]
            public string Usuario { get; set; }

            [DataMember(Order = 4)]
            public string Origen { get; set; }

            [DataMember(Order = 5)]
            public string RutProveedor { get; set; }

            [DataMember(Order = 6)]
            public string ASN_CodigoBarraExt { get; set; }

            [DataMember(Order = 7)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 8)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 9)]
            public string Patente { get; set; }

            [DataMember(Order = 10)]
            public string RutChofer { get; set; }

            [DataMember(Order = 11)]
            public string NombreChofer { get; set; }

            [DataMember(Order = 12)]
            public decimal CustomDecimalField_1 { get; set; }

            [DataMember(Order = 13)]
            public decimal CustomDecimalField_2 { get; set; }

            [DataMember(Order = 14)]
            public decimal CustomDecimalField_3 { get; set; }

            [DataMember(Order = 15)]
            public string CustomDateField_1 { get; set; }

            [DataMember(Order = 16)]
            public string CustomDateField_2 { get; set; }

            [DataMember(Order = 17)]
            public string CustomDateField_3 { get; set; }

            [DataMember(Order = 18)]
            public string CustomTextField_1 { get; set; }

            [DataMember(Order = 19)]
            public string CustomTextField_2 { get; set; }

            [DataMember(Order = 20)]
            public string CustomTextField_3 { get; set; }

            [DataMember(Order = 21)]
            public List<API_REQUEST_TYPE_38_DETALLES> Items { get; set; }
        }

        //define estructura de items de la Solicitus ASN
        [DataContract]
        public class API_REQUEST_TYPE_38_DETALLES
        {
            [DataMember(Order = 1)]
            public string LPN_CodigoBarraExt { get; set; }

            [DataMember(Order = 2)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 3)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 4)]
            public int TipoDocumento { get; set; }

            [DataMember(Order = 5)]
            public string NumeroDocumento { get; set; }

            [DataMember(Order = 6)]
            public string FechaDocumento { get; set; }

            [DataMember(Order = 7)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 8)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 9)]
            public decimal DetCustomDecimalField_1 { get; set; }

            [DataMember(Order = 10)]
            public decimal DetCustomDecimalField_2 { get; set; }

            [DataMember(Order = 11)]
            public decimal DetCustomDecimalField_3 { get; set; }

            [DataMember(Order = 12)]
            public string DetCustomDateField_1 { get; set; }

            [DataMember(Order = 13)]
            public string DetCustomDateField_2 { get; set; }

            [DataMember(Order = 14)]
            public string DetCustomDateField_3 { get; set; }

            [DataMember(Order = 15)]
            public string DetCustomTextField_1 { get; set; }

            [DataMember(Order = 16)]
            public string DetCustomTextField_2 { get; set; }

            [DataMember(Order = 17)]
            public string DetCustomTextField_3 { get; set; }
        }

        //define estructura de cabecera del JSON Cambio Estado
        [DataContract]
        public class API_REQUEST_TYPE_41_CambioEstado
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public string Usuario { get; set; }

            [DataMember(Order = 3)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 4)]
            public int Motivo { get; set; }

            [DataMember(Order = 5)]
            public string Glosa { get; set; }

            [DataMember(Order = 6)]
            public int EstadoProdOrig { get; set; }

            [DataMember(Order = 7)]
            public int EstadoProdDest { get; set; }

            [DataMember(Order = 8)]
            public string Certificado { get; set; }

            [DataMember(Order = 9)]
            public string FechaCertificado { get; set; }

            [DataMember(Order = 10)]
            public string Certificado2 { get; set; }

            [DataMember(Order = 11)]
            public string FechaCertificado2 { get; set; }

            [DataMember(Order = 12)]
            public int CodigoBodega { get; set; }

            [DataMember(Order = 13)]
            public string Dato1 { get; set; }

            [DataMember(Order = 14)]
            public string Dato2 { get; set; }

            [DataMember(Order = 15)]
            public string Dato3 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 17)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 18)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 19)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 20)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 21)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 99)]
            public List<API_REQUEST_TYPE_41_CambioEstado_DET> Items { get; set; }
        }

        //define estructura de items del JSON Solicitud Despacho
        [DataContract]
        public class API_REQUEST_TYPE_41_CambioEstado_DET
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 3)]
            public string NumeroLote { get; set; }

            [DataMember(Order = 4)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 5)]
            public string NuevoLote { get; set; }

            [DataMember(Order = 6)]
            public string NuevoFecVecto { get; set; }

            [DataMember(Order = 7)]
            public string Dato1 { get; set; }

            [DataMember(Order = 8)]
            public string Dato2 { get; set; }

            [DataMember(Order = 9)]
            public string Dato3 { get; set; }

            [DataMember(Order = 10)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 11)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 12)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 13)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 14)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 15)]
            public string Fecha3 { get; set; }
        }

        //JSON Evolutivo por Mes en Posiciones Cliente
        [DataContract]
        public class API_REQUEST_TYPE_42_EvolutivoMesCliente
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int Mes { get; set; }

            [DataMember(Order = 3)]
            public int Ejercicio { get; set; }

            [DataMember(Order = 4)]
            public int Limit { get; set; }

            [DataMember(Order = 5)]
            public int Rowset { get; set; }
        }

        //JSON Genera Consumo
        [DataContract]
        public class API_REQUEST_TYPE_43_GeneraConsumo
        {
            [DataMember(Order = 1)]
            public string NombreProceso { get; set; }

            [DataMember(Order = 2)]
            public int Empid { get; set; }

            [DataMember(Order = 3)]
            public int TipoTransaccion { get; set; }

            [DataMember(Order = 4)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 5)]
            public int CodigoBodega { get; set; }

            [DataMember(Order = 6)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 7)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 8)]
            public string Glosa { get; set; }

            [DataMember(Order = 99)]
            public List<API_REQUEST_TYPE_43_GeneraConsumo_Det> Items = new List<API_REQUEST_TYPE_43_GeneraConsumo_Det>();
        }

        [DataContract]
        public class API_REQUEST_TYPE_43_GeneraConsumo_Det
        {
            [DataMember(Order = 1)]
            public int Linea { get; set; }

            [DataMember(Order = 2)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 3)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 4)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 5)]
            public string Lote { get; set; }

            [DataMember(Order = 6)]
            public string FechaVencimiento { get; set; }

            [DataMember(Order = 7)]
            public int Estado { get; set; }

            [DataMember(Order = 8)]
            public string CodigoUbicacion { get; set; }
        }

        //JSON Genera Consumo
        [DataContract]
        public class API_REQUEST_TYPE_45_CreaReceta
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public string NombreReceta { get; set; }

            [DataMember(Order = 3)]
            public int TipoProduccion { get; set; }

            [DataMember(Order = 4)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 5)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 6)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 7)]
            public decimal Kilos { get; set; }

            [DataMember(Order = 8)]
            public string Version { get; set; }

            [DataMember(Order = 9)]
            public string VigenciaDesde { get; set; }

            [DataMember(Order = 10)]
            public string VigenciaHasta { get; set; }

            [DataMember(Order = 11)]
            public int BodegaDestino { get; set; }

            [DataMember(Order = 12)]
            public string UbicacionDestino { get; set; }

            [DataMember(Order = 13)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 14)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 15)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 17)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 18)]
            public decimal Valor3 { get; set; }

            [DataMember(Order = 19)]
            public string Dato1 { get; set; }

            [DataMember(Order = 20)]
            public string Dato2 { get; set; }

            [DataMember(Order = 21)]
            public string Dato3 { get; set; }

            [DataMember(Order = 99)]
            public List<API_REQUEST_TYPE_45_CreaReceta_Det> Items = new List<API_REQUEST_TYPE_45_CreaReceta_Det>();
        }

        [DataContract]
        public class API_REQUEST_TYPE_45_CreaReceta_Det
        {
            [DataMember(Order = 1)]
            public int Linea { get; set; }

            [DataMember(Order = 2)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 3)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 4)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 5)]
            public int Estado { get; set; }

            [DataMember(Order = 6)]
            public int BodegaOrigen { get; set; }

            [DataMember(Order = 7)]
            public int IndPermiteReemplazo { get; set; }

            [DataMember(Order = 8)]
            public int IndTienePicking { get; set; }

            [DataMember(Order = 9)]
            public string Dato1 { get; set; }

            [DataMember(Order = 10)]
            public string Dato2 { get; set; }

            [DataMember(Order = 11)]
            public string Dato3 { get; set; }

            [DataMember(Order = 12)]
            public string Fecha1 { get; set; }

            [DataMember(Order = 13)]
            public string Fecha2 { get; set; }

            [DataMember(Order = 14)]
            public string Fecha3 { get; set; }

            [DataMember(Order = 15)]
            public decimal Valor1 { get; set; }

            [DataMember(Order = 16)]
            public decimal Valor2 { get; set; }

            [DataMember(Order = 17)]
            public decimal Valor3 { get; set; }
        }

        //JSON Confirma Cierre de Inventario 
        [DataContract]
        public class API_REQUEST_TYPE_46_ConfirmaCierreInventario 
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int NumInventario { get; set; }

            [DataMember(Order = 3)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 4)]
            public string Usuario { get; set; }
        }

        //JSON Cambio Estado RDM
        [DataContract]
        public class API_REQUEST_TYPE_47_CambioEstadoRDM
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int RecepcionId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public int Estado { get; set; }
        }

        //JSON Actualiza Fecha Entrega SDD
        [DataContract]
        public class API_REQUEST_TYPE_48_ActualizaFechaEntregaSDD
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int SolDespId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public string FechaEntrega { get; set; }
        }

        //JSON Guarda datos L_Forecast_ProduccionNB

        [DataContract]
        public class API_REQUEST_TYPE_49_Forecast_ProduccionNB
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 99)]
            public List<API_REQUEST_TYPE_49_Forecast_ProduccionNB_Items> Items { get; set; }
        }

        [DataContract]
        public class API_REQUEST_TYPE_49_Forecast_ProduccionNB_Items
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public decimal Canal1 { get; set; }

            [DataMember(Order = 3)]
            public decimal Canal2 { get; set; }

            [DataMember(Order = 4)]
            public decimal Canal3 { get; set; }

            [DataMember(Order = 5)]
            public decimal Canal4 { get; set; }

            [DataMember(Order = 6)]
            public string Periodo { get; set; }

            [DataMember(Order = 7)]
            public string FechaDesde { get; set; }

            [DataMember(Order = 8)]
            public string FechaHasta { get; set; }

            [DataMember(Order = 9)]
            public string Usuario { get; set; }
        }

        //50 WEBHOOK

        //JSON Adjunta archivo SDR base 64 
        [DataContract]
        public class API_REQUEST_TYPE_51_AdjuntaArchivoSDR
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int SolRecepId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public string ArchivoBase64 { get; set; }

            [DataMember(Order = 6)]
            public string NombreArchivo { get; set; }
        }

        //JSON Adjunta archivo SDD base 64 
        [DataContract]
        public class API_REQUEST_TYPE_52_AdjuntaArchivoSDD
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int SolDespId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public string ArchivoBase64 { get; set; }

            [DataMember(Order = 6)]
            public string NombreArchivo { get; set; }
        }

        //JSON Anula Solicitud Traslado 
        [DataContract]
        public class API_REQUEST_TYPE_53_AnulaSolTraslado
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int SolDespId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public string FechaEntrega { get; set; }

            [DataMember(Order = 6)]
            public int Motivo { get; set; }

            [DataMember(Order = 7)]
            public string GlosaAnula { get; set; }
        }

        //JSON Anula Solicitud Despacho
        [DataContract]
        public class API_REQUEST_TYPE_54_AnulaSolDespacho
        {
            [DataMember(Order = 1)]
            public int Empid { get; set; }

            [DataMember(Order = 2)]
            public int SolDespId { get; set; }

            [DataMember(Order = 3)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 4)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 5)]
            public string FechaEntrega { get; set; }

            [DataMember(Order = 6)]
            public int Motivo { get; set; }

            [DataMember(Order = 7)]
            public string GlosaAnula { get; set; }
        }

        //JSON Webhook Ruta DRIVIN
        public class API_REQUEST_TYPE_55_WebhookRutaDRIVIN
        {
            public string vehicle { get; set; }
            public string vehicle_detail { get; set; }
            public int route_id { get; set; }
            public string route_code { get; set; }
            public string description { get; set; }
            public string deploy_date { get; set; }
            public string supplier_code { get; set; }
            public string scenario_token { get; set; }
            public string schema_code { get; set; }
            public string schema_name { get; set; }
            public string approved_at { get; set; }
            public string platform { get; set; }
            public string started_at { get; set; }
            public string fleet_sequence { get; set; }
            public string login_url { get; set; }
            public List<string> tags { get; set; }
            public API_REQUEST_TYPE_55_Driver driver { get; set; }
            public API_REQUEST_TYPE_55_Summary summary { get; set; }
            public List<API_REQUEST_TYPE_55_Trip> trips { get; set; }
        }
        public class API_REQUEST_TYPE_55_Driver
        {
            public string email { get; set; }
            public string full_name { get; set; }
            public string phone { get; set; }
        }
        public class API_REQUEST_TYPE_55_Summary
        {
            public int total_trips { get; set; }
            public int total_orders { get; set; }
            public int total_addresses { get; set; }
            public int total_distance { get; set; }
            public int total_time { get; set; }
            public int trip_number { get; set; }
        }
        public class API_REQUEST_TYPE_55_Trip
        {
            public API_REQUEST_TYPE_55_Summary summary { get; set; }
            public List<API_REQUEST_TYPE_55_Result> results { get; set; }
        }
        public class API_REQUEST_TYPE_55_Result
        {
            public string deposit { get; set; }
            public int position { get; set; }
            public int eta_mode { get; set; }
            public string eta { get; set; }
            public string eta_approved { get; set; }
            public string eta_started { get; set; }
            public API_REQUEST_TYPE_55_Stop stop { get; set; }
            public List<API_REQUEST_TYPE_55_Order> orders { get; set; }
        }
        public class API_REQUEST_TYPE_55_Stop
        {
            public string code { get; set; }
            public string name { get; set; }
            public string client { get; set; }
            public string address_type { get; set; }
            public string address { get; set; }
            public string reference { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string lat { get; set; }
            public string lng { get; set; }
            public string postal_code { get; set; }
            public int service_time { get; set; }
            public int priority { get; set; }
            public List<string> skills { get; set; }
            public List<API_REQUEST_TYPE_55_TimeWindow> time_windows { get; set; }
        }
        public class API_REQUEST_TYPE_55_TimeWindow
        {
            public string start { get; set; }
            public string end { get; set; }
        }
        public class API_REQUEST_TYPE_55_Order
        {
            public string idx { get; set; }
            public string code { get; set; }
            public string alt_code { get; set; }
            public string delivery_date { get; set; }
            public string supplier_code { get; set; }
            public string supplier_name { get; set; }
            public string client_code { get; set; }
            public string client_name { get; set; }
            public int units_1 { get; set; }
            public int units_2 { get; set; }
            public int units_3 { get; set; }
            public string custom_1 { get; set; }
            public string custom_2 { get; set; }
            public string custom_3 { get; set; }
            public string custom_4 { get; set; }
            public string custom_5 { get; set; }
            public string custom_6 { get; set; }
            public string custom_7 { get; set; }
            public string custom_8 { get; set; }
            public string custom_9 { get; set; }
            public string custom_10 { get; set; }
            public string custom_11 { get; set; }
            public string number_1 { get; set; }
            public string number_2 { get; set; }
            public string number_3 { get; set; }
            public string number_4 { get; set; }
            public List<string> items { get; set; }
            public List<string> pickups { get; set; }
        }

        #endregion

        #region RESPONSE MODELS       

        [DataContract]
        public class API_RESPONSE_TYPE_0
        {
            [DataMember(Order = 1)]
            public int Count { get; set; }

            [DataMember(Order = 2)]
            public string Resultado { get; set; }

            [DataMember(Order = 3)]
            public string Descripcion { get; set; }

            [DataMember(Order = 4)]
            public long Resultado_Codigo { get; set; }

            [DataMember(Order = 5)]
            public string Resultado_Descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_1
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public long limit { get; set; }

            [DataMember(Order = 6)]
            public long rowset { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> items = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_2
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 99)]
            public long pickUP { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_3
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public long limit { get; set; }

            [DataMember(Order = 6)]
            public long rowset { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> cabeceras = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_4_OK
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int id { get; set; }

            [DataMember(Order = 99)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_4_ERROR
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int id { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> errores = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_5
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int contador { get; set; }

            [DataMember(Order = 6)]
            public string estado { get; set; }

            [DataMember(Order = 7)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_6
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int limit { get; set; }

            [DataMember(Order = 6)]
            public int rowset { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> items = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_7
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int solRecepId{ get; set; }

            [DataMember(Order = 6)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_8
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int solDespId { get; set; }

            [DataMember(Order = 6)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_9
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> items = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_10
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_11
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_12
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public bool resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public int PickId { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> items = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_13
        {
            [DataMember(Order = 1)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 2)]
            public string resultado_descripcion { get; set; }
            
        }

        [DataContract]
        public class API_RESPONSE_TYPE_14
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 5)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_14_CAB> items = new List<API_RESPONSE_TYPE_14_CAB>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_14_CAB
        {
            [DataMember(Order = 1)]
            public int? EmpId { get; set; }

            [DataMember(Order = 2)]
            public int NumeroReferencia { get; set; }

            [DataMember(Order = 3)]
            public string FechaReferencia { get; set; }

            [DataMember(Order = 4)]
            public string RutCliente { get; set; }

            [DataMember(Order = 5)]
            public string RazonSocial { get; set; }

            [DataMember(Order = 6)]
            public string Email { get; set; }

            [DataMember(Order = 7)]
            public string TipoDocumento { get; set; }

            [DataMember(Order = 8)]
            public string NumeroDocto { get; set; }

            [DataMember(Order = 9)]
            public string FechaDocto { get; set; }

            [DataMember(Order = 10)]
            public decimal MontoCompra { get; set; }

            [DataMember(Order = 11)]
            public string RutTercero { get; set; }

            [DataMember(Order = 12)]
            public string NombreTercero { get; set; }

            [DataMember(Order = 13)]
            public string EmailTercero { get; set; }

            [DataMember(Order = 14)]
            public string UsuarioDig { get; set; }

            [DataMember(Order = 15)]
            public short TipoSolicitud { get; set; }

            [DataMember(Order = 16)]
            public string Direccion { get; set; }

            [DataMember(Order = 17)]
            public string Telefono { get; set; }

            [DataMember(Order = 18)]
            public string Glosa { get; set; }

            [DataMember(Order = 19)]
            public string Estado { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> Detalle = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_15
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string descripcion { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_15_DET> items = new List<API_RESPONSE_TYPE_15_DET>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_15_DET
        {
            [DataMember(Order = 1)]
            public string Serie { get; set; }

            [DataMember(Order = 2)]
            public string FechaVencimiento { get; set; }

            [DataMember(Order = 3)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 4)]
            public decimal Stock { get; set; }

            [DataMember(Order = 5)]
            public decimal StockTotal { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_15_SALDOS> StockEstados = new List<API_RESPONSE_TYPE_15_SALDOS>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_15_SALDOS
        {
            [DataMember(Order = 1)]
            public string Estado { get; set; }

            [DataMember(Order = 2)]
            public decimal StockEstado { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_16
        {
            [DataMember(Order = 1)]
            public int? count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 4)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 5)]
            public int? codigoBodega { get; set; }

            [DataMember(Order = 6)]
            public string glosaBodega { get; set; }

            [DataMember(Order = 99)]
            public List<dynamic> detalle = new List<dynamic>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_17
        {
            [DataMember(Order = 1)]
            public string Resultado { get; set; }

            [DataMember(Order = 2)]
            public string Descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_18
        {
            [DataMember(Order = 1)]
            public int Count { get; set; }

            [DataMember(Order = 2)]
            public string Resultado { get; set; }

            [DataMember(Order = 2)]
            public string Resultado_Descripcion { get; set; }

            [DataMember(Order = 2)]
            public long Resultado_Codigo { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_19
        {
            [DataMember(Order = 1)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 2)]
            public string resultado_descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_24
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 6)]
            public long ProcesadosOK { get; set; }

            [DataMember(Order = 7)]
            public long ProcesadosERROR { get; set; }

            [DataMember(Order = 8)]
            public List<API_RESPONSE_TYPE_24_DET> Lineas = new List<API_RESPONSE_TYPE_24_DET>();
        }
        
        [DataContract]
        public class API_RESPONSE_TYPE_24_DET
        {
            [DataMember(Order = 1)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 2)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 3)]
            public string resultado { get; set; }

            [DataMember(Order = 4)]
            public string descripcion { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_27 //respuesta de: INTEGRACION_CONFIRMACIONES_JSON/LISTAR
        {
            [DataMember(Order = 1)]
            public int Count { get; set; }

            [DataMember(Order = 2)]
            public string Resultado { get; set; }

            [DataMember(Order = 3)]
            public string Descripcion { get; set; }

            [DataMember(Order = 4)]
            public long Resultado_Codigo { get; set; }

            [DataMember(Order = 5)]
            public string Resultado_Descripcion { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_27_CAB> Confirmacion = new List<API_RESPONSE_TYPE_27_CAB>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_27_CAB
        {
            [DataMember(Order = 1)]
            public long Id { get; set; }

            [DataMember(Order = 2)]
            public int EmpId { get; set; }

            [DataMember(Order = 3)]
            public string UserName { get; set; }

            [DataMember(Order = 4)]
            public string NombreProceso { get; set; }

            [DataMember(Order = 5)]
            public string FechaProceso { get; set; }

            [DataMember(Order = 6)]
            public long Folio  { get; set; }

            [DataMember(Order = 7)]
            public long FolioRel { get; set; }

            [DataMember(Order = 8)]
            public int Estado { get; set; }

            [DataMember(Order = 9)]
            public string FechaEstado { get; set; }

            [DataMember(Order = 10)]
            public int TipoDocumento { get; set; }

            [DataMember(Order = 11)]
            public string NumeroDocto { get; set; }

            [DataMember(Order = 12)]
            public string FechaDocto { get; set; }

            [DataMember(Order = 13)]
            public string TipoReferencia { get; set; }

            [DataMember(Order = 14)]
            public string NumeroReferencia { get; set; }

            [DataMember(Order = 15)]
            public string FechaReferencia { get; set; }

            [DataMember(Order = 16)]
            public string RutCliente { get; set; }

            [DataMember(Order = 17)]
            public string RazonSocial { get; set; }

            [DataMember(Order = 18)]
            public int TipoIntegracion { get; set; }

            [DataMember(Order = 19)]
            public string Texto1 { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_27_DET> Items = new List<API_RESPONSE_TYPE_27_DET>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_27_DET
        {
            [DataMember(Order = 1)]
            public int Id { get; set; }

            [DataMember(Order = 2)]
            public int IntId { get; set; }

            [DataMember(Order = 3)]
            public int Linea { get; set; }

            [DataMember(Order = 4)]
            public int ItemReferencia { get; set; }

            [DataMember(Order = 5)]
            public string Cadena { get; set; }

            [DataMember(Order = 6)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 7)]
            public string NroSerieDesp { get; set; }

            [DataMember(Order = 8)]
            public string FechaVectoDesp { get; set; }

            [DataMember(Order = 9)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 10)]
            public float Cantidad { get; set; }

            [DataMember(Order = 11)]
            public float CantidadProc { get; set; }

            [DataMember(Order = 12)]
            public string Texto1 { get; set; }

            [DataMember(Order = 13)]
            public string Texto2 { get; set; }

            [DataMember(Order = 14)]
            public string Texto3 { get; set; }

            [DataMember(Order = 15)]
            public int Estado { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_30
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 6)]
            public long ProcesadosOK { get; set; }

            [DataMember(Order = 7)]
            public long ProcesadosERROR { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_30_DET> Lineas = new List<API_RESPONSE_TYPE_30_DET>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_30_DET
        {
            [DataMember(Order = 11)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 12)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 13)]
            public string resultado { get; set; }

            [DataMember(Order = 14)]
            public string descripcion { get; set; }

            [DataMember(Order = 15)]
            public string CodigoArticulo { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_31
        {
            [DataMember(Order = 1)]
            public long Count { get; set; }

            [DataMember(Order = 2)]
            public long Limit { get; set; }

            [DataMember(Order = 3)]
            public long Rowset { get; set; }

            [DataMember(Order = 4)]
            public string resultado { get; set; }

            [DataMember(Order = 5)]
            public string descripcion { get; set; }

            [DataMember(Order = 6)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 7)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 8)]
            public List<API_RESPONSE_TYPE_31_ARTICULOS> Articulos = new List<API_RESPONSE_TYPE_31_ARTICULOS>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_31_ARTICULOS
        {
            [DataMember(Order = 1)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 2)]
            public string DescripArt { get; set; }

            [DataMember(Order = 3)]
            public string DescripTecnica { get; set; }

            [DataMember(Order = 4)]
            public string DescripCorta { get; set; }

            [DataMember(Order = 5)]
            public string UnidadMedidaBase { get; set; }

            [DataMember(Order = 6)]
            public int LineaProducto { get; set; }

            [DataMember(Order = 7)]

            //L_Articulos -> TipoModelo = Tipo Producto
            public string TipoProducto { get; set; }

            [DataMember(Order = 8)]

            //L_Articulos -> Tipo = Tipo Articulo
            public int TipoArticulo { get; set; }

            [DataMember(Order = 9)]
            public string VigenciaDesde { get; set; }

            [DataMember(Order = 10)]
            public string VigenciaHasta { get; set; }

            [DataMember(Order = 11)]
            public string Rotacion { get; set; }

            [DataMember(Order = 12)]
            public string CodigoFabrica { get; set; }

            [DataMember(Order = 13)]
            public int UsaSerie { get; set; }

            [DataMember(Order = 14)]
            public int UsaLote { get; set; }

            [DataMember(Order = 15)]
            public string EAN13 { get; set; }

            [DataMember(Order = 16)]
            public string DUN14 { get; set; }

            [DataMember(Order = 17)]
            public string UnidadMedidaCompra { get; set; }

            [DataMember(Order = 18)]
            public string UnidadMedidaVenta { get; set; }

            [DataMember(Order = 19)]
            public string CodigoProveedor { get; set; }

            [DataMember(Order = 20)]
            public string Version { get; set; }

            [DataMember(Order = 21)]
            public string Marca { get; set; }

            [DataMember(Order = 22)]

            public List<API_RESPONSE_TYPE_31_DETALLES_UM> UnidadMedida = new List<API_RESPONSE_TYPE_31_DETALLES_UM>();

            [DataMember(Order = 23)]

            public List<API_RESPONSE_TYPE_31_DETALLES_CAT> Categoria = new List<API_RESPONSE_TYPE_31_DETALLES_CAT>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_31_DETALLES_UM
        {
            [DataMember(Order = 1)]
            public string CodigoUM { get; set; }

            [DataMember(Order = 2)]
            public decimal Factor { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_31_DETALLES_CAT
        {
            [DataMember(Order = 1)]
            public string Categoria { get; set; }

            [DataMember(Order = 2)]
            public string SubCategoria { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_32
        {
            [DataMember(Order = 1)]
            public long Count { get; set; }

            [DataMember(Order = 2)]
            public long Limit { get; set; }

            [DataMember(Order = 3)]
            public long Rowset { get; set; }

            [DataMember(Order = 4)]
            public string resultado { get; set; }

            [DataMember(Order = 5)]
            public string descripcion { get; set; }

            [DataMember(Order = 6)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 7)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 8)]
            public List<API_RESPONSE_TYPE_32_ARTICULOS> Articulos = new List<API_RESPONSE_TYPE_32_ARTICULOS>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_32_ARTICULOS
        {
            [DataMember(Order = 1)]
            public string Cliente { get; set; }

            [DataMember(Order = 2)]
            public string CodigoArticulo { get; set; }

            [DataMember(Order = 3)]
            public string DescripArt { get; set; }

            [DataMember(Order = 4)]
            public string UnidadPresentacion { get; set; }

            [DataMember(Order = 5)]
            public string ClasificacionTemperatura { get; set; }

            [DataMember(Order = 6)]
            public string ClasificacionEmbarque { get; set; }

            [DataMember(Order = 7)]
            public string HojaSeguridad { get; set; }

            [DataMember(Order = 8)]
            public decimal Largo { get; set; }

            [DataMember(Order = 9)]
            public decimal Alto{ get; set; }

            [DataMember(Order = 10)]
            public decimal Ancho{ get; set; }

            [DataMember(Order = 11)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 12)]
            public string Paletizado { get; set; }

            [DataMember(Order = 13)]
            public string CodigoCliente { get; set; }

            [DataMember(Order = 14)]
            public string GrupoMaterial { get; set; }

            [DataMember(Order = 15)]
            public string EAN { get; set; }

            [DataMember(Order = 16)]
            public string CategoriaEAN { get; set; }

            [DataMember(Order = 17)]
            public decimal CostoUnitario { get; set; }

            [DataMember(Order = 18)]
            public string ArchivoImagen { get; set; }

            [DataMember(Order = 19)]
            public decimal PesoNeto { get; set; }

            [DataMember(Order = 20)]
            public string UnidadPeso { get; set; }

            [DataMember(Order = 21)]
            public decimal Volumen { get; set; }

            [DataMember(Order = 22)]
            public string UnidadVolumen { get; set; }

            [DataMember(Order = 23)]
            public string Cuarentena { get; set; }

            [DataMember(Order = 24)]
            public string TipoAlmacenamiento { get; set; }

            [DataMember(Order = 25)]
            public string ClasificacionAlmacenaje { get; set; }

            [DataMember(Order = 26)]
            public string Reposicion { get; set; }

            [DataMember(Order = 27)]
            public string Temperatura { get; set; }

            [DataMember(Order = 28)]
            public string InventarioCiclico { get; set; }

            [DataMember(Order = 29)]
            public string Apilamiento { get; set; }

            [DataMember(Order = 30)]
            public string ClasificacionABC { get; set; }

            [DataMember(Order = 31)]
            public string Lote { get; set; }

            [DataMember(Order = 32)]
            public string RestriccionViaAerea { get; set; }

            [DataMember(Order = 33)]
            public string EmbalajeCadenaFrio { get; set; }

            [DataMember(Order = 34)]
            public string ArchivoPDF { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_33
        {
            [DataMember(Order = 1)]
            public long Count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 99)]
            public List<API_RESPONSE_TYPE_33_MOVIMIENTOS> Movimientos = new List<API_RESPONSE_TYPE_33_MOVIMIENTOS>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_33_MOVIMIENTOS
        {
            [DataMember(Order = 1)]
            public int Almacen { get; set; }

            [DataMember(Order = 2)]
            public string CodigoMaterial { get; set; }

            [DataMember(Order = 3)]
            public string Lote { get; set; }

            [DataMember(Order = 4)]
            public DateTime Fecha { get; set; }

            [DataMember(Order = 5)]
            public int NumeroDocumento { get; set; }

            [DataMember(Order = 6)]
            public string TipoMovimiento { get; set; }

            [DataMember(Order = 7)]
            public decimal Cantidad { get; set; }

            [DataMember(Order = 8)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 9)]
            public string ClasificacionAlmacenaje { get; set; }

            [DataMember(Order = 10)]
            public string Zona { get; set; }

            [DataMember(Order = 11)]
            public string Posicion { get; set; }

            [DataMember(Order = 12)]
            public string Username { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_34
        {
            [DataMember(Order = 1)]
            public long Count { get; set; }

            //[DataMember(Order = 2)]
            //public long Limit { get; set; }

            //[DataMember(Order = 3)]
            //public long Rowset { get; set; }

            [DataMember(Order = 4)]
            public string resultado { get; set; }

            [DataMember(Order = 5)]
            public string descripcion { get; set; }

            [DataMember(Order = 6)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 7)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 8)]
            public List<API_RESPONSE_TYPE_34_STOCK> StockMaterial = new List<API_RESPONSE_TYPE_34_STOCK>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_34_STOCK
        {
            [DataMember(Order = 1)]
            public string Almacen { get; set; }

            [DataMember(Order = 2)]
            public string CodigoMaterial { get; set; }

            [DataMember(Order = 3)]
            public string Lote { get; set; }

            [DataMember(Order = 4)]
            public int Cantidad { get; set; }

            [DataMember(Order = 5)]
            public string UnidadMedida { get; set; }

            [DataMember(Order = 6)]
            public string Clasificacion { get; set; }

            [DataMember(Order = 7)]
            public string Zona { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_36
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string resultado_descripcion { get; set; }            

            [DataMember(Order = 6)]
            public int solRecepId { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_38
        {
            [DataMember(Order = 1)]
            public long count { get; set; }

            [DataMember(Order = 2)]
            public string resultado { get; set; }

            [DataMember(Order = 3)]
            public string descripcion { get; set; }

            [DataMember(Order = 4)]
            public long resultado_codigo { get; set; }

            [DataMember(Order = 5)]
            public string resultado_descripcion { get; set; }

            [DataMember(Order = 6)]
            public int AsnId { get; set; }
        }

        [DataContract]
        public class API_RESPONSE_TYPE_42_EVOLUTIVO_CAB
        {
            [DataMember(Order = 1)]
            public int Count { get; set; }

            [DataMember(Order = 2)]
            public string Resultado { get; set; }

            [DataMember(Order = 3)]

            public List<API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM> Items = new List<API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM>();
        }

        [DataContract]
        public class API_RESPONSE_TYPE_42_EVOLUTIVO_ITEM
        {
            [DataMember(Order = 1)]
            public int Id { get; set; }

            [DataMember(Order = 2)]
            public int EmpId { get; set; }

            [DataMember(Order = 3)]
            public string RazonSocial { get; set; }

            [DataMember(Order = 4)]
            public string Fecha { get; set; }

            [DataMember(Order = 5)]
            public int Piso { get; set; }

            [DataMember(Order = 6)]
            public int Estantes { get; set; }

            [DataMember(Order = 7)]
            public int Rack { get; set; }

            [DataMember(Order = 8)]
            public int Total { get; set; }
        }

        #endregion
    }
}