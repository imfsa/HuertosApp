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
    public partial class DescargaDatosComercialPage : ContentPage
    {
        public DescargaDatosComercialPage()
        {
            InitializeComponent();
            fechaDesde.Date = DateTime.Now;
            fechaHasta.Date = DateTime.Now;
        }

        // Quitar Filtros
        private void OnQuitarFiltrosClicked(object sender, EventArgs e)
        {
            fechaDesde.Date = DateTime.Now;
            fechaHasta.Date = DateTime.Now;
            lblTotalRegistros.Text = "Total de registros: 0";
        }

        // Consultar Datos
        private async void OnConsultarClicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "No hay conexión a Internet.", "OK");
                return;
            }

            DateTime desde = fechaDesde.Date;
            DateTime hasta = fechaHasta.Date;

            string url = $"https://api.imf.cl:8443/huertosapp/datos_cosechacomercial.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

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

        // Exportar Datos a Excel
        private async void OnExportarExcelClicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "No hay conexión a Internet.", "OK");
                return;
            }

            DateTime desde = fechaDesde.Date;
            DateTime hasta = fechaHasta.Date;

            string url = $"https://api.imf.cl:8443/huertosapp/datos_cosechacomercial.php?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

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
                                var worksheet = workbook.Worksheets.Add("CosechaComercial");

                                // Encabezados
                                worksheet.Cell(1, 1).Value = "ID";
                                worksheet.Cell(1, 2).Value = "Fecha";
                                worksheet.Cell(1, 3).Value = "Genotipo";
                                worksheet.Cell(1, 4).Value = "Temporada";
                                worksheet.Cell(1, 5).Value = "Predio";
                                worksheet.Cell(1, 6).Value = "Rodal";
                                worksheet.Cell(1, 7).Value = "Kilos";
                                worksheet.Cell(1, 8).Value = "Cosechador";
                                worksheet.Cell(1, 9).Value = "Modalidad";
                                worksheet.Cell(1, 10).Value = "N° Árboles/Día";
                                worksheet.Cell(1, 11).Value = "Total Árboles";
                                worksheet.Cell(1, 12).Value = "Usuario ID";
                                worksheet.Cell(1, 13).Value = "Nombre Usuario";

                                int row = 2;
                                foreach (var registro in data.data)
                                {
                                    worksheet.Cell(row, 1).Value = registro.ID;
                                    worksheet.Cell(row, 2).Value = registro.Fecha;
                                    worksheet.Cell(row, 3).Value = registro.Genotipo;
                                    worksheet.Cell(row, 4).Value = registro.Temporada;
                                    worksheet.Cell(row, 5).Value = registro.Predio;
                                    worksheet.Cell(row, 6).Value = registro.Rodal;
                                    worksheet.Cell(row, 7).Value = registro.Kilos;
                                    worksheet.Cell(row, 8).Value = registro.Cosechador;
                                    worksheet.Cell(row, 9).Value = registro.Modalidad;
                                    worksheet.Cell(row, 10).Value = registro.ArbolesDia;
                                    worksheet.Cell(row, 11).Value = registro.TotalArboles;
                                    worksheet.Cell(row, 12).Value = registro.UsuarioId;
                                    worksheet.Cell(row, 13).Value = registro.NombreUsuario;
                                    row++;
                                }

                                // **Ruta para guardar el archivo**
#if ANDROID
                                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                                var filePath = Path.Combine(downloadsPath, "DatosCosechaComercial.xlsx");
#else
                                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DatosCosechaComercial.xlsx");
#endif

                                workbook.SaveAs(filePath);

                                // **Abrir archivo**
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
            public List<RegistroComercial> data { get; set; }
        }
    }
}
