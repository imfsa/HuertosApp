using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class CosechaPendiente
    {
        public string rc_id { get; set; }
        public string tree_id { get; set; }
        public string fecha_cosecha { get; set; }
        public string genotipo { get; set; }
        public string temporada { get; set; }
        public string especie { get; set; }
        public string replica { get; set; }
        public string fila { get; set; }
        public string columna { get; set; }
        public string predio { get; set; }
        public string huerto_nombre { get; set; }
        public string cod_huerto { get; set; }
        public string kilos { get; set; }
        public string cosechador { get; set; }
        public string despachado { get; set; }
        public string despacho_id { get; set; }
        public string created_at { get; set; }

        // Para selección en la UI
        public bool IsSelected { get; set; }
    }

    public class PendientesResponse
    {
        public bool success { get; set; }
        public List<CosechaPendiente> data { get; set; }
    }

}
