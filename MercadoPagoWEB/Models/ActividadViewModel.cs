using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MercadoPagoWEB.Models
{
    /// <summary>
    /// ViewModel para mostrar un item en la lista de actividad/transacciones.
    /// </summary>
    public class ActividadViewModel
    {
        public int IdTransaccion { get; set; }
        public DateTime FechaHora { get; set; }
        public string TipoTransaccion { get; set; }
        public string Descripcion { get; set; }

        /// <summary>
        /// El monto que afectó a la cuenta (puede ser positivo o negativo)
        /// </summary>
        public decimal MontoAfectado { get; set; }

        /// <summary>
        /// Propiedad de ayuda para la Vista.
        /// Si es true, el monto es positivo (ingreso).
        /// Si es false, el monto es negativo (egreso).
        /// </summary>
        public bool EsIngreso
        {
            get { return this.MontoAfectado >= 0; }
        }

        /// <summary>
        /// Propiedad de ayuda para la clase CSS de Bootstrap
        /// </summary>
        public string ClaseCssMonto
        {
            get { return this.EsIngreso ? "text-success" : "text-danger"; }
        }

        /// <summary>
        /// Propiedad de ayuda para el ícono
        /// </summary>
        public string IconoCss
        {
            get
            {
                if (this.TipoTransaccion == "Transferencia" && !this.EsIngreso)
                {
                    return "fa-solid fa-arrow-up text-danger"; // Egreso
                }
                if (this.TipoTransaccion == "Transferencia" && this.EsIngreso)
                {
                    return "fa-solid fa-arrow-down text-success"; // Ingreso
                }
                if (this.TipoTransaccion == "Ingreso")
                {
                    return "fa-solid fa-wallet text-primary"; // Ingreso de dinero
                }

                return "fa-solid fa-dollar-sign"; // Default
            }
        }
    }
}