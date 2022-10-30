using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(this.connectionString);
            string query = "INSERT INTO Cuentas(Nombre, TipoCuentaId, Descripcion, Balance)" +
                "VALUES(@Nombre, @TipoCuentaId, @Descripcion, @Balance); SELECT SCOPE_IDENTITY();";
            var id = await connection.QuerySingleAsync<int>(query, cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT c.Id, c.Nombre, c.Balance, tc.Nombre as TipoCuenta FROM Cuentas as c " +
                "INNER JOIN TiposCuentas as tc ON tc.Id = c.TipoCuentaId " +
                "WHERE tc.UsuarioId = @UsuarioId ORDER BY tc.Orden;";
            var cuentas = await connection.QueryAsync<Cuenta>(query, new { usuarioId });
            return cuentas;
        }
    }
}
