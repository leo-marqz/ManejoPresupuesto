using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using ManejoPresupuesto.Services;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(
            IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios
            )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta =
                await repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if(!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(
                tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if(yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre),
                    $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }
            await repositorioTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }


        [HttpPut]
        public async Task<IActionResult> Actualizar()
        {
            return Ok();
        }

    }
}
