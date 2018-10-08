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
    public class ArticulosController : Controller
    {
        private VendimiaEntities db = new VendimiaEntities();

        // GET: Articulos
        public ActionResult Index()
        {
            return View(db.Articulo.ToList());
        }

        // GET: Articulos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Articulos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Articulo,Descripcion,Modelo,Precio,Existencia")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                articulo.Activo = true;
                db.Articulo.Add(articulo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(articulo);
        }

        // GET: Articulos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulo.Find(id);
            if (articulo == null)
            {
                return HttpNotFound();
            }
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Articulo,Descripcion,Modelo,Precio,Existencia")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                articulo.Activo = true;
                db.Entry(articulo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(articulo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
        public ActionResult Buscar(string id)
        {
            var coincidencias = (from a in db.Articulo
                                where (a.Descripcion.Contains(id) && a.Activo == true)
                                select new
                                {
                                    Id_Articulo = a.Id_Articulo,
                                    Descripcion = a.Descripcion,
                                    Modelo = a.Modelo,
                                    Precio = a.Precio,
                                    Existencia = a.Existencia
                                });
            List<Articulo> articulos = new List<Articulo>();
            Configuracion config = db.Configuracion.FirstOrDefault();
            foreach(var a in coincidencias)
            {
                Articulo art = new Articulo()
                {
                    Id_Articulo = a.Id_Articulo,
                    Descripcion = a.Descripcion,
                    Modelo = a.Modelo,
                    Precio = Calculadora.Importe(a.Precio),
                    Existencia = a.Existencia
                };
                articulos.Add(art);
            }
            return Json(articulos, JsonRequestBehavior.AllowGet);
        }
    }
}
