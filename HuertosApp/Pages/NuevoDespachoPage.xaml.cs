using ClosedXML.Excel;
using HuertosApp.Models;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

namespace HuertosApp.Pages
{
    public partial class NuevoDespachoPage : ContentPage
    {
        // Lista de cosechas pendientes que vienen de la API
        private List<CosechaPendiente> _pendientes = new();

        // Totales de selección
        private int _totalSeleccionados = 0;
        private double _totalKilosSeleccionados = 0;

        public NuevoDespachoPage()
        {
            InitializeComponent();

            // Fechas por defecto: hoy
            FechaDesdePicker.Date = DateTime.Today;
            FechaHastaPicker.Date = DateTime.Today;
        }

        // ======================= CONSULTA A API =======================

        private async void BtnConsultar_Clicked(object sender, EventArgs e)
        {
            try
            {
                await CargarPendientesDesdeApiAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"No se pudieron cargar las cosechas pendientes.\n\nDetalle: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Llama a datos_cosecha_pendiente.php con rango de fechas
        /// y carga la lista de cosechas NO despachadas.
        /// </summary>
        private async Task CargarPendientesDesdeApiAsync()
        {
            DateTime desde = FechaDesdePicker.Date.Date;
            DateTime hasta = FechaHastaPicker.Date.Date;

            // Normalizar rango por si el usuario invierte las fechas
            if (hasta < desde)
                (desde, hasta) = (hasta, desde);

            string desdeStr = desde.ToString("yyyy-MM-dd");
            string hastaStr = hasta.ToString("yyyy-MM-dd");

            string url =
                $"https://api.imf.cl:8443/huertosappV2/datos_cosecha_pendiente.php?desde={desdeStr}&hasta={hastaStr}";

            using var client = new HttpClient();

            var json = await client.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<PendientesResponse>(json, options);

            if (result != null && result.success && result.data != null)
            {
                _pendientes = result.data;

                // Todas comienzan sin seleccionar
                foreach (var c in _pendientes)
                    c.IsSelected = false;
            }
            else
            {
                _pendientes = new List<CosechaPendiente>();

                await DisplayAlert(
                    "Información",
                    "No se encontraron cosechas pendientes para el rango indicado.",
                    "OK");
            }

            // Actualizar UI
            listaPendientes.ItemsSource = _pendientes;

            bool hayDatos = _pendientes.Count > 0;
            listaPendientes.IsVisible = hayDatos;
            LblSinResultados.IsVisible = !hayDatos;

            RecalcularTotales();
        }

        // ======================= SUBE AL SERVIDOR =======================

        private async Task SubirExcelServidorAsync(int despachoId, string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                    return;

                using var client = new HttpClient();
                using var form = new MultipartFormDataContent();

                // despacho_id
                form.Add(new StringContent(despachoId.ToString()), "despacho_id");

                // archivo
                var fileName = Path.GetFileName(fullPath);
                var fileStream = File.OpenRead(fullPath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                form.Add(fileContent, "archivo", fileName);

                var response = await client.PostAsync(
                    "https://api.imf.cl:8443/huertosappV2/subir_excel_despacho.php",
                    form);

                var resp = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Respuesta subir_excel_despacho.php: " + resp);
            }
            catch (Exception ex)
            {
                // No botar la app: solo log.
                Debug.WriteLine("Error subiendo Excel: " + ex.Message);
            }
        }

        // ======================= SELECCIÓN Y TOTALES =======================

        private void OnCheckChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is not CheckBox check)
                return;

            if (check.BindingContext is not CosechaPendiente cosecha)
                return;

            cosecha.IsSelected = e.Value;
            RecalcularTotales();
        }

        /// <summary>
        /// Calcula totales de pendientes y selección
        /// y actualiza tarjetas + resumen.
        /// </summary>
        private void RecalcularTotales()
        {
            int pendientesTotal = _pendientes.Count;

            var seleccionados = _pendientes
                .Where(x => x.IsSelected)
                .ToList();

            _totalSeleccionados = seleccionados.Count;

            _totalKilosSeleccionados = seleccionados.Sum(x =>
            {
                if (double.TryParse(x.kilos, out double k))
                    return k;
                return 0;
            });

            LblTotalPendientes.Text = pendientesTotal.ToString();
            LblTotalKilosSeleccionados.Text = _totalKilosSeleccionados.ToString("N1");

            lblResumen.Text =
                $"Pendientes: {pendientesTotal} • Seleccionados: {_totalSeleccionados} • Kilos seleccionados: {_totalKilosSeleccionados:N1}";
        }

        // ======================= DESPACHO + EXCEL =======================

        private async void GenerarDespacho_Clicked(object sender, EventArgs e)
        {
            try
            {
                var seleccionados = _pendientes
                    .Where(x => x.IsSelected)
                    .ToList();

                if (seleccionados.Count == 0)
                {
                    await DisplayAlert(
                        "Selección requerida",
                        "Debes seleccionar al menos una cosecha para generar el despacho.",
                        "OK");
                    return;
                }

                bool confirmar = await DisplayAlert(
                    "Confirmar despacho",
                    $"Se generará un despacho con {seleccionados.Count} cosechas seleccionadas " +
                    $"({_totalKilosSeleccionados:N1} kg aprox.).\n\n¿Deseas continuar?",
                    "Sí, generar",
                    "Cancelar");

                if (!confirmar)
                    return;

                BtnGenerar.IsEnabled = false;
                BtnConsultar.IsEnabled = false;
                Activity.IsVisible = true;
                Activity.IsRunning = true;

                // Construir payload para crear_despacho.php
                var payload = new
                {
                    // Dejamos que el servidor genere el folio; solo mandamos fecha y cosechas.
                    fecha_desp = DateTime.Now.ToString("yyyy-MM-dd"),
                    documento = "", // se puede usar luego cuando exista upload del archivo
                    cosechas = seleccionados.Select(x => x.rc_id).ToList()
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    "https://api.imf.cl:8443/huertosappV2/crear_despacho.php",
                    content);

                var respString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var creado = JsonSerializer.Deserialize<CrearDespachoResponse>(respString, options);

                if (creado != null && creado.success)
                {
                    // Folio "corto" para el usuario (ej: 17)
                    // Folio corto para el usuario (ej: 17)
                    int despachoId = creado.despacho_id;
                    string folioUsuario = despachoId.ToString();

                    // Folio interno del servidor (DESP-20251126145608)
                    string folioInterno = creado.folio ?? string.Empty;

                    // Nombre exacto del archivo Excel
                    string nombreDocumento = $"{despachoId}_{folioInterno}.xlsx";

                    // El Excel mostrará en G2 el folio visible (17)
                    // y se guardará como 17_DESP-20251126145608.xlsx
                    await GenerarExcelDespachoAsync(folioUsuario, despachoId, nombreDocumento, seleccionados);

                    await DisplayAlert(
                        "Despacho creado",
                        $"Despacho creado correctamente.\nFolio: {folioUsuario}",
                        "OK");

                    await CargarPendientesDesdeApiAsync();

                }

                else
                {
                    string msg = (creado != null && !string.IsNullOrWhiteSpace(creado.message))
                        ? creado.message
                        : respString;

                    await DisplayAlert(
                        "Error",
                        $"No se pudo crear el despacho.\nRespuesta del servidor:\n{msg}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Ocurrió un problema al generar el despacho.\n\nDetalle: {ex.Message}",
                    "OK");
            }
            finally
            {
                Activity.IsRunning = false;
                Activity.IsVisible = false;
                BtnGenerar.IsEnabled = true;
                BtnConsultar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Genera el Excel usando la plantilla RegistroDespacho.xlsx
        /// con logo CMPC y formato oficial.
        /// Además sube una copia al servidor usando despacho_id.
        /// </summary>
        private async Task GenerarExcelDespachoAsync(
            string folioVisible,              // lo que ve el usuario (17)
            int despachoId,
            string nombreDocumento,           // 17_DESP-20251126145608.xlsx
            List<CosechaPendiente> seleccionados)
        {
            if (seleccionados == null || seleccionados.Count == 0)
                return;

            using var assetStream = await FileSystem.OpenAppPackageFileAsync("RegistroDespacho.xlsx");
            using var memoryStream = new MemoryStream();
            await assetStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var ws = workbook.Worksheet(1);

            var first = seleccionados[0];

            // En el Excel el folio será el número corto (17)
            ws.Cell("G2").Value = folioVisible;
            ws.Cell("C7").Value = first.especie;
            ws.Cell("C8").Value = first.predio;
            ws.Cell("F7").Value = first.temporada;
            ws.Cell("F8").Value = first.huerto_nombre;

            // 3) Rellenar filas de la tabla (máximo 15 filas: 12..26)
            int row = 12;
            int index = 1;

            foreach (var c in seleccionados)
            {
                if (row > 26) break; // máximo 15 registros en la hoja

                // Columna A: N°
                ws.Cell(row, 1).Value = index;

                // Columna B: Fecha cosecha (dd/MM/yyyy)
                if (DateTime.TryParse(c.fecha_cosecha, out var fecha))
                {
                    ws.Cell(row, 2).Value = fecha;
                    ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else
                {
                    ws.Cell(row, 2).Value = c.fecha_cosecha;
                }

                // Columna C: Genotipo
                ws.Cell(row, 3).Value = c.genotipo;

                // Columna D: Réplica
                ws.Cell(row, 4).Value = c.replica;

                // Columna E: Fila
                ws.Cell(row, 5).Value = c.fila;

                // Columna F: Columna
                ws.Cell(row, 6).Value = c.columna;

                // Columna G: Kilos cosechados
                if (double.TryParse(
                        c.kilos,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var kilosNum))
                {
                    ws.Cell(row, 7).Value = kilosNum;
                }
                else
                {
                    ws.Cell(row, 7).Value = c.kilos;
                }

                row++;
                index++;
            }

            // 4) Total kilos en G27 (suma de G12:G26)
            ws.Cell("G27").FormulaA1 = "SUM(G12:G26)";

            // 5) Guardar en carpeta Descargas con nombre FOLIO_ddMMyyyy.xlsx
            //string fechaStr = DateTime.Now.ToString("ddMMyyyy");
            // folio ya viene como "17_DESP-20251126145608"
            string fileName = nombreDocumento;

#if ANDROID
    string path = Android.OS.Environment
        .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
        .AbsolutePath;
#else
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif

            string fullPath = Path.Combine(path, fileName);
            workbook.SaveAs(fullPath);

            await SubirExcelServidorAsync(despachoId, fullPath);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fullPath)
            });

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Despacho {folioVisible}",
                File = new ShareFile(fullPath)
            });
        }
    }

    // ======================= MODELOS DE RESPUESTA =======================

    public class PendientesResponse
    {
        public bool success { get; set; }
        public List<CosechaPendiente>? data { get; set; }
        public string? message { get; set; }
    }

    public class CosechaPendiente
    {
        public string rc_id { get; set; } = string.Empty;      // VIENE COMO STRING
        public string tree_id { get; set; } = string.Empty;    // STRING
        public string fecha_cosecha { get; set; } = string.Empty;
        public string genotipo { get; set; } = string.Empty;

        public int temporada { get; set; }                     // NÚMERO
        public string especie { get; set; } = string.Empty;
        public int replica { get; set; }
        public int fila { get; set; }
        public int columna { get; set; }

        public string predio { get; set; } = string.Empty;
        public string huerto_nombre { get; set; } = string.Empty;
        public int cod_huerto { get; set; }

        public string kilos { get; set; } = string.Empty;      // STRING
        public string cosechador { get; set; } = string.Empty;

        public int despachado { get; set; }                    // 0 / 1
        public int? despacho_id { get; set; }                  // puede venir null
        public string created_at { get; set; } = string.Empty;

        // Propiedad solo para la app (no viene del JSON)
        public bool IsSelected { get; set; } = false;
    }

    public class CrearDespachoResponse
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public int despacho_id { get; set; }
        public string? folio { get; set; }
        public List<int>? cosechas { get; set; }
    }
}
