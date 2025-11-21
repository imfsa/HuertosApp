using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HuertosApp.Models
{
    [Table("RegistroCosecha")]
    public class RegistroCosecha
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }          // Id local

        public long TreeId { get; set; }     // id árbol (desde QR)

        public string FechaCosecha { get; set; } = "";   // yyyy-MM-dd

        public int Temporada { get; set; }
        public string Genotipo { get; set; } = "";
        public string Especie { get; set; } = "";
        public int Replica { get; set; }
        public int Fila { get; set; }
        public int Columna { get; set; }
        public string Predio { get; set; } = "";
        public int CodHuerto { get; set; }
        public string HuertoNombre { get; set; } = "";

        public decimal Kilos { get; set; }   // 0 – 100
        public string Cosechador { get; set; } = ""; // iniciales separados por coma

        public bool Despachado { get; set; } = false;    // estado de despacho
        public long? DespachoId { get; set; }            // se llenará cuando se despache

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
