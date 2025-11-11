using System.ComponentModel.DataAnnotations;

namespace MercadoPagoWEB.Models
{
    /// <summary>
    /// ViewModel para capturar los datos del formulario de transferencia.
    /// </summary>
    public class TransferenciaViewModel
    {
        [Required(ErrorMessage = "El CVU o Alias de destino es obligatorio.")]
        [Display(Name = "CVU o Alias del Destinatario")]
        [StringLength(50)]
        public string CvuOAliasDestino { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 1000000.00, ErrorMessage = "El monto debe ser un valor positivo.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto a Transferir")]
        public decimal Monto { get; set; }

        [StringLength(255, ErrorMessage = "La descripción no puede superar los 255 caracteres.")]
        [Display(Name = "Descripción (Opcional)")]
        public string Descripcion { get; set; }
    }
}