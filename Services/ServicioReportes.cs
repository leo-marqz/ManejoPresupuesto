using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Services
{
    public interface IServicioReportes
    {
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int año, dynamic ViewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int año, dynamic viewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetallas(int usuarioId, int mes, int año, dynamic ViewBag);
    }

    public class ServicioReportes : IServicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;
        public ServicioReportes(
            IRepositorioTransacciones repositorioTransacciones, 
            IHttpContextAccessor httpContextAccessor
            )
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(
            int usuarioId, int mes, int año, dynamic ViewBag
            )
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);
            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            AsignarValoresAlViewBag(ViewBag, fechaInicio, fechaFin);
            var modelo = await repositorioTransacciones.ObtenerPorSemana(parametro);
            return modelo;
        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetallas(
            int usuarioId, int mes, int año, dynamic ViewBag
            )
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);
            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio, fechaFin);
            return modelo;
        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(
            int usuarioId, int cuentaId, int mes, int año, dynamic ViewBag
            )
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, año);
            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones
                .ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio, fechaFin);

            return modelo;
        }

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio, DateTime fechaFin)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaFin.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaFin.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaFin.AddMonths(1).Year;
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;

            Console.WriteLine("Protocolo: " + httpContext.Request.Protocol +
                " - Url: " + httpContext.Request.Path + httpContext.Request.QueryString);
        }

        private static ReporteTransaccionesDetalladas GenerarReporteTransaccionesDetalladas(
            DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones
            )
        {
            var modelo = new ReporteTransaccionesDetalladas();
            var transaccionesPorFechas = transacciones
                .OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });
            modelo.TransaccionesAgrupadas = transaccionesPorFechas;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;
            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(year: año, month: mes, day: 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            return (fechaInicio, fechaFin);
        }
    }
}
