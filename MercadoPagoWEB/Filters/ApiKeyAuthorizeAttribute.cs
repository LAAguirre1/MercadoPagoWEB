using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers; // Para HttpActionContext
using System.Web.Http.Filters; // Para ActionFilterAttribute

namespace MercadoPagoWEB.Filters
{
    /// <summary>
    /// Filtro de ACCIÓN personalizado para Web API que valida una API Key
    /// enviada en el header "Authorization: Bearer [API_KEY]".
    /// Se ejecuta DESPUÉS de [AllowAnonymous] pero ANTES de la acción.
    /// </summary>
    public class ApiKeyAuthorizeAttribute : ActionFilterAttribute // <-- ¡CAMBIO IMPORTANTE!
    {
        // Sobrescribimos el método OnActionExecuting
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // 1. Intentar leer el header 'Authorization'
            var authHeader = actionContext.Request.Headers.Authorization;

            // 2. Verificar si el header existe y tiene el formato "Bearer [clave]"
            if (authHeader == null || authHeader.Scheme.ToLower() != "bearer" || string.IsNullOrEmpty(authHeader.Parameter))
            {
                // Si no existe o el formato es incorrecto, rechazar la solicitud
                HandleUnauthorized(actionContext, "Header 'Authorization' inválido o ausente. Use el formato 'Bearer [API_KEY]'.");
                return;
            }

            // 3. Obtener la API Key enviada por el cliente
            string apiKeyDelCliente = authHeader.Parameter;

            // 4. Obtener la lista de claves válidas desde el Web.config
            string clavesValidasConfig = ConfigurationManager.AppSettings["ValidApiKeys"];

            if (string.IsNullOrEmpty(clavesValidasConfig))
            {
                // Error de configuración en NUESTRO servidor
                HandleUnauthorized(actionContext, "Error de configuración interna del servidor de API Keys.");
                return;
            }

            // 5. Convertir la lista de claves (separadas por coma) en una lista real
            List<string> listaClavesValidas = clavesValidasConfig.Split(',').Select(k => k.Trim()).ToList();

            // 6. Validar si la clave del cliente está en nuestra lista de claves válidas
            if (listaClavesValidas.Contains(apiKeyDelCliente))
            {
                // ¡Éxito! La clave es válida. Dejar que la acción continúe.
                base.OnActionExecuting(actionContext);
            }
            else
            {
                // ¡Fracaso! La clave enviada no es válida.
                HandleUnauthorized(actionContext, "API Key inválida.");
            }
        }

        /// <summary>
        /// Método helper para enviar una respuesta 401 Unauthorized (No Autorizado)
        /// </summary>
        private void HandleUnauthorized(HttpActionContext actionContext, string mensaje)
        {
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                new { Message = "Acceso No Autorizado: " + mensaje } // Cuerpo de la respuesta
            );
        }
    }
}