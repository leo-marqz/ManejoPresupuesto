using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuracion)
        {
            this.connectionString = configuracion.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(this.connectionString);
            var id = await connection.QuerySingleAsync("Transaccion_Insertar", 
                new{
                    transaccion.UsuarioId,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.Nota
                }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
