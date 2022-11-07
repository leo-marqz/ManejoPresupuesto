using System.Security.Claims;

namespace ManejoPresupuesto.Services
{

    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public int ObtenerUsuarioId()
        {
            if(httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier)
                    .FirstOrDefault();
                var id = int.Parse(idClaim.Value);
                return id;
            }else
            {
                throw new ApplicationException("El usuario no esta logueado");
            }
            return 1;
        }
    }
}
