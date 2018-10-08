using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Vendimia;
using Vendimia.Models;

namespace Vendimia.Controllers
{
    public class ConfiguracionesController : Controller
    {
        private VendimiaEntities db = new VendimiaEntities();

        public ActionResult Obtener()
        {
            Configuracion configuracion = db.Configuracion.FirstOrDefault();
            return Json(configuracion, JsonRequestBehavior.AllowGet);
        }

        // GET: Configuraciones/Edit/5
        public ActionResult Edit()
        {
            Configuracion configuracion = db.Configuracion.FirstOrDefault();
            if (configuracion == null)
            {
                return HttpNotFound();
            }
            return View(configuracion);
        }

        // POST: Configuraciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TasaFinanciamiento,Enganche,PlazoMaximo")] Configuracion configuracion)
        {
            if (ModelState.IsValid)
            {
                if (Data_Update(configuracion))
                    ViewBag.Exito = "La configuracion ha sido registrada.";
                else
                    ModelState.AddModelError(string.Empty, "Ocurrio un error al registrar tu configuracion.");
            }
            return Edit();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /*
         * Este lo hice diferente por 2 razones, la primera para mostrar la separacion de capas y que tambien puedo
         * hacer consultas o ejecutar stored procedures sin utilizar entity framework, usare lo que se necesite
         * dependiendo de la situacion
         */
        public bool Data_Update(Configuracion configuracion)
        {
            using (SqlConnection con = new SqlConnection(_Settings.db))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SP_ConfiguracionUPD";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@TasaFinanciamiento", configuracion.TasaFinanciamiento);
                    cmd.Parameters.AddWithValue("@Enganche", configuracion.Enganche);
                    cmd.Parameters.AddWithValue("@PlazoMaximo", configuracion.PlazoMaximo);

                    con.Open();

                    int ds = cmd.ExecuteNonQuery();

                    con.Close();

                    if (ds == 0)
                        return false;

                    return true;
                }
            }
        }
    }
}
