using System.ComponentModel.DataAnnotations; // Para validaciones

namespace MercadoPagoWEB.Models.Api
{
    /// <summary>
    /// Representa el JSON que esperamos recibir de un cliente externo
    /// para procesar un pago o transferencia.
    /// </summary>
    public class PagoExternoRequest
    {
        [Required(ErrorMessage = "El Monto es obligatorio.")]
        [Range(1.00, 1000000.00, ErrorMessage = "El monto debe ser positivo y estar entre 1.00 y 1,000,000.00")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El Identificador de Origen (CVU/Alias) es obligatorio.")]
        [StringLength(50)]
        public string IdentificadorOrigen { get; set; }

        [Required(ErrorMessage = "El Identificador de Destino (CVU/Alias) es obligatorio.")]
        [StringLength(50)]
        public string IdentificadorDestino { get; set; }

        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
        public string Descripcion { get; set; }
    }
}