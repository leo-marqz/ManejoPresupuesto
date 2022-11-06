namespace ManejoPresupuesto.Models
{
    public class ReporteMensualViewModel
    {
        public IEnumerable<ResultadoObtenerPorMes> TransaccionesPorMes { get; set; }
        public decimal Ingresos => TransaccionesPorMes.Sum(x=>x.Ingreso);
        public decimal Egresos => TransaccionesPorMes.Sum(x => x.Egreso);
        public decimal Total => Ingresos - Egresos;
        public int Año { get; set; }
    }
}
