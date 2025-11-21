using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class Foto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Base64String { get; set; }
        public string Nombre { get; set; }
        public bool Enviado { get; set; }
    }
}
