using MercadoPagoWEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace MercadoPagoWEB.Controllers
{
    public class BilleteraController : Controller
    {

        private MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        // GET: Billetera
        public ActionResult Index()
        {
            // ** SIMULACIÓN: Asumimos que el usuario logueado tiene id_usuario = 1 **
            int usuarioIDLogueado = 1;

            // 1. Obtener datos del usuario y cuenta (Usando LINQ de Entity Framework)
            var usuarioDB = db.Usuario.Find(usuarioIDLogueado);
            var cuentaDB = db.CuentaDigital.FirstOrDefault(c => c.id_usuario == usuarioIDLogueado);
            var metodosDB = db.MetodoPago.Where(m => m.id_usuario == usuarioIDLogueado).ToList();

            if (usuarioDB == null || cuentaDB == null)
            {
                return HttpNotFound("Usuario o cuenta no encontrada.");
            }

            // 2.Mapear a BilleteraViewModel
            var viewModel = new BilleteraViewModel
            {
                NombreCompleto = $"{usuarioDB.nombre} {usuarioDB.apellido}",
                EstadoKYC = usuarioDB.estado_kyc,
                SaldoActual = cuentaDB.saldo_actual ?? 0.00m,
                CVU_Alias = cuentaDB.alias,
                CVU = cuentaDB.cvu,
                Moneda = cuentaDB.moneda,
                MetodosPago = metodosDB.Select(m => new MetodoPagoSimpleViewModel
                {
                    TipoMetodo = m.tipo_metodo,
                    BancoEmisor = m.banco_emisor,
                    Ultimos4Digitos = m.ultimos_4_digitos
                }).ToList()
            };
            return View("MiBilletera", viewModel);
        }


        public async Task<ActionResult> ProbarApiKey ()
        {
            var renaperService = new RenaperService();

            string apiKey = await renaperService.GetApiKeyAsync();

            if (string.IsNullOrEmpty(apiKey))
            {
                // La prueba falló
                ViewBag.Resultado = "ERROR AL OBTENER LA CLAVE API.";
                ViewBag.Detalle = "Revisa si el servico de RENAPER esta corriendo";
            }
            else
            {
                // La prueba fue exitosa
                ViewBag.Resultado = "CLAVE API OBTENIDA CON EXITO.";
                ViewBag.Detalle = $"Clave API: **{apiKey}** (Longitud: {apiKey.Length})";
            }
            // Usaremos la vista de Index de Billetera para mostrar el resultado o una vista simple
            return View("ResultadoPrueba");
        }
    }
}