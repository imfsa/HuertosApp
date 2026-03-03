using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuertosApp.Models
{
    public class DespachosResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public List<DespachoResumen> data { get; set; } = new();
    }
}
