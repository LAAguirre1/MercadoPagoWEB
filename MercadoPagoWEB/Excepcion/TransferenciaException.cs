using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MercadoPagoWEB.Excepcion
{
    /// <summary>
    /// Excepción personalizada para manejar errores de negocio durante la transferencia.
    /// </summary>
    public class TransferenciaException : Exception
    {
        public TransferenciaException(string message) : base(message)
        {
        }
    }
}