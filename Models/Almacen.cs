using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Collections.Generic;

namespace InvetarioCrud.Models
{
    public class Almacen
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string NombreAlmacen { get; set; }
        public string Descripcion { get; set; }

        [Ignore] 
        public List<AlmacenProductoInfo> ProductosEnAlmacen { get; set; } = new();
    }
}
