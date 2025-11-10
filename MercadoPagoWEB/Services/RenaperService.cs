using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Web.Caching; // Asegúrate de incluir este using para Cache

public class RenaperService
{
    private readonly HttpClient _client;
    private const string ApiKeyCacheKey = "RenaperApiKeyCache";

    public RenaperService()
    {
        // 1. Inicializar HttpClient
        string baseUrl = ConfigurationManager.AppSettings["RenaperApiBaseUrl"];
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Accept.Clear();

        // Configuramos para aceptar texto plano, que es lo que devuelve la API para el token y la respuesta
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        _client.Timeout = TimeSpan.FromSeconds(15);
    }

    // Estructura de la solicitud POST para la clave
    private class ApiKeyRequest
    {
        public string mail { get; set; }
        public int solicitudesSemanales { get; set; }
    }

    /// <summary>
    /// Intenta obtener la clave de la caché o la solicita al servicio RENAPER si no existe.
    /// Esta función maneja el error 500 forzado por la API al solicitar la clave repetidas veces.
    /// </summary>
    public async Task<string> GetApiKeyAsync()
    {
        // 1. INTENTAR OBTENER DEL WEB.CONFIG (Persistencia Larga)
        string persistentKey = ConfigurationManager.AppSettings["RenaperActiveApiKey"];
        if (!string.IsNullOrEmpty(persistentKey))
        {
            Console.WriteLine("Usando clave API persistente de Web.config.");
            return persistentKey;
        }

        // 2. Si no existe en Web.config, realizar la solicitud POST (Solo la primera vez)
        string mail = ConfigurationManager.AppSettings["RenaperRequestMail"];
        if (!int.TryParse(ConfigurationManager.AppSettings["RenaperRequestLimit"], out int limit))
        {
            limit = 100;
        }

        var requestBody = new ApiKeyRequest
        {
            mail = mail,
            solicitudesSemanales = limit
        };

        string json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _client.PostAsync("/api/keys", content);

            if (response.IsSuccessStatusCode)
            {
                // ÉXITO: Obtuvimos una clave nueva
                string apiKey = (await response.Content.ReadAsStringAsync()).Trim().Trim('"');

                // 3. ALMACENAR LA CLAVE EN WEB.CONFIG
                SaveKeyToWebConfig(apiKey);

                return apiKey;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                 response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Si falla con 500 o 400 (y no hay clave persistente), es un error de la API
                Console.WriteLine("Error crítico de la API: No se pudo obtener la clave. El token debe ser provisto manualmente en Web.config.");
                return null;
            }
            else
            {
                // Otro error HTTP
                Console.WriteLine($"Error al obtener la clave: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error de conexión al obtener la clave: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Método auxiliar para guardar el token en Web.config de forma persistente.
    /// </summary>
    private void SaveKeyToWebConfig(string apiKey)
    {
        try
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // Usa OpenWebConfiguration si estás en un proyecto web moderno
            // Para MVC tradicional, OpenExeConfiguration funciona con ajustes.

            var appSettings = config.AppSettings.Settings;

            // Si la clave existe, la actualiza; si no, la añade
            if (appSettings["RenaperActiveApiKey"] == null)
            {
                appSettings.Add("RenaperActiveApiKey", apiKey);
            }
            else
            {
                appSettings["RenaperActiveApiKey"].Value = apiKey;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            Console.WriteLine("Nueva clave API guardada en Web.config.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ADVERTENCIA: No se pudo guardar la clave en Web.config. Error: {ex.Message}");
        }
    }

    // ----------------------------------------------------------------------------------
    // Método de Verificación de Identidad (KYC)
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Realiza la verificación de identidad (KYC) llamando al servicio externo.
    /// </summary>
    /// <param name="cuil">CUIL completo (ej: 20-30123456-7) a verificar.</param>
    /// <param name="apiKey">La clave API obtenida previamente.</param>
    public async Task<bool> VerificarIdentidad(string cuil, string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(cuil))
        {
            Console.WriteLine("Error: Clave API o CUIL no pueden estar vacíos.");
            return false;
        }

        //string cuilEncoded = Uri.EscapeDataString(cuil);
        string endpoint = $"/api/personas/por-cuil/{cuil}";

        try
        {
            // Usamos SendAsync para poder adjuntar el Header X-API-Key a la solicitud GET
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint))
            {
                requestMessage.Headers.Add("X-API-Key", apiKey);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                // Realizar la llamada GET
                HttpResponseMessage response = await _client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    // Código 200 OK: CUIL encontrado y válido.
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                         response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // 404/400: No encontrado o formato de CUIL incorrecto. No verificado.
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Verificación fallida para CUIL {cuil}. Código: {response.StatusCode}. Detalle: {errorDetail}");
                    return false;
                }
                else
                {
                    // Otros errores (Ej: 401 Unauthorized por clave vencida/mala, 500)
                    Console.WriteLine($"Error de servicio RENAPER. Código: {response.StatusCode}.");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fallo en la comunicación: {ex.Message}");
            return false;
        }
    }
}