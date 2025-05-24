using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace InvetarioCrud.Models
{
    public class AlmacenProducto
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; } 

        [Indexed] 
        public int IDProducto { get; set; }

        [Indexed]
        public int IDAlmacen { get; set; }

        public int Cantidad { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
    public class AlmacenProductoInfo : AlmacenProducto
    {
        public string NombreProducto { get; set; }
        public string NombreAlmacen { get; set; }
    }
}
