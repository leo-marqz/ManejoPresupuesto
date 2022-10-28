using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly string cadena;
        public TiposCuentasController(IConfiguration configuration)
        {
            this.cadena = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Crear()
        {
            using(var con = new SqlConnection(cadena))
            {
                var query = con.Query("SELECT 1").FirstOrDefault();
            }
            return View();
        }

        [HttpPost]
        public IActionResult Crear(TipoCuenta tipoCuenta)
        {
            if(!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            return View();
        }
    }
}
