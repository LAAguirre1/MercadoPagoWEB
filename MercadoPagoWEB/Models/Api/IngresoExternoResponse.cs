using System;

namespace MercadoPagoWEB.Models.Api
{
    /// <summary>
    /// Representa la respuesta JSON que enviamos al socio externo
    /// si el ingreso fue exitoso.
    /// </summary>
    public class IngresoExternoResponse
    {
        public int IdTransaccionInterna { get; set; }
        public string IdTransaccionExterna { get; set; }
        public string Estado { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal MontoConfirmado { get; set; }
        public string Mensaje { get; set; }
    }
}