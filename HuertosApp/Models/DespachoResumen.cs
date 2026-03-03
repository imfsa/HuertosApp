using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class DespachoResumen
    {
        // Ya lo cambiamos antes
        public string despacho_id { get; set; } = string.Empty;

        public string folio { get; set; } = string.Empty;
        public string fecha_desp { get; set; } = string.Empty;

        public int total_arboles { get; set; }

        // 🔴 CAMBIO IMPORTANTE: de double -> string
        public string total_kilos { get; set; } = "0";

        public string documento { get; set; } = string.Empty;

        public string FolioMostrar
        {
            get
            {
                // Ej: "Folio 19 - DESP-20251126051936"
                return $"Folio {despacho_id} - {folio}";
            }
        }
    }
}

