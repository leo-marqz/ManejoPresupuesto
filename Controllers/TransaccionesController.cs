using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Transactions;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IRepositorioTransacciones repositorioTransacciones, 
            IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas, 
            IRepositorioCategorias repositorioCategorias, IMapper mapper,
            IServicioReportes servicioReportes
            )
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = await servicioReportes
                .ObtenerReporteTransaccionesDetallas(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes = default, int año = default)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transacciones = await servicioReportes
                .ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);
            var agrupado = transacciones.GroupBy(x => x.Semana)
                .Select(x => new ResultadoObtenerPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                .Select(x => x.Monto).FirstOrDefault(),
                    Egresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Egreso)
                                .Select(x => x.Monto).FirstOrDefault()
                }).ToList();
            if(año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }
            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            var diasSegmentados = diasDelMes.Chunk(7).ToList();
            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }

            }
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public IActionResult Mensual()
        {
            return View();
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        public IActionResult Calendario()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await this.ObtenerCuentas(usuarioId);
            modelo.Categorias = await this.ObtenerCategorias(modelo.TipoOperacionId, usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel transaccionCreacion)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            if(!ModelState.IsValid)
            {
                transaccionCreacion.Cuentas = await this.ObtenerCuentas(usuarioId);
                transaccionCreacion.Categorias = await this.ObtenerCategorias(
                    transaccionCreacion.TipoOperacionId, usuarioId
                    );
                return View(transaccionCreacion);
            }
            var cuenta = await this.repositorioCuentas.ObtenerPorId(transaccionCreacion.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await this.repositorioCategorias.ObtenerPorId(transaccionCreacion.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            transaccionCreacion.UsuarioId = usuarioId;
            if(transaccionCreacion.TipoOperacionId == TipoOperacion.Egreso)
            {
                transaccionCreacion.Monto *= -1;
            }
            await this.repositorioTransacciones.Crear(transaccionCreacion);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await this.repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(
            TipoOperacion tipoOperacion
            , int usuarioId)
        {
            var categorias = await this.repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categorias = await this.ObtenerCategorias(tipoOperacion, usuarioId);
            return Ok(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await this.repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            if(modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(transaccion.TipoOperacionId, usuarioId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(TransaccionActualizacionViewModel tr)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            if(!ModelState.IsValid)
            {
                tr.Categorias = await ObtenerCategorias(tr.TipoOperacionId, usuarioId);
                tr.Cuentas = await ObtenerCuentas(usuarioId);
                return View(tr);
            }
            var cuenta = await this.repositorioCuentas.ObtenerPorId(tr.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await this.repositorioCategorias.ObtenerPorId( tr.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var transaccion = mapper.Map<Transaccion>(tr);
            tr.MontoAnterior = tr.Monto;
            if(tr.TipoOperacionId == TipoOperacion.Egreso)
            {
                transaccion.Monto *= -1;
            }
            await this.repositorioTransacciones.Actualizar(
                transaccion, 
                tr.MontoAnterior, 
                tr.CuentaAnteriorId
                );
            if(string.IsNullOrEmpty(tr.UrlRetorno))
            {
                Console.WriteLine("Entro a url vacia en transacciones actualizar");
                return RedirectToAction("Index");
            }
            else
            {
                Console.WriteLine("url redireccion: " + tr.UrlRetorno);
                return LocalRedirect(tr.UrlRetorno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id, string urlRetorno = null)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await this.repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTransacciones.Eliminar(id);
            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

    }
}
