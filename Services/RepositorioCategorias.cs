using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "INSERT INTO Categorias(Nombre, TipoOperacionId, UsuarioId) " +
                "VALUES(@Nombre, @TipoOperacionId, @UsuarioId);SELECT SCOPE_IDENTITY();";
            var id = await connection.QuerySingleAsync<int>(query, categoria);
            categoria.Id = id;
        }
    }
}
