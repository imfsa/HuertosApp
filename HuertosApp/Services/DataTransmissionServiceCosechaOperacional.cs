using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HuertosApp.Models;

namespace HuertosApp.Services
{
    public static class DataTransmissionServiceCosechaOperacional
    {
        private static readonly HttpClient client = new HttpClient();

        private const string Url =
            "https://api.imf.cl:8443/huertosappV2/registro_cosecha_operacional.php";

        public static async Task<(bool ok, int enviados, string? error)>
            TransmitirLoteAsync(IEnumerable<RegistroCosecha> registros)
        {
            try
            {
                var lista = registros?.ToList() ?? new List<RegistroCosecha>();
                if (!lista.Any())
                    return (true, 0, null);

                // Mapear exactamente a los nombres que espera PHP
                var payload = lista.Select(r => new
                {
                    tree_id = r.TreeId,
                    fecha_cosecha = r.FechaCosecha,
                    temporada = r.Temporada,
                    genotipo = r.Genotipo,
                    especie = r.Especie,
                    replica = r.Replica,
                    fila = r.Fila,
                    columna = r.Columna,
                    predio = r.Predio,
                    cod_huerto = r.CodHuerto,
                    huerto_nombre = r.HuertoNombre,
                    kilos = r.Kilos,
                    cosechadores = r.Cosechador,                      
                    UsuarioId = r.UsuarioId ?? App.CurrentUser.Id  // 👈 ID usuario
                }).ToList();

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(Url, content);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, 0,
                        $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
                }

                var body = await response.Content.ReadAsStringAsync();

                // Limpiar espacios y BOM
                body = body.TrimStart('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

                // Si comienza con '<' es HTML o error del servidor → no intentamos parsear JSON
                if (body.StartsWith("<"))
                {
                    return (false, 0,
                        "El servidor devolvió HTML en vez de JSON. Respuesta:\n" + body);
                }

                Dictionary<string, JsonElement>? dict;
                try
                {
                    dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);
                }
                catch (Exception ex)
                {
                    // Este es precisamente el error que veías: '<' invalid start...
                    return (false, 0,
                        "No se pudo interpretar la respuesta JSON: " + ex.Message +
                        "\nRespuesta cruda del servidor:\n" + body);
                }

                var success = dict != null &&
                              dict.TryGetValue("success", out var s) &&
                              s.ValueKind == JsonValueKind.True;

                var insertados = 0;
                if (dict != null &&
                    dict.TryGetValue("insertados", out var ins) &&
                    ins.ValueKind == JsonValueKind.Number)
                {
                    insertados = ins.GetInt32();
                }

                return (success, insertados, success ? null : body);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }
    }
}
