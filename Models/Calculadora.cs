using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;

namespace Vendimia.Models
{
    public class Calculadora
    {
        public static decimal Importe(decimal precio)
        {
            VendimiaEntities db = new VendimiaEntities();
            Configuracion config = db.Configuracion.FirstOrDefault();
            return Math.Round(precio * (1 + ((decimal)config.TasaFinanciamiento * config.PlazoMaximo / 100)),2);
        }
        public static decimal Enganche(decimal importe)
        {
            VendimiaEntities db = new VendimiaEntities();
            Configuracion config = db.Configuracion.FirstOrDefault();
            return Math.Round(((decimal)config.Enganche) / 100 * importe,2);
        }
        public static decimal BonificacionEnganche(decimal enganche)
        {
            VendimiaEntities db = new VendimiaEntities();
            Configuracion config = db.Configuracion.FirstOrDefault();
            return Math.Round(enganche * ( ( ((decimal)config.TasaFinanciamiento) * ((decimal)config.PlazoMaximo)) / 100),2);
        }
        public static decimal PrecioContado(decimal total)
        {
            VendimiaEntities db = new VendimiaEntities();
            Configuracion config = db.Configuracion.FirstOrDefault();
            return Math.Round(total / (1+((((decimal)config.TasaFinanciamiento) * ((decimal)config.PlazoMaximo)) / 100)),2);
        }
        public static decimal Total(decimal contado, int plazo)
        {
            VendimiaEntities db = new VendimiaEntities();
            Configuracion config = db.Configuracion.FirstOrDefault();
            return Math.Round(contado * ( 1 + ( ( (decimal)config.TasaFinanciamiento) * plazo / 100) ),2);
        }
    }
}