using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Crear(TipoCuenta tipoCuenta);
        Task Eliminar(int id);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(this.connectionString);
            var consulta = @"INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden) VALUES(@Nombre, @UsuarioId, 0);SELECT SCOPE_IDENTITY();";
            var id = await connection.QuerySingleAsync<int>(consulta, tipoCuenta);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var consulta = @"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId";
            var existe = await connection.QueryFirstOrDefaultAsync<int>(consulta, new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var consulta = @"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId";
            return await connection.QueryAsync<TipoCuenta>(consulta, new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(this.connectionString);
            var consulta = "UPDATe TiposCuentas SET Nombre = @Nombre WHERE Id = @Id";
            await connection.ExecuteAsync(consulta, tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var consulta = "SELECT Id, Nombre, Orden FROM TiposCuentas WHERE Id = @Id AND UsuarioId = @UsuarioId";
            return await connection
                .QueryFirstOrDefaultAsync<TipoCuenta>(consulta, new {id, usuarioId});
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(this.connectionString);
            var consulta = "DELETE FROM TiposCuentas WHERE Id = @Id";
            await connection.ExecuteAsync(consulta, new {id});
        }
    }
}
