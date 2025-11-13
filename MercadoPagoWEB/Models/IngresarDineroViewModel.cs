using System.ComponentModel.DataAnnotations;

namespace MercadoPagoWEB.Models
{
    /// <summary>
    /// ViewModel para el formulario simple de ingreso de dinero.
    /// </summary>
    public class IngresarDineroViewModel
    {
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1.00, 500000.00, ErrorMessage = "Puedes ingresar entre $1,00 y $500.000,00.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto a Ingresar")]
        public decimal Monto { get; set; }

        // Simulación de un método de pago (tarjeta)
        // En una app real, aquí irían los datos de la tarjeta o un token.
        [Required(ErrorMessage = "Debe seleccionar un método de pago simulado.")]
        [Display(Name = "Método de Pago (Simulado)")]
        public string MetodoPagoSimulado { get; set; }
    }
}