using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            //usuario.EmailNormalizado = usuario.Email.ToUpper();
            using var connection = new SqlConnection(this.connectionString);
            var query = "INSERT INTO Usuarios(Email, EmailNormalizado, PasswordHash) " +
                "VALUES(@Email, @EmailNormalizado, @PasswordHash); SELECT SCOPE_IDENTITY();";
            var usuarioId = await connection.QuerySingleAsync<int>(query, usuario);

            await connection.ExecuteAsync("CrearDatosDefectoAUsuarioNuevo",
                new { usuarioId }, commandType: System.Data.CommandType.StoredProcedure);

            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT * FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado";
            return await connection.QuerySingleOrDefaultAsync<Usuario>(query, new {emailNormalizado});
        }
    }
}
