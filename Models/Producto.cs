using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace InvetarioCrud.Models
{
    public  class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
    }
}
