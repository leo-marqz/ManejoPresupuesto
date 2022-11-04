using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicio;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;

        public CuentasController(
            IRepositorioTiposCuentas repositorioTiposCuentas, 
            IServicioUsuarios servicio,
            IRepositorioCuentas repositorioCuentas,
            IMapper mapper,
            IRepositorioTransacciones repositorioTransacciones
            )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicio = servicio;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await this.repositorioCuentas.Buscar(usuarioId);
            var modelo = cuentasConTipoCuenta.GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel()
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var cuenta = await this.repositorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var model = this.mapper.Map<CuentaCreacionViewModel>(cuenta);
            //var model = new CuentaCreacionViewModel()
            //{
            //    Id = cuenta.Id,
            //    Nombre = cuenta.Nombre,
            //    TipoCuentaId = cuenta.TipoCuentaId,
            //    Descripcion = cuenta.Descripcion,
            //    Balance = cuenta.Balance
            //};
            model.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await this.ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var tipoCuenta = await this.repositorioTiposCuentas
                .ObtenerPorId(cuenta.TipoCuentaId, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            if(!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await this.ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }
            await this.repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var cuenta = await this.repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEcontrado", "Home");
            }
            var tipoCuenta = await this.repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await this.repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var cuenta = await this.repositorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCuenta(int id)
        {
            var usuarioId = this.servicio.ObtenerUsuarioId();
            var cuenta = await this.repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await this.repositorioCuentas.Eliminar(id);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(tc => new SelectListItem(tc.Nombre, tc.Id.ToString()));
        }

        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = servicio.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            DateTime fechaInicio;
            DateTime fechaFin;
            if(mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }else
            {
                fechaInicio = new DateTime(year: año, month: mes, day: 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = id,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones
                .ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);
            var modelo = new ReporteTransaccionesDetalladas();
            ViewBag.Cuenta = cuenta.Nombre;
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

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaFin.AddMonths(-1).Year;

            ViewBag.mesPosterior = fechaFin.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaFin.AddMonths(1).Year;
            var urlPath = HttpContext.Request.Path + HttpContext.Request.QueryString;
            ViewBag.urlRetorno = urlPath;
            Console.WriteLine(urlPath);
            return View(modelo);
        }
    }
}
