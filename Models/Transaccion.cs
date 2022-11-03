using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Range(minimum:0, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoria")]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoria")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }
        //[DataType(DataType.DateTime)]
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        //public DateTime FechaTransaccion { get; set; } = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:MM tt"));
        public decimal Monto { get; set; }
        [StringLength(maximumLength:1000 , ErrorMessage = "El campo {1} notas no puede contener mas de 1,000 caracteres")]
        public string Nota { get; set; }
    }
}
