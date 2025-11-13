using MercadoPagoWEB.Filters; // Para nuestro [ApiKeyAuthorize]
using MercadoPagoWEB.Models; // Para MercadoPagoDBEntities1 y CuentaDigital
using MercadoPagoWEB.Models.Api; // Para nuestros DTOs
using System;
using System.Data.SqlClient; // Para la excepción de SQL y los parámetros
using System.Net;
using System.Threading.Tasks; // Para operaciones Asíncronas
using System.Web.Http; // IMPORTANTE: Usar System.Web.Http
using System.Data.Entity; // Para .FirstOrDefaultAsync()

namespace MercadoPagoWEB.Controllers.Api
{
    /// <summary>
    /// API pública para que aplicaciones externas puedan realizar pagos.
    /// </summary>
    [AllowAnonymous]
    [ApiKeyAuthorize] // ¡El "guardia de seguridad" protege a TODO este controlador!
    [RoutePrefix("api/v1/pagos")] // La URL base será /api/v1/pagos
    public class PagosController : ApiController
    {
        // Tu contexto de base de datos de siempre
        private MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        /// <summary>
        /// Procesa una solicitud de transferencia externa entre dos cuentas (CVU o Alias)
        /// </summary>
        [HttpPost] // Este método solo responde a solicitudes POST
        [Route("")]  // La ruta completa es /api/v1/pagos
        public async Task<IHttpActionResult> RealizarPago([FromBody] PagoExternoRequest request)
        {
            // 1. Validar que el JSON recibido es válido (ej. campos requeridos, rangos)
            if (!ModelState.IsValid)
            {
                // Si falla, devuelve un error 400 con la lista de problemas
                return BadRequest(ModelState);
            }

            // 2. Validar que no sea un auto-pago
            if (request.IdentificadorOrigen.Equals(request.IdentificadorDestino, StringComparison.OrdinalIgnoreCase))
            {
                return Content(HttpStatusCode.BadRequest,
                    new { Message = "Error de Lógica: El Identificador de Origen y Destino no pueden ser el mismo." });
            }

            try
            {
                // 3. Buscar las cuentas en la BD (por CVU o Alias)
                // Usamos FirstOrDefaultAsync para no bloquear el servidor
                var cuentaOrigen = await db.CuentaDigital
                    .FirstOrDefaultAsync(c => c.cvu == request.IdentificadorOrigen || c.alias == request.IdentificadorOrigen);

                var cuentaDestino = await db.CuentaDigital
                    .FirstOrDefaultAsync(c => c.cvu == request.IdentificadorDestino || c.alias == request.IdentificadorDestino);

                // 4. Validar que las cuentas existan
                if (cuentaOrigen == null)
                {
                    return Content(HttpStatusCode.NotFound,
                        new { Message = $"La cuenta de origen '{request.IdentificadorOrigen}' no fue encontrada." });
                }

                if (cuentaDestino == null)
                {
                    return Content(HttpStatusCode.NotFound,
                        new { Message = $"La cuenta de destino '{request.IdentificadorDestino}' no fue encontrada." });
                }

                // 5. ¡Ejecutar el Stored Procedure que ya teníamos!

                // Preparamos los parámetros para el SP
                var idOrigenParam = new SqlParameter("@id_origen", cuentaOrigen.id_cuenta);
                var idDestinoParam = new SqlParameter("@id_destino", cuentaDestino.id_cuenta);
                var montoParam = new SqlParameter("@monto", request.Monto);

                // Manejo de la descripción opcional (DBNull si es nulo)
                var descParam = new SqlParameter("@descripcion", (object)request.Descripcion ?? DBNull.Value);

                // Ejecutamos el SP de forma asíncrona
                await db.Database.ExecuteSqlCommandAsync(
                    "EXEC SP_RealizarTransferencia @id_origen, @id_destino, @monto, @descripcion",
                    idOrigenParam, idDestinoParam, montoParam, descParam);

                // 6. Si todo salió bien, preparar la respuesta de ÉXITO
                var response = new PagoExternoResponse
                {
                    IdTransaccion = -1, // No podemos obtener el ID de SCOPE_IDENTITY() fácilmente con ExecuteSqlCommandAsync,
                                        // lo dejamos en -1 para indicar que es una API. Opcional.
                    Estado = "Aprobada",
                    FechaHora = DateTime.Now,
                    MontoConfirmado = request.Monto,
                    Descripcion = request.Descripcion ?? "Transferencia Externa",
                    Mensaje = "La transferencia fue procesada exitosamente."
                };

                return Ok(response); // Devuelve un HTTP 200 OK con el JSON de respuesta
            }
            catch (SqlException ex)
            {
                // 7. ¡El Stored Procedure falló! (Ej: Saldo insuficiente)
                // Capturamos el error que lanzamos con RAISERROR
                if (ex.Message.Contains("Saldo insuficiente") || ex.Message.Contains("inválida"))
                {
                    // Error de negocio (ej. Error 50001 de SQL)
                    return Content(HttpStatusCode.BadRequest, new { Message = "Error de Negocio: " + ex.Message });
                }

                // Error de SQL genérico
                return Content(HttpStatusCode.InternalServerError, new { Message = "Error en la base de datos.", Detalle = ex.Message });
            }
            catch (Exception ex)
            {
                // Cualquier otro error inesperado en C#
                return InternalServerError(ex); // Devuelve un HTTP 500
            }
        }
    }
}