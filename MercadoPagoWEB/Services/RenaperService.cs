using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;

public class RenaperService
{
    private readonly HttpClient _client;
    // private string _apiKey; // Almacenamos la clave dinámicamente

    public RenaperService()
    {
        // 1. Inicializar HttpClient
        string baseUrl = ConfigurationManager.AppSettings["RenaperApiBaseUrl"];
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Accept.Clear();

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
    /// POST para obtener la clave API.
    /// </summary>
    /// <returns>Clave API como string o null si falla.</returns>
    public async Task<string> GetApiKeyAsync()
    {
        string mail = ConfigurationManager.AppSettings["RenaperRequestMail"];
        // Convertimos el string de Web.config a entero
        if (!int.TryParse(ConfigurationManager.AppSettings["RenaperRequestLimit"], out int limit))
        {
            limit = 100; // Valor por defecto si falla la conversión
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
                // La respuesta es la clave API en texto plano
                string apiKey = await response.Content.ReadAsStringAsync();

                // Limpiar espacios en blanco o comillas residuales que pueda devolver la API
                return apiKey.Trim().Trim('"');
            }
            else
            {
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
    /// Realiza la verificación de identidad (KYC) llamando al servicio externo.
    /// </summary>
    /// <param name="cuil">CUIL completo (ej: 20-30123456-7) a verificar.</param>
    public async Task<bool> VerificarIdentidad(string cuil, string apiKey)
    {
        // 1. Establecer la clave API para esta solicitud (si aún no está)
        // El cliente HttpClient debe ser reconfigurado si la clave cambia. 
        // Para simplificar, la enviamos por parámetro.

        // Si la clave no está presente o es nula, no podemos continuar
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: La clave API es nula o no fue obtenida.");
            return false;
        }

        // 2. Limpiar Headers anteriores y añadir la clave
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

        // 3. Endpoint de verificación
        string endpoint = $"/api/personas/por-cuil/{cuil}";

        try
        {
            HttpResponseMessage response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                // El servicio devuelve 200 OK si la persona existe y es válida.
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // El CUIL no existe.
                return false;
            }
            else
            {
                // Otro error (ej: clave inválida, error del servidor)
                Console.WriteLine($"Error de servicio: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fallo en la comunicación: {ex.Message}");
            return false;
        }
    }
}