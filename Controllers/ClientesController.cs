using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Vendimia;

namespace Vendimia.Controllers
{
    public class ClientesController : Controller
    {
        private VendimiaEntities db = new VendimiaEntities();

        // GET: Clientes
        public ActionResult Index()
        {
            return View(db.Cliente.ToList());
        }

       
        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.Activo = true;
                db.Cliente.Add(cliente);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Cliente,Nombre,ApellidoPaterno,ApellidoMaterno,RFC,Activo")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cliente).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cliente);
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
            var coincidencias = (from a in db.Cliente
                                           where ((a.Nombre + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno).Contains(id) && a.Activo == true)
                                            select new {
                                                Id_Cliente = a.Id_Cliente,
                                                Nombre = a.Nombre,
                                                ApellidoPaterno = a.ApellidoPaterno,
                                                ApellidoMaterno = a.ApellidoMaterno,
                                                RFC = a.RFC
                                            });
            return Json(coincidencias, JsonRequestBehavior.AllowGet);
        }
    }
}
