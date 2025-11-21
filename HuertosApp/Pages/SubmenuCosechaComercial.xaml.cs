using HuertosApp.Services;
using HuertosApp.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HuertosApp.Pages
{
    public partial class SubmenuCosechaComercial : ContentPage
    {
        public SubmenuCosechaComercial()
        {
            InitializeComponent();
        }

        // Navegar a la página de nuevo registro comercial
        private async void OnIngresarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NuevoRegistroComercialPage());
        }

        // Navegar a la página de consulta de registros comerciales
        private async void OnConsultarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarRegistroComercialPage());
        }

        // Transmitir datos comerciales
        private async void OnTransmitirDatosClicked(object sender, EventArgs e)
        {
            await TransmitirDatosAsync();
        }

        /// <summary>
        /// Método para transmitir los datos comerciales al servidor.
        /// </summary>
        private async Task TransmitirDatosAsync()
        {
            Console.WriteLine("Iniciando transmisión de datos...");

            // Obtener instancia de la base de datos
            var database = Database.GetDatabase();

            try
            {
                // Obtener todos los registros comerciales no enviados
                var registros = await database.Table<RegistroComercial>()
                                              .Where(r => !r.Enviado) // Filtrar registros no enviados
                                              .ToListAsync();

                // Verificar si hay registros para transmitir
                if (registros.Count == 0)
                {
                    await DisplayAlert("Información", "No hay registros comerciales para transmitir.", "OK");
                    return;
                }

                // Contadores de éxito y fallas
                int registrosEnviados = 0;
                int registrosFallidos = 0;

                // Transmitir registros comerciales
                foreach (var registro in registros)
                {
                    Console.WriteLine($"Transmitiendo registro ID: {registro.ID}");

                    // Enviar el registro al servidor
                    bool success = await DataTransmissionServiceRegistroComercial.TransmitirDatosAsync(registro);

                    if (success)
                    {
                        // Marcar como enviado solo si fue exitoso
                        registro.Enviado = true; // Actualizar estado
                        await database.UpdateRegistroComercialAsync(registro); // Actualizar en la base de datos
                        registrosEnviados++;

                        Console.WriteLine($"Registro ID {registro.ID} enviado correctamente.");
                    }
                    else
                    {
                        registrosFallidos++;
                        Console.WriteLine($"Error al enviar registro ID {registro.ID}.");
                    }
                }

                // Mostrar resultados de la transmisión
                await DisplayAlert("Resultados de Transmisión",
                    $"Registros enviados: {registrosEnviados}\n" +
                    $"Registros fallidos: {registrosFallidos}", "OK");
            }
            catch (Exception ex)
            {
                // Manejar errores generales
                await DisplayAlert("Error", $"Ocurrió un error durante la transmisión: {ex.Message}", "OK");
                Console.WriteLine($"Error durante la transmisión: {ex.Message}");
            }
        }


        /// <summary>
        /// Limpiar registros antiguos (más de 3 días).
        /// </summary>
        private async void OnLimpiarDatosClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Deseas limpiar los datos antiguos?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    await Database.ClearOldRegistrosAsync();
                    await DisplayAlert("Éxito", "Los datos antiguos han sido limpiados.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al limpiar los datos: {ex.Message}", "OK");
                }
            }
        }

        /// <summary>
        /// Descargar datos comerciales (opcional, no implementado).
        /// </summary>
        private async void OnDescargarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DescargaDatosComercialPage()); 
        }

        /// <summary>
        /// Salir al Login.
        /// </summary>
        private async void OnSalirClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Deseas salir?", "Sí", "No");
            if (confirm)
            {
                await Navigation.PushAsync(new LoginPage());
            }
        }
    }
}
