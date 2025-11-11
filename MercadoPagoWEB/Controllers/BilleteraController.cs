using MercadoPagoWEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MercadoPagoWEB.Services;
using MercadoPagoWEB.Excepcion;

namespace MercadoPagoWEB.Controllers
{
    public class BilleteraController : Controller
    {
        // Contexto de Base de Datos y Servicios (DI-like approach)
        private MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();
        private readonly UsuarioService _usuarioService = new UsuarioService();
        private readonly RenaperService _renaperService = new RenaperService();
        private readonly ITransferenciaService _transferenciaService = new TransferenciaService();

        // GET: Billetera
        public ActionResult Index()
        {
            // ** SIMULACIÓN: Asumimos que el usuario logueado tiene id_usuario = 1 **
            int usuarioIDLogueado = 2; // (Asegúrate de reemplazar con la lógica de sesión real)

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
                IdUsuario = usuarioIDLogueado,
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

        // Método de prueba temporal para la clave API
        public async Task<ActionResult> ProbarApiKey()
        {
            string apiKey = await _renaperService.GetApiKeyAsync(); // <-- Usando el campo de clase


            if (string.IsNullOrEmpty(apiKey))
            {
                ViewBag.Resultado = "❌ ERROR AL OBTENER LA CLAVE API.";
                ViewBag.Detalle = "Revisa si el servicio de RENAPER (localhost:8080) está corriendo o si la URL/configuración es correcta.";
            }
            else
            {
                ViewBag.Resultado = "✅ CLAVE API OBTENIDA CON ÉXITO.";
                ViewBag.Detalle = $"Clave API: **{apiKey}** (Longitud: {apiKey.Length})";
            }
            // Usaremos la vista de Index de Billetera para mostrar el resultado o una vista simple
            return View("ResultadoPrueba");
        }

        [HttpPost]
        public async Task<ActionResult> IniciarVerificacionKYC(int id_usuario, string cuil)
        {
            // 1. Validación de Datos
            if (string.IsNullOrEmpty(cuil))
            {
                TempData["Error"] = "El CUIL es obligatorio para la verificación";
                return RedirectToAction("Index", "Billetera");
            }

            // 2. Obtener la clave API (Usa caché si está disponible)
            string apiKey = await _renaperService.GetApiKeyAsync();

            if (string.IsNullOrEmpty(apiKey))
            {
                TempData["Error"] = "❌ No se pudo obtener la clave API de RENAPER. Revisar Web.config.";
                return RedirectToAction("Index", "Billetera");
            }

            // 3. Realizar la verificación de identidad (GET)
            // 
            bool isVerificado = await _renaperService.VerificarIdentidad(cuil, apiKey);


            // 4. Actualización de la BD (Servicio de Usuario) y Mensajería
            string estadoFinal = isVerificado ? "Verificado" : "Rechazado";

            try
            {
                _usuarioService.ActualizarEstadoKYC(id_usuario, estadoFinal); // <-- Llama al servicio de persistencia
                TempData["Mensaje"] = isVerificado
                    ? "✅ KYC verificado exitosamente."
                    : "❌ KYC rechazado. Por favor, revise sus datos o intente más tarde.";
            }
            catch (Exception ex)
            {
                // Un error aquí significa que la BD falló, no la API de RENAPER.
                TempData["Error"] = "Error crítico en la base de datos: " + ex.Message;
            }
            return RedirectToAction("Index", "Billetera");
        }


        /// <summary>
        /// GET: /Billetera/Transferir
        /// Muestra el formulario para iniciar una transferencia.
        /// </summary>
        public ActionResult Transferir()
        {
            // Muestra una vista (puedes hacerla modal o una página separada)
            // con el formulario de transferencia.
            var model = new TransferenciaViewModel();
            return View("TransferirForm", model); // Necesitarás crear esta vista
        }

        /// <summary>
        /// POST: /Billetera/Transferir
        /// Procesa el formulario de transferencia.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Transferir(TransferenciaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar el formulario con errores
                return View("TransferirForm", model);
            }

            // ** SIMULACIÓN: Obtener el ID de usuario (igual que en tu método Index) **
            int usuarioIDLogueado = 2; // (Asegúrate de reemplazar con la lógica de sesión real)

            try
            {
                // Llamar al servicio adaptado para EF6
                await _transferenciaService.RealizarTransferenciaAsync(model, usuarioIDLogueado);

                // Éxito: Redirigir al Index de la Billetera con mensaje de éxito
                // (Usando el mismo patrón de TempData que usas en KYC)
                TempData["Mensaje"] = $"¡Transferencia de ${model.Monto} realizada con éxito!";
                return RedirectToAction("Index");
            }
            catch (TransferenciaException ex)
            {
                // Error de negocio (ej. "Saldo insuficiente")
                // Lo mostramos como error en el formulario
                ModelState.AddModelError("", ex.Message);
                return View("TransferirForm", model);
            }
            catch (Exception ex)
            {
                // Error inesperado
                ModelState.AddModelError("", $"Ocurrió un error inesperado. {ex.Message}");
                return View("TransferirForm", model);
            }
        }
    }
}