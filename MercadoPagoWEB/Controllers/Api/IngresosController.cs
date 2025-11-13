using MercadoPagoWEB.Filters; // Para [ApiKeyAuthorize]
using MercadoPagoWEB.Models; // Para la BD
using MercadoPagoWEB.Models.Api; // Para los DTOs
using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;

namespace MercadoPagoWEB.Controllers.Api
{
    /// <summary>
    /// API pública para recibir ingresos (depósitos) desde
    /// otras aplicaciones asociadas (ej. Ualá, Grupo B).
    /// </summary>
    [AllowAnonymous] // 1. Permitimos el paso del guardia de Cookies
    [ApiKeyAuthorize] // 2. Nuestro guardia de API Key toma el control
    [RoutePrefix("api/v1/ingresos")]
    public class IngresosController : ApiController
    {
        private MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        /// <summary>
        /// Procesa un ingreso (crédito) a una cuenta interna desde una fuente externa.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> RealizarIngresoExterno([FromBody] IngresoExternoRequest request)
        {
            // 1. Validar que el JSON recibido es válido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 2. Buscar la cuenta de DESTINO en NUESTRA BD
                var cuentaDestino = await db.CuentaDigital
                    .FirstOrDefaultAsync(c => c.cvu == request.IdentificadorDestino || c.alias == request.IdentificadorDestino);

                // 3. Validar que la cuenta exista
                if (cuentaDestino == null)
                {
                    return Content(HttpStatusCode.NotFound,
                        new { Message = $"La cuenta de destino '{request.IdentificadorDestino}' no fue encontrada en nuestro sistema." });
                }

                // 4. ¡Ejecutar el Stored Procedure de Ingreso que YA TENEMOS!
                var idDestinoParam = new SqlParameter("@id_cuenta_destino", cuentaDestino.id_cuenta);
                var montoParam = new SqlParameter("@monto", request.Monto);

                // Usamos la referencia externa en la descripción para rastreo
                string desc = $"Ingreso de {request.IdTransaccionExterna}. {request.Descripcion ?? ""}";
                var descParam = new SqlParameter("@descripcion", desc);

                await db.Database.ExecuteSqlCommandAsync(
                    "EXEC SP_RealizarIngreso @id_cuenta_destino, @monto, @descripcion",
                    idDestinoParam, montoParam, descParam);

                // 5. Si todo salió bien, preparar la respuesta de ÉXITO
                var response = new IngresoExternoResponse
                {
                    IdTransaccionInterna = -1, // No podemos obtener esto fácilmente
                    IdTransaccionExterna = request.IdTransaccionExterna,
                    Estado = "Aprobada",
                    FechaHora = DateTime.Now,
                    MontoConfirmado = request.Monto,
                    Mensaje = "El ingreso fue acreditado exitosamente."
                };

                return Ok(response); // Devuelve un HTTP 200 OK
            }
            catch (SqlException ex)
            {
                // 6. El Stored Procedure falló (Error 50001, etc.)
                return Content(HttpStatusCode.BadRequest, new { Message = "Error de Negocio: " + ex.Message });
            }
            catch (Exception ex)
            {
                // Cualquier otro error inesperado en C#
                return InternalServerError(ex); // Devuelve un HTTP 500
            }
        }
    }
}