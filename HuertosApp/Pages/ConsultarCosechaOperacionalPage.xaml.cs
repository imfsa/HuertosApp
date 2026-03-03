using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuertosApp.Pages
{
    public partial class ConsultarCosechaOperacionalPage : ContentPage
    {
        private List<RegistroCosecha> _registrosActuales = new();

        public ConsultarCosechaOperacionalPage()
        {
            InitializeComponent();

            // Fecha por defecto: hoy
            FechaPicker.Date = DateTime.Today;
        }

        /// <summary>
        /// Al entrar a la página, intenta cargar de inmediato la fecha de hoy.
        /// Si algo falla, muestra mensaje y NO se cae.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await CargarRegistrosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error al iniciar",
                    $"Ocurrió un problema al cargar los registros.\n\nDetalle: {ex.Message}",
                    "OK");
            }
        }

        private async void BtnBuscar_Clicked(object sender, EventArgs e)
        {
            try
            {
                await CargarRegistrosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error al buscar",
                    $"No se pudieron cargar los registros.\n\nDetalle: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Carga registros desde la base para la fecha seleccionada
        /// y actualiza totales + lista de tarjetas.
        /// </summary>
        private async Task CargarRegistrosAsync()
        {
            // 1) Obtener fecha en formato igual al guardado: yyyy-MM-dd
            string fecha = FechaPicker.Date.ToString("yyyy-MM-dd");

            // 2) Consultar base de datos (método ya blindado en Database)
            var lista = await Database
                .GetRegistrosCosechaByFechaAsync(fecha)
                .ConfigureAwait(false);

            if (lista == null)
                lista = new List<RegistroCosecha>();

            // 3) Ordenar para que se vea ordenado
            lista = lista
                .OrderBy(r => r.TreeId)
                .ThenBy(r => r.Id)
                .ToList();

            _registrosActuales = lista;

            // 4) Calcular totales
            int totalArboles = _registrosActuales.Count;
            decimal totalKilos = 0m;

            foreach (var r in _registrosActuales)
            {
                try
                {
                    totalKilos += r.Kilos;
                }
                catch
                {
                    // Si algún registro tiene kilos corruptos, lo ignoramos.
                }
            }

            // 5) Actualizar UI en el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CosechaCollectionView.ItemsSource = _registrosActuales;

                LblTotalArboles.Text = totalArboles.ToString();
                LblTotalKilos.Text = totalKilos.ToString("F1");

                bool hayRegistros = totalArboles > 0;

                // Si no hay registros → mostrar mensaje y ocultar lista
                LblSinRegistros.IsVisible = !hayRegistros;
                CosechaCollectionView.IsVisible = hayRegistros;
            });
        }
    }
}
