using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }               // ID único del usuario
        public string Nombre { get; set; }       // Nombre de usuario
        public string Password { get; set; }     // Contraseña del usuario
        public string Estado { get; set; }       // Estado (Activo, Inactivo, etc.)
    }
}
