using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuarios servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categorias = await this.repositorioCategorias.Obtener(usuarioId, paginacion);
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            if(!ModelState.IsValid)
            {
                return  View(categoria);
            }
            categoria.UsuarioId = usuarioId;
            await this.repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categoria = await this.repositorioCategorias.ObtenerPorId(id, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categoria = await this.repositorioCategorias.ObtenerPorId(categoriaEditar.Id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            categoriaEditar.UsuarioId = usuarioId;
            await this.repositorioCategorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categoria = await this.repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCateogira(int id)
        {
            var usuarioId = this.servicioUsuarios.ObtenerUsuarioId();
            var categoria = await this.repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await this.repositorioCategorias.Eliminar(id);
            return RedirectToAction("Index");
        }
    }
}
