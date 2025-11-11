using System.ComponentModel.DataAnnotations;

namespace MercadoPagoWEB.Models
{
    /// <summary>
    /// ViewModel para el formulario de Login (Iniciar Sesión).
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del Email no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// ViewModel para el formulario de Registro.
    /// Combina los campos de Identity (Email/Password) y los de tu tabla Usuario (Nombre, Apellido, DNI).
    /// </summary>
    public class RegisterViewModel
    {
        // --- Campos para Identity (AspNetUsers) ---

        [Required(ErrorMessage = "El Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del Email no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        // --- Campos para tu tabla 'Usuario' (Perfil de Negocio) ---

        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(20)] // Coincide con tu base de datos (VARCHAR 20)
        [Display(Name = "DNI")]
        public string DNI { get; set; }
    }
}