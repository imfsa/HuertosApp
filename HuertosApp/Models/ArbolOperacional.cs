using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace HuertosApp.Models
{
    public class ArbolOperacional
    {
        [PrimaryKey]
        public string TreeId { get; set; } = string.Empty;
        public string Temporada { get; set; } = string.Empty;
        public string Genotipo { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string Replica { get; set; } = string.Empty;
        public string Fila { get; set; } = string.Empty;
        public string Columna { get; set; } = string.Empty;
        public string Predio { get; set; } = string.Empty;
        public string CodHuerto { get; set; } = string.Empty;
        public string HuertoNombre { get; set; } = string.Empty;
    }

}

