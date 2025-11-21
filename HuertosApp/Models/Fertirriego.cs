using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace HuertosApp.Models
{
    /// <summary>
    /// Representa un registro de fertirriego.
    /// </summary>
    public class Fertirriego
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Huerto { get; set; } 
        public string EtapaFertilizacion { get; set; } 
        public string Anio { get; set; }
        public string Fertilizantes { get; set; }
        public string TipoRiego { get; set; } 
        public string Sector { get; set; } 
        public string FechaRiego { get; set; }
        public string TiempoRiego { get; set; } 
        public string M3Sistema { get; set; } 
        public string MmSistema { get; set; } 
        public string EVP_Prom { get; set; } 
        public string FertilizanteEst1 { get; set; } 
        public string FertilizanteEst2 { get; set; }
        public string FertilizanteEst3 { get; set; } 
        public string FertilizanteKg { get; set; } 
        public string Observacion { get; set; } 
        public int? UsuarioId { get; set; }
        public string NombreUsuario { get; set; }    // Nombre asociado al usuario
        // ID del usuario que creó el registro
                                                     // Nueva propiedad para asociar la foto
        public string NombreFoto { get; set; } // Puede ser nula si no hay foto
        public bool Enviado { get; set; } = false;  // Estado de transmisión
    }
}
