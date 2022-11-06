namespace ManejoPresupuesto.Models
{
    public class ResultadoObtenerPorMes
    {
        public int Mes { get; set; }
        public DateTime FechaReferencia { get; set; }
        public decimal Ingreso { get; set; }
        public decimal Egreso { get; set; }
        public decimal Monto { get; set; }
        public TipoOperacion TipoOperacionId { get; set; }
    }
}
