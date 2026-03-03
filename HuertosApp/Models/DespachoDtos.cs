using System.Text.Json.Serialization;

namespace HuertosApp.Models
{
    public class CosechaPendienteDto
    {
        [JsonPropertyName("rc_id")]
        public int RcId { get; set; }

        [JsonPropertyName("tree_id")]
        public long TreeId { get; set; }

        [JsonPropertyName("fecha_cosecha")]
        public string FechaCosecha { get; set; } = "";

        [JsonPropertyName("genotipo")]
        public string Genotipo { get; set; } = "";

        [JsonPropertyName("temporada")]
        public int Temporada { get; set; }

        [JsonPropertyName("especie")]
        public string Especie { get; set; } = "";

        [JsonPropertyName("replica")]
        public int Replica { get; set; }

        [JsonPropertyName("fila")]
        public int Fila { get; set; }

        [JsonPropertyName("columna")]
        public int Columna { get; set; }

        [JsonPropertyName("predio")]
        public string Predio { get; set; } = "";

        [JsonPropertyName("huerto_nombre")]
        public string HuertoNombre { get; set; } = "";

        [JsonPropertyName("cod_huerto")]
        public int CodHuerto { get; set; }

        [JsonPropertyName("kilos")]
        public decimal Kilos { get; set; }

        [JsonPropertyName("cosechador")]
        public string Cosechador { get; set; } = "";
    }

    public class CosechaPendienteVm : CosechaPendienteDto
    {
        public bool IsSelected { get; set; }
    }

    public class RootPendientes
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<CosechaPendienteDto> Data { get; set; } = new();
    }


    // Respuesta estimada de crear_despacho.php
    public class CrearDespachoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        // Ej: número de folio generado
        [JsonPropertyName("folio")]
        public int Folio { get; set; }

        // URL al Excel generado (ajústalo al nombre real que devuelva PHP)
        [JsonPropertyName("file_url")]
        public string? FileUrl { get; set; }
    }


    public class DespachoHeaderDto
    {
        [JsonPropertyName("folio")]
        public int Folio { get; set; }

        [JsonPropertyName("fecha_despacho")]
        public string FechaDespacho { get; set; } = "";

        [JsonPropertyName("especie")]
        public string Especie { get; set; } = "";

        [JsonPropertyName("temporada")]
        public string Temporada { get; set; } = "";

        [JsonPropertyName("predio")]
        public string Predio { get; set; } = "";

        [JsonPropertyName("huerto")]
        public string Huerto { get; set; } = "";

        [JsonPropertyName("total_kilos")]
        public decimal TotalKilos { get; set; }

        [JsonPropertyName("file_url")]
        public string? FileUrl { get; set; }
    }

    public class RootDespachos
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<DespachoHeaderDto> Data { get; set; } = new();
    }
}
