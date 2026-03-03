using HuertosApp.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HuertosApp.Pages
{
    public partial class ConsultarDespachosPage : ContentPage
    {
        private List<DespachoResumen> _despachos = new();

        private const string BaseUrl =
            "https://api.imf.cl:8443/huertosappV2/listar_despachos.php";

        private const string ExcelBaseUrl =
            "https://api.imf.cl:8443/huertosappV2/despachos_excel/";

        public ConsultarDespachosPage()
        {
            InitializeComponent();

            FechaDesde.Date = DateTime.Today.AddDays(-7);
            FechaHasta.Date = DateTime.Today;
        }

        private async void BtnBuscar_Clicked(object sender, EventArgs e)
        {
            await CargarDespachosAsync();
        }

        private async Task CargarDespachosAsync()
        {
            try
            {
                string desde = FechaDesde.Date.ToString("yyyy-MM-dd");
                string hasta = FechaHasta.Date.ToString("yyyy-MM-dd");
                string folio = FolioEntry.Text?.Trim() ?? string.Empty;

                string url = $"{BaseUrl}?desde={desde}&hasta={hasta}";
                if (!string.IsNullOrEmpty(folio))
                    url += $"&folio={Uri.EscapeDataString(folio)}";

                using var client = new HttpClient();
                var json = await client.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var resp = JsonSerializer.Deserialize<ListarDespachosResponse>(json, options);

                if (resp == null || !resp.success)
                {
                    await DisplayAlert("Error",
                        resp?.message ?? "No se pudieron obtener los despachos.",
                        "OK");
                    return;
                }

                _despachos = resp.data ?? new List<DespachoResumen>();

                int totalDespachos = _despachos.Count;

                // total_arboles sigue siendo int, así que sin problema
                int totalArboles = _despachos.Sum(d => d.total_arboles);

                // Para kilos, parseamos cada string con TryParse
                double totalKilos = 0;

                foreach (var d in _despachos)
                {
                    if (double.TryParse(
                            d.total_kilos,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var kilos))
                    {
                        totalKilos += kilos;
                    }
                    // Si viene vacío o raro, simplemente no suma (lo tratamos como 0)
                }


                LblTotalDespachos.Text = totalDespachos.ToString();
                LblTotalArboles.Text = totalArboles.ToString();
                LblTotalKilos.Text = totalKilos.ToString("N1");

                bool hay = _despachos.Any();
                DespachosCollection.ItemsSource = _despachos;
                DespachosCollection.IsVisible = hay;
                LblSinResultados.IsVisible = !hay;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    "No se pudieron cargar los despachos.\n\nDetalle: " + ex.Message,
                    "OK");
            }
        }


        private async void BtnExcel_Clicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.BindingContext is not DespachoResumen d)
                return;

            if (string.IsNullOrWhiteSpace(d.documento))
            {
                await DisplayAlert("Sin archivo",
                    "Este despacho no tiene archivo Excel asociado.",
                    "OK");
                return;
            }

            string url = ExcelBaseUrl + d.documento;

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(
                        "Error",
                        $"No se pudo descargar el archivo.\nCódigo HTTP: {(int)response.StatusCode}",
                        "OK");
                    return;
                }

                // 1) Descargamos los bytes del Excel
                var bytes = await response.Content.ReadAsByteArrayAsync();

                // 2) Carpeta de Descargas según plataforma
                string fileName = d.documento;

#if ANDROID
        string path = Android.OS.Environment
            .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
            .AbsolutePath;
#else
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fullPath = Path.Combine(path, fileName);

                // 3) Guardar archivo en el dispositivo
                File.WriteAllBytes(fullPath, bytes);

                // 4) Abrir el archivo con la app que elija el usuario (Excel, WPS, etc.)
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(fullPath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    "No se pudo descargar el archivo.\n\nDetalle: " + ex.Message,
                    "OK");
            }
        }
    }
}
