namespace ManejoPresupuesto.Models
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordPorPagina = 10;
        private readonly int cantidadMaximaRecordsPorPagina = 50;

        public int RecordsPorPagina { 
            get { return recordPorPagina; }
            set
            {
                recordPorPagina = (value > cantidadMaximaRecordsPorPagina) ? 
                    cantidadMaximaRecordsPorPagina : value;
            }
        }

        public int RecordsASaltar => recordPorPagina * (Pagina - 1);
    }
}
