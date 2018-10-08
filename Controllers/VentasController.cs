using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Vendimia;
using Vendimia.Models;

namespace Vendimia.Controllers
{
    public class VentasController : Controller
    {
        private VendimiaEntities db = new VendimiaEntities();

        // GET: Ventas
        public ActionResult Index()
        {
            var venta = db.Venta.Include(v => v.Cliente).Include(v => v.Estatus).Where(v => v.Estatus_Id == 2);
            return View(venta.ToList());
        }

        // GET: Ventas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta venta = db.Venta.Find(id);
            if (venta == null)
            {
                return HttpNotFound();
            }
            return View(venta);
        }

        // GET: Ventas/Create
        public ActionResult Create()
        {
            Venta venta = new Venta()
            {
                Estatus_Id = 1,
                FechaCreacion = DateTime.Now
            };
            ViewBag.PlazoMaximo = db.Configuracion.FirstOrDefault().PlazoMaximo;
            db.Venta.Add(venta);
            db.SaveChanges();
            return View(venta);
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Venta venta)
        {
            if (ModelState.IsValid)
            {
                Configuracion config = db.Configuracion.FirstOrDefault();
                venta.FechaCreacion = DateTime.Now;
                venta.Estatus_Id = 2;
                venta.TasaFinanciamiento = config.TasaFinanciamiento;
                venta.Enganche = config.Enganche;
                decimal importe = 0;
                List<VentaDetalle> articulos = db.VentaDetalle.Where(a => a.Venta_Id == venta.Id_Venta).ToList<VentaDetalle>();
                foreach (VentaDetalle v in articulos) {
                    importe += Calculadora.Importe(db.Articulo.Find(v.Articulo_Id).Precio);
                }
                decimal enganche = Calculadora.Enganche(importe);
                decimal bonificacion = Calculadora.BonificacionEnganche(enganche);
                decimal total = importe - enganche - bonificacion;
                decimal contado = Calculadora.PrecioContado(total);
                decimal pagar = Calculadora.Total(contado,(int) venta.Plazo);
                decimal mensualidad =Math.Round((pagar/ (decimal)venta.Plazo),2);
                venta.Mensualidad = mensualidad;
                venta.Importe = importe;
                venta.MontoEnganche = enganche;
                venta.MontoBonificacion = bonificacion;
                venta.Total = pagar;
                db.Entry(venta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(venta);
        }

        [HttpPost]
        public ActionResult AgregarArticulo(VentaView ventaView)
        {
            if (ModelState.IsValid)
            {
                VentaDetalle comprobacion = (from a in db.VentaDetalle
                                   where (a.Venta_Id == ventaView.Venta_Id && a.Articulo_Id == ventaView.Articulo_Id)
                                   select a).FirstOrDefault();

                Venta venta = db.Venta.Find(ventaView.Venta_Id);
                Articulo articulo = db.Articulo.Find(ventaView.Articulo_Id);

                if (articulo.Existencia < ventaView.Cantidad)
                {
                    return Json(new { estado = false });
                }
                articulo.Existencia -= ventaView.Cantidad;
                db.Entry(articulo).State = EntityState.Modified;

                if (comprobacion == null)
                {
                    VentaDetalle vd = new VentaDetalle()
                    {
                        Articulo_Id = ventaView.Articulo_Id,
                        Cantidad = ventaView.Cantidad
                    };
                    venta.VentaDetalle.Add(vd);
                    db.Entry(venta).State = EntityState.Modified;
                }
                else
                {
                    comprobacion.Cantidad += ventaView.Cantidad;
                    db.Entry(comprobacion).State = EntityState.Modified;
                }
                
                db.SaveChanges();
                return Json(new { estado = true });
            }
            return Json(new { estado = false });
        }

        [HttpPost]
        public ActionResult QuitarArticulo(VentaView ventaView)
        {
            if (ModelState.IsValid)
            {
                Articulo articulo = db.Articulo.Find(ventaView.Articulo_Id);
                
                VentaDetalle vd = (from a in db.VentaDetalle
                                   where (a.Venta_Id == ventaView.Venta_Id && a.Articulo_Id == ventaView.Articulo_Id)
                                   select a).FirstOrDefault();

                articulo.Existencia += vd.Cantidad;
                db.Entry(articulo).State = EntityState.Modified;

                db.VentaDetalle.Remove(vd);

                db.SaveChanges();
                return Json(new { estado = true });
            }
            return Json(new { estado = false });
        }

        [HttpPost]
        public ActionResult CancelarVenta(Venta venta)
        {
            if (ModelState.IsValid)
            {
                List<VentaDetalle> articulos = db.VentaDetalle.Where(a => a.Venta_Id == venta.Id_Venta).ToList<VentaDetalle>();

                venta = db.Venta.Find(venta.Id_Venta);
                
                foreach (VentaDetalle v in articulos)
                {
                    Articulo articulo = db.Articulo.Find(v.Articulo_Id);
                    articulo.Existencia += v.Cantidad;
                    db.Entry(articulo).State = EntityState.Modified;
                }
                venta.Estatus_Id = 4;
                db.Entry(venta).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { estado = true });
            }
            return Json(new { estado = false });
        }

        [HttpPost]
        public ActionResult ModificarArticulo(VentaView ventaView)
        {
            if (ModelState.IsValid)
            {
                Articulo articulo = db.Articulo.Find(ventaView.Articulo_Id);
                VentaDetalle vd = (from a in db.VentaDetalle
                                   where (a.Venta_Id == ventaView.Venta_Id && a.Articulo_Id == ventaView.Articulo_Id)
                                   select a).FirstOrDefault();
                if (articulo.Existencia < (ventaView.Cantidad - vd.Cantidad))
                {
                    return Json(new { estado = false });
                }
                
                articulo.Existencia -= (ventaView.Cantidad - vd.Cantidad);
                vd.Cantidad = ventaView.Cantidad;
                db.Entry(articulo).State = EntityState.Modified;
                db.Entry(vd).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { estado = true });
            }
            return Json(new { estado = false });
        }

        public ActionResult SeleccionarPlazo(Venta venta)
        {
            ViewBag.PlazoMaximo = db.Configuracion.FirstOrDefault().PlazoMaximo;
            return View("SeleccionarPlazo",venta);
        }

        [HttpPost]
        public ActionResult SeleccionarPlazo(int Id_Venta, int Plazo)
        {
            Venta venta = db.Venta.Find(Id_Venta);
            if (ModelState.IsValid)
            {
                foreach (VentaDetalle v in venta.VentaDetalle)
                {
                    Articulo art = db.Articulo.Find(v.Articulo_Id);
                    if (v.Cantidad > art.Existencia)
                    {
                        ViewBag.Error = "Se agotaron las existencias";
                        RedirectToAction("Create", venta);
                    }
                    art.Existencia -= v.Cantidad;
                    db.Entry(art).State = EntityState.Modified;
                }
                venta.Mensualidad = Math.Round(Calculadora.PrecioContado((decimal)venta.Total)/Plazo,2);
                venta.Total = Calculadora.Total(Calculadora.PrecioContado((decimal)venta.Total), Plazo);
                venta.Plazo = Plazo;
                venta.Estatus_Id = 2;
                db.Entry(venta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(venta);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
