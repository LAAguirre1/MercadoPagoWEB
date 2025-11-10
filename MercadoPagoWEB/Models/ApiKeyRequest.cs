using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MercadoPagoWEB.Models
{
    public class ApiKeyRequest
    {
        public string mail { get; set; }
        public int solicitudesSemanales { get; set; }
    }
}