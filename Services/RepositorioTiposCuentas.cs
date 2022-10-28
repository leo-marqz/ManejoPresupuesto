using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTiposCuentas
    {
        void Crear(TipoCuenta tipoCuenta);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = @"INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden) VALUES(@Nombre, @UsuarioId, 0);SELECT SCOPE_IDENTITY();";
            var id = connection.QuerySingle<int>(query, tipoCuenta);
            tipoCuenta.Id = id;
        }
    }
}
