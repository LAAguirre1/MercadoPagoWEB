using System;

namespace MercadoPagoWEB.Models.Api
{
    /// <summary>
    /// Representa la respuesta JSON que enviaremos al cliente externo
    /// si la transacción fue exitosa.
    /// </summary>
    public class PagoExternoResponse
    {
        public int IdTransaccion { get; set; }
        public string Estado { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal MontoConfirmado { get; set; }
        public string Descripcion { get; set; }
        public string Mensaje { get; set; }
    }
}