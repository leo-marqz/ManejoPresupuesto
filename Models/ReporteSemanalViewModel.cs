namespace ManejoPresupuesto.Models
{
    public class ReporteSemanalViewModel
    {
        public decimal Ingresos => TransaccionesPorSemana.Sum(x => x.Ingresos);
        public decimal Egresos => TransaccionesPorSemana.Sum(x => x.Egresos);
        public decimal Total => Ingresos - Egresos;
        public DateTime FechaReferencia { get; set; }
        public IEnumerable<ResultadoObtenerPorSemana> TransaccionesPorSemana { get; set; }
    }
}
