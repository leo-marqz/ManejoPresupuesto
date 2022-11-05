using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Crear(Transaccion transaccion);
        Task Eliminar(int id);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
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

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(this.connectionString);
            await connection.ExecuteAsync("Transaccion_Actualizar", new
            {
                transaccion.Id, transaccion.FechaTransaccion, transaccion.Monto,
                transaccion.CategoriaId, transaccion.CuentaId, transaccion.Nota,
                montoAnterior,
                cuentaAnteriorId
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT transacciones.*, cat.TipoOperacionId " +
                "FROM Transacciones INNER JOIN Categorias cat " +
                "ON cat.Id = Transacciones.CategoriaId " +
                "WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId";
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(query, new {id, usuarioId});
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(this.connectionString);
            await connection.ExecuteAsync("Transaccion_Eliminar", new { id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(
            ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT tr.Id, tr.Monto, tr.FechaTransaccion, cg.Nombre AS Categoria," +
                " ct.Nombre AS Cuenta, cg.TipoOperacionId " +
                "FROM Transacciones tr " +
                "INNER JOIN Categorias cg ON cg.Id = tr.CategoriaId" +
                " INNER JOIN Cuentas ct ON ct.Id = tr.CuentaId " +
                "WHERE tr.CuentaId = @CuentaId AND tr.UsuarioId = @UsuarioId " +
                "AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin";
            return await connection.QueryAsync<Transaccion>(query, modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(
            ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(this.connectionString);
            var query = "SELECT tr.Id, tr.Monto, tr.FechaTransaccion, cg.Nombre AS Categoria," +
                " ct.Nombre AS Cuenta, cg.TipoOperacionId " +
                "FROM Transacciones tr " +
                "INNER JOIN Categorias cg ON cg.Id = tr.CategoriaId" +
                " INNER JOIN Cuentas ct ON ct.Id = tr.CuentaId " +
                "WHERE tr.UsuarioId = @UsuarioId " +
                "AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin " +
                "ORDER BY tr.FechaTransaccion DESC";
            return await connection.QueryAsync<Transaccion>(query, modelo);
        }

    }
}
