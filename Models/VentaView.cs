using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vendimia.Models
{
    public class VentaView
    {
        public int Venta_Id { get; set; }
        public int Articulo_Id { get; set; }
        public int Cantidad { get; set; }
    }
}