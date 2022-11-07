using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);
        Task Eliminar(int id);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId";
            return await connection.QueryFirstOrDefaultAsync<Categoria>(query, new { id, usuarioId });
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "INSERT INTO Categorias(Nombre, TipoOperacionId, UsuarioId) " +
                "VALUES(@Nombre, @TipoOperacionId, @UsuarioId);SELECT SCOPE_IDENTITY();";
            var id = await connection.QuerySingleAsync<int>(query, categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = $"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId ORDER BY Nombre " +
                $"OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} " +
                $"ROW ONLY";
            return await connection.QueryAsync<Categoria>(query, new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT * FROM Categorias " +
                "WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @tipoOperacionId";
            return await connection.QueryAsync<Categoria>(query, new { usuarioId, tipoOperacionId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId " +
                "WHERE Id = @Id";
            await connection.ExecuteAsync(query, categoria);
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "DELETE Categorias WHERE Id = @Id";
            await connection.ExecuteAsync(query, new { id });
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @UsuarioId";
            return await connection.ExecuteScalarAsync<int>(query, new {usuarioId });
        }
    }
}
