using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;
using HuertosApp.Models;

namespace HuertosApp.Pages
{
    public partial class DescargaDatosPage : ContentPage
    {
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

            string url = $"http://179.61.13.234:8089/ws_huertosapp/datos_fertirriego.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        var data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(jsonData);

                        if (data != null && data.success)
                        {
                            lblTotalRegistros.Text = $"Total de registros: {data.data.Count}";
                        }
                        else
                        {
                            await DisplayAlert("Error", "No se pudo obtener los datos.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error en la solicitud HTTP.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error en la solicitud HTTP: {ex.Message}", "OK");
                }
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

            string url = $"http://179.61.13.234:8089/ws_huertosapp/datos_fertirriego.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        var data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(jsonData);

                        if (data != null && data.success)
                        {
                            try
                            {
                                var workbook = new XLWorkbook();
                                var worksheet = workbook.Worksheets.Add("Fertirriego");

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
                                worksheet.Cell(1, 19).Value = "Usuario ID";    // Nueva columna para ID
                                worksheet.Cell(1, 20).Value = "Nombre Usuario"; // Nueva columna para el nombre del usuario

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

                                    if (!string.IsNullOrWhiteSpace(registro.NombreFoto))
                                    {
                                        var cell = worksheet.Cell(row, 18);
                                        cell.Value = "Ver Foto";
                                        cell.SetHyperlink(new XLHyperlink(registro.NombreFoto)); // Solo si hay una foto válida
                                    }
                                    else
                                    {
                                        worksheet.Cell(row, 18).Value = ""; // Dejar vacío si no hay foto
                                    }

                                    worksheet.Cell(row, 19).Value = registro.UsuarioId;         // Mostrar ID
                                    worksheet.Cell(row, 20).Value = registro.NombreUsuario;    // Mostrar Nombre del Usuario

                                    row++;
                                }

#if ANDROID
                                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                                var filePath = Path.Combine(downloadsPath, "DatosFertirriego.xlsx");
#else
                                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DatosFertirriego.xlsx");
#endif

                                workbook.SaveAs(filePath);

                                await Launcher.OpenAsync(new OpenFileRequest
                                {
                                    File = new ReadOnlyFile(filePath)
                                });
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Error", $"Error al generar el archivo Excel: {ex.Message}", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "No se pudo obtener los datos.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error en la solicitud HTTP.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error en la solicitud HTTP: {ex.Message}", "OK");
                }
            }
        }

        public class RootObject
        {
            public bool success { get; set; }
            public List<Fertirriego> data { get; set; }
        }
    }
}
