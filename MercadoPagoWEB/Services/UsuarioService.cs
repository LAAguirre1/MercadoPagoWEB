using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MercadoPagoWEB.Services
{
    public class UsuarioService
    {
        private readonly MercadoPagoDBEntities1 db = new MercadoPagoDBEntities1();

        /// <summary>
        /// Actualiza el campo 'estado_kyc' del usario en la base de datos.
        /// </summary>
        /// <param name="usuarioId">El ID del usuario cuya cuenta se va a actualizar</param>
        /// <param name="nuevoEstado">El nuevo estado (ej: "Verificado", "Rechazado"</param>
        public void ActualizarEstadoKYC(int usuarioId, string nuevoEstado)
        {
            try
            {
                var usuario = db.Usuario.Find(usuarioId);

                if (usuario != null)
                {
                    usuario.estado_kyc = nuevoEstado;
                    db.SaveChanges();
                }
                else
                {
                    throw new ApplicationException($"Usuario con ID {usuarioId} no encontrado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el estado KYC en la base de datos.", ex);
            }
        }
    }
}