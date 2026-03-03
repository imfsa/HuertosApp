using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net.Http;
using ClosedXML.Excel;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;
using HuertosApp.Models;
using System.Text.Json;
using ClosedXML.Excel.Drawings;

namespace HuertosApp.Pages
{
    public partial class DescargaDatosPage : ContentPage
    {
        // ✅ URL pública base donde el servidor expone las fotos (por archivo)
        private const string BASE_FOTOS_URL = "https://api.imf.cl:8443/huertosapp/fotos_huertos/";

        // ✅ Opciones JSON: IMPORTANTÍSIMO para evitar null por diferencias de nombre/case
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // ✅ clave
        };

        public DescargaDatosPage()
        {
            InitializeComponent();
            fechaDesde.Date = DateTime.Now;
            fechaHasta.Date = DateTime.Now;
        }

        private void OnQuitarFiltrosClicked(object sender, EventArgs e)
        {
            fechaDesde.Date = DateTime.Now;
            fechaHasta.Date = DateTime.Now;
            lblTotalRegistros.Text = "Total de registros: 0";
        }

        private async void OnConsultarClicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "No hay conexión a Internet.", "OK");
                return;
            }

            DateTime desde = fechaDesde.Date;
            DateTime hasta = fechaHasta.Date;

            string url = $"https://api.imf.cl:8443/huertosapp/datos_fertirriego.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

            using var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Error", "Error en la solicitud HTTP.", "OK");
                    return;
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<RootObject>(jsonData, JsonOptions);

                if (data != null && data.success && data.data != null)
                {
                    lblTotalRegistros.Text = $"Total de registros: {data.data.Count}";

                    // ✅ Diagnóstico: cuántos vienen con foto desde el API
                    int conFoto = 0;
                    foreach (var r in data.data)
                        if (!string.IsNullOrWhiteSpace(r.NombreFoto)) conFoto++;

                    await DisplayAlert("Info", $"Registros con NombreFoto (desde API): {conFoto}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener los datos.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error en la solicitud HTTP: {ex.Message}", "OK");
            }
        }

        private async void OnExportarExcelClicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "No hay conexión a Internet.", "OK");
                return;
            }

            DateTime desde = fechaDesde.Date;
            DateTime hasta = fechaHasta.Date;

            string url = $"https://api.imf.cl:8443/huertosapp/datos_fertirriego.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

            using var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Error", "Error en la solicitud HTTP.", "OK");
                    return;
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<RootObject>(jsonData, JsonOptions);

                if (data == null || !data.success || data.data == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener los datos.", "OK");
                    return;
                }

                // ✅ Diagnóstico antes de exportar (para confirmar que no vienen nulos)
                int conFoto = 0;
                foreach (var r in data.data)
                    if (!string.IsNullOrWhiteSpace(r.NombreFoto)) conFoto++;

                // Si esto sale 0, el problema NO es Excel: es el JSON / PHP datos_fertirriego.php
                // (lo dejo como alerta porque te ahorra horas)
                await DisplayAlert("Info", $"Exportación: registros con NombreFoto (desde API): {conFoto}", "OK");

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Fertirriego");

                // Encabezados
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Huerto";
                worksheet.Cell(1, 3).Value = "Etapa/Fertilización (mes)";
                worksheet.Cell(1, 4).Value = "Año";
                worksheet.Cell(1, 5).Value = "Fertilizantes";
                worksheet.Cell(1, 6).Value = "Tipo Riego (Trad o Tecn)";
                worksheet.Cell(1, 7).Value = "Sector";
                worksheet.Cell(1, 8).Value = "Fechas de Riego";
                worksheet.Cell(1, 9).Value = "Tiempo Riego (Hrs)";
                worksheet.Cell(1, 10).Value = "M³ Sistema";
                worksheet.Cell(1, 11).Value = "mm Sistema";
                worksheet.Cell(1, 12).Value = "EVP Prom (mm)";
                worksheet.Cell(1, 13).Value = "Fertilizante Est. 1 (cc)";
                worksheet.Cell(1, 14).Value = "Fertilizante Est. 2 (cc)";
                worksheet.Cell(1, 15).Value = "Fertilizante Est. 3 (cc)";
                worksheet.Cell(1, 16).Value = "Fertilizante (Kg)";
                worksheet.Cell(1, 17).Value = "Observación";
                worksheet.Cell(1, 18).Value = "Imagen";
                worksheet.Cell(1, 19).Value = "Usuario ID";
                worksheet.Cell(1, 20).Value = "Nombre Usuario";
                worksheet.Cell(1, 21).Value = "URL Foto"; // ✅ visible

                worksheet.Range(1, 1, 1, 21).Style.Font.Bold = true;
                worksheet.SheetView.FreezeRows(1);

                int row = 2;
                foreach (var registro in data.data)
                {
                    worksheet.Cell(row, 1).Value = registro.ID;
                    worksheet.Cell(row, 2).Value = registro.Huerto;
                    worksheet.Cell(row, 3).Value = registro.EtapaFertilizacion;
                    worksheet.Cell(row, 4).Value = registro.Anio;
                    worksheet.Cell(row, 5).Value = registro.Fertilizantes;
                    worksheet.Cell(row, 6).Value = registro.TipoRiego;
                    worksheet.Cell(row, 7).Value = registro.Sector;

                    worksheet.Cell(row, 8).Value = !string.IsNullOrWhiteSpace(registro.FechaRiego)
                        ? registro.FechaRiego
                        : "Sin fecha";

                    worksheet.Cell(row, 9).Value = registro.TiempoRiego;
                    worksheet.Cell(row, 10).Value = registro.M3Sistema;
                    worksheet.Cell(row, 11).Value = registro.MmSistema;
                    worksheet.Cell(row, 12).Value = registro.EVP_Prom;
                    worksheet.Cell(row, 13).Value = registro.FertilizanteEst1;
                    worksheet.Cell(row, 14).Value = registro.FertilizanteEst2;
                    worksheet.Cell(row, 15).Value = registro.FertilizanteEst3;
                    worksheet.Cell(row, 16).Value = registro.FertilizanteKg;
                    worksheet.Cell(row, 17).Value = registro.Observacion;

                    var cellFoto = worksheet.Cell(row, 18);
                    var cellUrl = worksheet.Cell(row, 21);

                    string urlFoto = ConstruirUrlFotoPublica(registro.NombreFoto);

                    if (!string.IsNullOrWhiteSpace(urlFoto))
                    {
                        cellFoto.Value = "Ver Foto";
                        cellFoto.SetHyperlink(new XLHyperlink(urlFoto));
                        cellFoto.Style.Font.Underline = XLFontUnderlineValues.Single;
                        cellFoto.Style.Font.FontColor = XLColor.Blue;

                        cellUrl.Value = urlFoto; // ✅ visible
                    }
                    else
                    {
                        cellFoto.Value = "";
                        cellUrl.Value = "";
                    }

                    worksheet.Cell(row, 19).Value = registro.UsuarioId;
                    worksheet.Cell(row, 20).Value = registro.NombreUsuario;

                    row++;
                }

                worksheet.Columns(1, 21).AdjustToContents();

#if ANDROID
                var downloadsPath = Android.OS.Environment
                    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
                    .AbsolutePath;

                var filePath = Path.Combine(downloadsPath, $"DatosFertirriego_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx");
#else
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"DatosFertirriego_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx");
#endif

                workbook.SaveAs(filePath);

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error en la solicitud HTTP: {ex.Message}", "OK");
            }
        }

        private static string ConstruirUrlFotoPublica(string nombreFoto)
        {
            if (string.IsNullOrWhiteSpace(nombreFoto))
                return null;

            var raw = nombreFoto.Trim().Replace("\\", "/");

            // Si ya viene URL completa, úsala
            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return raw;

            // último segmento (nombre del archivo)
            var soloNombre = raw.Contains("/")
                ? raw.Substring(raw.LastIndexOf("/") + 1)
                : raw;

            soloNombre = soloNombre.Trim();
            if (string.IsNullOrWhiteSpace(soloNombre))
                return null;

            return BASE_FOTOS_URL + Uri.EscapeDataString(soloNombre);
        }

        public class RootObject
        {
            public bool success { get; set; }
            public List<Fertirriego> data { get; set; }
        }
    }
}