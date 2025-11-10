using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace MercadoPagoWEB.Models
{
    public class BilleteraViewModel
    {
        // Datos del Usuario
        public string NombreCompleto { get; set; }
        public string EstadoKYC { get; set; }

        public int IdUsuario { get; set; }

        // Datos de la Cuenta Digital
        public decimal SaldoActual { get; set; }
        public string CVU { get; set; }
        public string CVU_Alias { get; set; }
        public string Moneda { get; set; }

        // Datos de Métodos de Pago
        public List <MetodoPagoSimpleViewModel> MetodosPago { get; set; }
    }

    public class MetodoPagoSimpleViewModel
    {
        public string TipoMetodo { get; set; }
        public string BancoEmisor { get; set; }
        public string Ultimos4Digitos { get; set; }
    }
}