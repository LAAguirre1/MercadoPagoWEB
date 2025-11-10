using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MercadoPagoWEB;

namespace MercadoPagoWEB.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly RenaperService _renaperService = new RenaperService();
        private readonly MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        // GET: Usuarios
        public ActionResult Index()
        {
            return View(db.Usuario.ToList());
        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_usuario,dni,nombre,apellido,email,password_hash,fecha_registro,estado_kyc")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                db.Usuario.Add(usuario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_usuario,dni,nombre,apellido,email,password_hash,fecha_registro,estado_kyc")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Usuario usuario = db.Usuario.Find(id);
            db.Usuario.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Métoto para iniciar la verificación KYC usando Renaper
        [HttpPost]
        public async Task<ActionResult> IniciarVerificacionKYC(int id_usuario, string cuil)
        {
            // Inicializamos el servicio
            var _renaperService = new RenaperService();

            // Obtenemos la clave API
            string apiKey = await _renaperService.GetApiKeyAsync();

            if (string.IsNullOrEmpty(apiKey))
            {
                TempData["Error"] = "No se pudo obtener la clave API de Renaper.";
                return RedirectToAction("Index", "Billetera");
            }

            // Realizar la verificación de identidad
            bool isVerificado = await _renaperService.VerificarIdentidad(cuil, apiKey);

            // Actualizar el estado KYC del usuario en la base de datos
            string estadoFinal = isVerificado ? "Verificado" : "Rechazado";

            try
            {
                // El método de actualización que definimos antes
                ActualizarEstadoKYC(id_usuario, estadoFinal);
                TempData["Mensaje"] = isVerificado
                    ? "KYC verificado exitosamente."
                    : "KYC rechazado. Por favor, revise sus datos.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el estado KYC: {ex.Message}";
            }
            return RedirectToAction("Index", "Billetera");
        }

        // Dentro de tu clase de acceso a datos o directamente en el controlador
        private void ActualizarEstadoKYC(int usuarioId, string nuevoEstado)
        {
            try
            {
                //1. Buscar el usuario por su ID
                var usuario = db.Usuario.Find(usuarioId);

                if (usuario != null)
                {
                    //2. Asignar el nuevo estado
                    usuario.estado_kyc = nuevoEstado;


                    //3. Guardar los cambios en la base de datos
                    db.SaveChanges();
                }
                else
                {
                    // Manejo de error: Usuario no encontrado
                    Console.WriteLine($"Error: Usuario con Id {usuarioId} no encontrado.");
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores de la base de datos (Ej: Fallo de conexión)
                throw new Exception("Error al actualizar el estado KYC en la base de datos.", ex);
            }
        }
    }
}
