using System.ComponentModel.DataAnnotations;

namespace MercadoPagoWEB.Models.Api
{
    /// <summary>
    /// Representa el JSON que recibimos de un socio (ej. Ualá)
    /// para acreditar un ingreso en una de nuestras cuentas.
    /// </summary>
    public class IngresoExternoRequest
    {
        [Required(ErrorMessage = "El Monto es obligatorio.")]
        [Range(1.00, 1000000.00, ErrorMessage = "El monto debe ser positivo.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El Identificador de Destino (CVU/Alias) es obligatorio.")]
        [StringLength(50)]
        public string IdentificadorDestino { get; set; }

        [Required(ErrorMessage = "Se requiere una referencia externa de la transacción.")]
        [StringLength(100)]
        public string IdTransaccionExterna { get; set; } // ¡Importante para rastreo!

        [StringLength(100)]
        public string Descripcion { get; set; }
    }
}