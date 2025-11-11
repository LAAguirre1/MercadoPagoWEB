using MercadoPagoWEB.Models; 
using System.Threading.Tasks;

namespace MercadoPagoWEB.Services
{
    /// <summary>
    /// Interfaz para el servicio de transferencias.
    /// </summary>
    public interface ITransferenciaService
    {
        /// <summary>
        /// Ejecuta la lógica de negocio para realizar una transferencia.
        /// </summary>
        /// <param name="model">Datos de la transferencia (CVU/Alias, monto).</param>
        /// <param name="idUsuarioOrigen">ID del usuario que está enviando el dinero.</param>
        /// <returns>Task completado.</returns>
        Task RealizarTransferenciaAsync(TransferenciaViewModel model, int idUsuarioOrigen);
    }
}