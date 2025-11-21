using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class RegistroComercial
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }                      // Identificador único
        public string Fecha { get; set; }               // Fecha de registro
        public string Genotipo { get; set; }            // Genotipo (Ej: Mezcla)
        public string Temporada { get; set; }           // Temporada (2024 o 2025)
        public int Predio { get; set; }                 // Número de predio
        public int Rodal { get; set; }                  // Número de rodal
        public string Kilos { get; set; }              // Cantidad en kilos
        public string Cosechador { get; set; }          // Cosechador (selección de lista)
        public string Modalidad { get; set; }           // Modalidad (Volteo o Gavillas)
        public int ArbolesDia { get; set; }             // Número de árboles por día
        public int TotalArboles { get; set; }           // Total de árboles cosechados
        public int? UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public bool Enviado { get; set; } = false;  // Estado de transmisión
    }
}
