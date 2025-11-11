using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MercadoPagoWEB.Models;

namespace MercadoPagoWEB.Controllers
{
    public class TransaccionController : Controller
    {
        // Contexto de Entity Framework
        private readonly MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        // GET: Transaccion/Listado
        // (Mi Botón de la billetera apunta a "Listado", "Transaccion")
        public ActionResult Listado()
        {
            // **Simulación: asumimos que el usuario logueado tiene id_usuario = 2 **
            // Reemplazar con la lógica de autenticación real cuando la tenga
            int usuarioIDLoqueado = 2;

            // 1. Encontrar la cuenta digital del usuario
            var cuentaUsuario = db.CuentaDigital.FirstOrDefault(c => c.id_usuario == usuarioIDLoqueado);

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