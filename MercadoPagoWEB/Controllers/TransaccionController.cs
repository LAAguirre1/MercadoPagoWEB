using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MercadoPagoWEB.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace MercadoPagoWEB.Controllers
{
    [Authorize]
    public class TransaccionController : Controller
    {
        // Contexto de Entity Framework
        private readonly MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        // GET: Transaccion/Listado
        // (Mi Botón de la billetera apunta a "Listado", "Transaccion")
        public ActionResult Listado()
        {
            // --- INICIO DEL BLOQUE NUEVO (SÍNCRONO) ---

            // 1. Obtener el ID de Identity (el string largo)
            string idAspNetUser = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(idAspNetUser))
            {
                // Si [Authorize] falla, esto nos protege
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized, "No se pudo obtener la sesión de usuario.");
            }

            // 2. Buscar en nuestra tabla 'Usuario' usando ese ID (¡SIN Async!)
            var usuario = db.Usuario.FirstOrDefault(u => u.AspNetUserId == idAspNetUser);

            if (usuario == null)
            {
                // Esto pasaría si el usuario está logueado pero no tiene perfil
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "El perfil de usuario no está sincronizado.");
            }

            // 3. Ahora usamos el ID real del usuario que ha iniciado sesión
            int usuarioIDLogueado = usuario.id_usuario;

            // --- FIN DEL BLOQUE NUEVO ---

            // 1. Encontrar la cuenta digital del usuario
            var cuentaUsuario = db.CuentaDigital.FirstOrDefault(c => c.id_usuario == usuarioIDLogueado);

            if (cuentaUsuario == null)
            {
                // Manejar el caso que el usuario no tenga cuenta (No debería pasar en teoría)
                TempData["Error"] = "No se encontró la cuenta digital del usuario.";
                return RedirectToAction("Index", "Billetera");
            }

            int idCuentaUsuario = cuentaUsuario.id_cuenta;


            // 2. LA CONSULTA LINQ
            // Buscamos todos los movimientos donde el usuario sea origen O destino
            // Usamos .Include() para traer los datos de la Transaccion 
            // Proyectamos a ActividadViewModel
            var listaMovimientos = db.MovimientoCuenta
                .Include("Transaccion")
                .Where(m => m.id_cuenta_origen == idCuentaUsuario || m.id_cuenta_destino == idCuentaUsuario)
                .OrderByDescending(m => m.Transaccion.fecha_hora)
                .Select(m => new ActividadViewModel
                {
                    IdTransaccion = m.id_transaccion,
                    FechaHora = m.Transaccion.fecha_hora.Value,
                    TipoTransaccion = m.Transaccion.tipo_transaccion,
                    Descripcion = m.Transaccion.descripcion,
                    // Usamos el monto_afectado de este movimiento especifico
                    MontoAfectado = m.monto_afectado
                })
                .ToList();

            // 3. Pasar el modelo a la vista
            return View("ListadoActividad", listaMovimientos);
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