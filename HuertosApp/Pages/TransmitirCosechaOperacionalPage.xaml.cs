using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.ApplicationModel;

namespace HuertosApp.Pages
{
    public partial class TransmitirCosechaOperacionalPage : ContentPage
    {
        private List<RegistroCosecha> _pendientes = new();

        public TransmitirCosechaOperacionalPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await CargarPendientesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"No se pudieron cargar los registros pendientes.\n\nDetalle: {ex.Message}",
                    "OK");
            }
        }

        private async Task CargarPendientesAsync()
        {
            // Por seguridad, si algo falla en la DB, devolvemos lista vacía.
            var lista = await Database.GetRegistrosCosechaPendientesSincronizarAsync()
                                      .ConfigureAwait(false);

            _pendientes = lista ?? new List<RegistroCosecha>();

            int total = _pendientes.Count;
            decimal totalKilos = 0m;

            foreach (var r in _pendientes)
            {
                try
                {
                    totalKilos += r.Kilos;
                }
                catch
                {
                    // Si un registro tiene kilos malos, lo ignoramos en el total.
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ListaPendientes.ItemsSource = _pendientes;

                if (total == 0)
                {
                    LblResumen.Text = "No hay registros pendientes de transmisión.";
                    ListaPendientes.IsVisible = false;
                    LblSinPendientes.IsVisible = true;
                }
                else
                {
                    LblResumen.Text = $"Pendientes: {total} registros • Total kilos: {totalKilos:N1}";
                    ListaPendientes.IsVisible = true;
                    LblSinPendientes.IsVisible = false;
                }
            });
        }

        private async void BtnTransmitir_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (_pendientes.Count == 0)
                {
                    await DisplayAlert(
                        "Sin datos",
                        "No hay registros pendientes de transmisión.",
                        "OK");
                    return;
                }

                bool confirmar = await DisplayAlert(
                    "Confirmar transmisión",
                    $"Se enviarán {_pendientes.Count} registros de cosecha al servidor.\n\n¿Deseas continuar?",
                    "Sí, transmitir",
                    "Cancelar");

                if (!confirmar)
                    return;

                BtnTransmitir.IsEnabled = false;
                Activity.IsVisible = true;
                Activity.IsRunning = true;

                int ok = 0;
                int fail = 0;

                // Transmisión en lote
                var (exito, enviados, error) =
                    await DataTransmissionServiceCosechaOperacional.TransmitirLoteAsync(_pendientes);

                if (exito)
                {
                    ok = enviados;

                    // Marcar como sincronizados los que se enviaron
                    foreach (var registro in _pendientes)
                    {
                        registro.Sincronizado = true;
                        await Database.UpdateRegistroCosechaAsync(registro);
                    }
                }
                else
                {
                    ok = enviados;
                    fail = Math.Max(0, _pendientes.Count - enviados);
                }

                Activity.IsRunning = false;
                Activity.IsVisible = false;
                BtnTransmitir.IsEnabled = true;

                string mensajeResultado =
                    $"Transmitidos correctamente: {ok}\nFallidos: {fail}";

                if (!string.IsNullOrWhiteSpace(error))
                    mensajeResultado += $"\n\nDetalle: {error}";

                await DisplayAlert(
                    "Transmisión finalizada",
                    mensajeResultado,
                    "OK");

                await CargarPendientesAsync();
            }
            catch (Exception ex)
            {
                Activity.IsRunning = false;
                Activity.IsVisible = false;
                BtnTransmitir.IsEnabled = true;

                await DisplayAlert(
                    "Error",
                    $"Ocurrió un problema durante la transmisión.\n\nDetalle: {ex.Message}",
                    "OK");
            }
        }
    }
}
