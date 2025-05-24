using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace InvetarioCrud.Models
{
    public enum TipoMovimiento
    {
        Ingreso,
        Traslado,
        Salida
    }
    public class Movimiento
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int IDProducto { get; set; }
        public int? IDAlmacenOrigen { get; set; }
        public int IDAlmacenDestino { get; set; }
        public int Cantidad { get; set; }
        public TipoMovimiento Tipo { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [Ignore]
        public string NombreProducto { get; set; }
        [Ignore]
        public string NombreAlmacenOrigen { get; set; }
        [Ignore]
        public string NombreAlmacenDestino { get; set; }
    }
}
