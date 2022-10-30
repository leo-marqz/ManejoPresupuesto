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

        public CuentasController(
            IRepositorioTiposCuentas repositorioTiposCuentas, 
            IServicioUsuarios servicio,
            IRepositorioCuentas repositorioCuentas
            )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicio = servicio;
            this.repositorioCuentas = repositorioCuentas;
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

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(tc => new SelectListItem(tc.Nombre, tc.Id.ToString()));
        }
    }
}
