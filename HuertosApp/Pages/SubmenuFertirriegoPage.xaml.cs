using HuertosApp.Services;
using System;
using System.Threading.Tasks;

namespace HuertosApp.Pages
{
    public partial class SubmenuFertirriegoPage : ContentPage
    {
        public SubmenuFertirriegoPage()
        {
            InitializeComponent();
        }

        // Navegar a NuevoRegistroFertirriegoPage
        private async void OnIngresarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NuevoRegistroFertirriegoPage());
        }

        // Navegar a ConsultarFertirriegoPage
        private async void OnConsultarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarFertirriegoPage());
        }

        // Transmitir datos
        private async void OnTransmitirDatosClicked(object sender, EventArgs e)
        {
            await TransmitirDatosAsync();
        }


        // Transmitir Fotos
        //private async void OnTransmitirFotosClicked(object sender, EventArgs e)
        //{
        //     DataTransmissionServiceFertirriego.SendPhoto();
        //}

        private async Task TransmitirDatosAsync()
        {
            Console.WriteLine("Iniciando transmisión de datos...");
            var database = Database.GetDatabase();
            var registros = await database.GetAllFertirriegoAsync();
            var fotosNoEnviadas = await database.GetFotosNoEnviadasAsync();

            if (registros.Count == 0 && fotosNoEnviadas.Count == 0)
            {
                await DisplayAlert("Información", "No hay registros o fotos para transmitir.", "OK");
                return;
            }

            int registrosEnviados = 0;
            int registrosFallidos = 0;
            int fotosEnviadas = 0;
            int fotosFallidas = 0;

            // Transmitir registros
            foreach (var registro in registros)
            {
                if (!registro.Enviado)
                {
                    bool success = await DataTransmissionServiceFertirriego.TransmitirDatosAsync(registro);
                    if (success)
                    {
                        registro.Enviado = true;
                        await database.UpdateFertirriegoAsync(registro);
                        registrosEnviados++;
                    }
                    else
                    {
                        registrosFallidos++;
                    }
                }
            }

            // Transmitir fotos
            foreach (var foto in fotosNoEnviadas)
            {
                bool success = await DataTransmissionServiceFertirriego.TransmitirFotoAsync(foto);
                if (success)
                {
                    foto.Enviado = true;
                    await database.UpdatePhotoAsync(foto);
                    fotosEnviadas++;
                }
                else
                {
                    fotosFallidas++;
                }
            }

            // Mostrar resultados de transmisión de fotos
            await DisplayAlert("Resultados de Transmisión",
                $"Registros enviados: {registrosEnviados}\nRegistros fallidos: {registrosFallidos}\n" +
                $"Fotos enviadas: {fotosEnviadas}\nFotos fallidas: {fotosFallidas}", "OK");
        }

        /// Boton transmitir Fotos
        private async Task TransmitirFotosAsync()
        {
            Console.WriteLine("Iniciando transmisión de datos...");
            var database = Database.GetDatabase();
            var registros = await database.GetAllFertirriegoAsync();
            var fotosNoEnviadas = await database.GetFotosNoEnviadasAsync();

            if (registros.Count == 0 && fotosNoEnviadas.Count == 0)
            {
                await DisplayAlert("Información", "No hay registros o fotos para transmitir.", "OK");
                return;
            }

            int registrosEnviados = 0;
            int registrosFallidos = 0;
            int fotosEnviadas = 0;
            int fotosFallidas = 0;

            // Transmitir registros
            //foreach (var registro in registros)
            //{
            //    if (!registro.Enviado)
            //    {
            //        bool success = await DataTransmissionServiceFertirriego.TransmitirDatosAsync(registro);
            //        if (success)
            //        {
            //            registro.Enviado = true;
            //            await database.UpdateFertirriegoAsync(registro);
            //            registrosEnviados++;
            //        }
            //        else
            //        {
            //            registrosFallidos++;
            //        }
            //    }
            //}

            // Transmitir fotos
            foreach (var foto in fotosNoEnviadas)
            {
                bool success = await DataTransmissionServiceFertirriego.TransmitirFotoAsync(foto);
                if (success)
                {
                    foto.Enviado = true;
                    await database.UpdatePhotoAsync(foto);
                    fotosEnviadas++;
                }
                else
                {
                    fotosFallidas++;
                }
            }

            // Mostrar resultados de transmisión de fotos
            await DisplayAlert("Resultados de Transmisión",
                $"Registros enviados: {registrosEnviados}\nRegistros fallidos: {registrosFallidos}\n" +
                $"Fotos enviadas: {fotosEnviadas}\nFotos fallidas: {fotosFallidas}", "OK");
        }











        // Limpiar registros antiguos
        private async void OnLimpiarDatosClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Estás seguro de que deseas limpiar los datos de hace más de tres días?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    await Database.ClearOldRegistrosAsync();
                    await DisplayAlert("Éxito", "Los datos han sido limpiados.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al limpiar los datos: {ex.Message}", "OK");
                }
            }
        }

        // Descargar datos (opcional, no implementado)
        private async void OnDescargarDatosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DescargaDatosPage());
        }

        // Salir al Login
        private async void OnSalirClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Estás seguro de que deseas salir?", "Sí", "No");
            if (confirm)
            {
                await Navigation.PushAsync(new LoginPage());
            }
        }
    }
}
