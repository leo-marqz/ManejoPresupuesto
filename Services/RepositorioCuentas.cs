using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuentaActualizar);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task Eliminar(int id);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
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

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT c.Id, tc.Id as TipoCuentaId, c.Nombre , c.Balance, c.Descripcion  " +
                "FROM Cuentas as c  " +
                "INNER JOIN TiposCuentas as tc ON tc.Id = c.TipoCuentaId " +
                "WHERE tc.UsuarioId = @UsuarioId AND c.Id = @Id ORDER BY tc.Orden;";
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(query, new {usuarioId, id});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuentaActualizar)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "UPDATE Cuentas " +
                "SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId " +
                "WHERE Id = @Id";
            await connection.ExecuteAsync(query, cuentaActualizar);
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "DELETE FROM Cuentas WHERE Id = @Id";
            await connection.ExecuteAsync(query, new {id});
        }
    }
}
