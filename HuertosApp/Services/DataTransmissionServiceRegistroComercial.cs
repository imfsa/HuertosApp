using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HuertosApp.Models;

namespace HuertosApp.Services
{
    public static class DataTransmissionServiceRegistroComercial
    {
        // Cliente HTTP para las solicitudes
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Transmite los datos de un registro comercial al servidor.
        /// </summary>
        /// <param name="registro">El registro comercial a transmitir.</param>
        /// <returns>True si la transmisión fue exitosa; de lo contrario, False.</returns>
        public static async Task<bool> TransmitirDatosAsync(RegistroComercial registro)
        {
            try
            {
                // Mostrar en consola el inicio de la transmisión
                Console.WriteLine($"Iniciando transmisión para el registro con ID {registro.ID}");

                // Serializar el objeto registro a formato JSON
                var jsonContent = JsonSerializer.Serialize(registro);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // URL del servicio para Registro Comercial
                var url = "https://api.imf.cl:8443/huertosapp/registro_comercial.php";

                // Enviar solicitud POST al servidor
                var response = await client.PostAsync(url, content);

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer y procesar la respuesta del servidor
                    var responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta del servidor: {responseData}");

                    // Deserializar la respuesta JSON
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseData);

                    // Comprobar si el campo "success" está presente y es verdadero
                    return result.ContainsKey("success");
                }
                else
                {
                    // Log en caso de error HTTP
                    Console.WriteLine($"Error transmitiendo el registro: Código {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Capturar y mostrar excepciones durante la transmisión
                Console.WriteLine($"Excepción durante la transmisión: {ex.Message}");
                return false;
            }
        }
    }
}
