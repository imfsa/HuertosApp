using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HuertosApp.Models;

namespace HuertosApp.Services
{
    public static class DataTransmissionServiceFertirriego
    {
        private static readonly HttpClient client = new HttpClient();

        /// 
        /// Transmite los datos de un registro Fertirriego al servidor.
        /// 
        /// El registro de Fertirriego a transmitir.
        ///True si la transmisión fue exitosa; de lo contrario, False.
        public static async Task<bool> TransmitirDatosAsync(Fertirriego registro)
        {
            try
            {
                Console.WriteLine($"Iniciando transmisión para el registro con ID {registro.ID}");

                // Serializar el objeto registro a JSON
                var jsonContent = JsonSerializer.Serialize(registro);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // URL del servicio para Fertirriego
                var url = "http://179.61.13.234:8089/ws_huertosapp/registro_fertirriego.php";
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta del servidor: {responseData}");

                    // Analizar la respuesta del servidor
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseData);

                    // Verificar si la clave "success" existe y es true
                    return result.ContainsKey("success");
                }
                else
                {
                    Console.WriteLine($"Error transmitiendo el registro: Código {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción durante la transmisión: {ex.Message}");
                return false;
            }
        }

        //public static async void SendPhoto(){

        //    var fotosNoEnviadas = await Database.GetFotosNoEnviadasAsync();

        //    if (fotosNoEnviadas.Count > 0)
        //    {
        //        bool allSentSuccessfully = true;

        //        foreach (var foto in fotosNoEnviadas)
        //        {
        //            var data = new Dictionary<string, string>
        //                {
        //                    {"imagen", foto.Base64String},
        //                    {"nombre", foto.Nombre}
        //                };
        //            var content = new FormUrlEncodedContent(data);

        //            try
        //            {
        //                var response = await client.PostAsync("http://179.61.13.234:8089/ws_huertosapp/recibir_fotos.php", content);

        //                if (response.IsSuccessStatusCode)
        //                {
        //                    foto.Enviado = true;
        //                    await Database.UpdatePhotoAsync(foto);
        //                }
        //                else
        //                {
        //                    allSentSuccessfully = false;
        //                    Console.WriteLine($"Error al enviar la foto: {response.ReasonPhrase}");
        //                }
        //            }
        //            catch (HttpRequestException ex)
        //            {
        //                allSentSuccessfully = false;
        //                Console.WriteLine($"Error de red al enviar la foto: {ex.Message}");
        //            }
        //            catch (Exception ex)
        //            {
        //                allSentSuccessfully = false;
        //                Console.WriteLine($"Excepción al enviar la foto: {ex.Message}");
        //            }
        //        }

        //        if (allSentSuccessfully)
        //        {
        //            //  await DisplayAlert("Éxito", "Todas las fotos fueron enviadas correctamente.", "OK");
        //            Console.WriteLine("Fotos enviadas");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Error");
        //            //await DisplayAlert("Aviso", "Algunas fotos no pudieron ser enviadas.", "OK");

        //        }
        //    }
        //    else
        //    {
        //        // Opcional: mensaje de no hay fotos por enviar.
        //        // await DisplayAlert("Aviso", "No hay Fotos por enviar", "OK");
        //    }


        //}

   
        /// Transmite los datos de una foto al servidor.
        public static async Task<bool> TransmitirFotoAsync(Foto foto)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    { "imagen", foto.Base64String },
                    { "nombre", foto.Nombre }
                };
                var content = new FormUrlEncodedContent(data);

                var url = "http://179.61.13.234:8089/ws_huertosapp/recibir_fotos.php"; // URL exclusiva para fotos
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta del servidor para foto: {responseData}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error al transmitir la foto: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción durante la transmisión de la foto: {ex.Message}");
                return false;
            }
        }
    }
}
