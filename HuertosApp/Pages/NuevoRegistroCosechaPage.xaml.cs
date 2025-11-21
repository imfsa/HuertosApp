using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using HuertosApp.Models;
using HuertosApp.Services;

namespace HuertosApp.Pages
{
    public partial class NuevoRegistroCosechaPage : ContentPage
    {
        private readonly List<CosechadorModel> cosechadores;

        public NuevoRegistroCosechaPage()
        {
            InitializeComponent();

            // Fecha del día actual (solo lectura)
            FechaEntry.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Estado de despacho por defecto
            EstadoDespachoEntry.Text = "NO";

            // Inicializar lista de cosechadores
            cosechadores = new List<CosechadorModel>
            {
                new CosechadorModel { Nombre = "HS", IsSelected = false },
                new CosechadorModel { Nombre = "WP", IsSelected = false },
                new CosechadorModel { Nombre = "BF", IsSelected = false },
                new CosechadorModel { Nombre = "MM", IsSelected = false },
                new CosechadorModel { Nombre = "SG", IsSelected = false },
                new CosechadorModel { Nombre = "AC", IsSelected = false },
                new CosechadorModel { Nombre = "RC", IsSelected = false }
            };

            collectionViewCosechadores.ItemsSource = cosechadores;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Configurar opciones del lector (solo QR)
            if (cameraView != null)
            {
                cameraView.Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormat.QrCode,
                    AutoRotate = true,
                    Multiple = false
                };

                cameraView.IsDetecting = true;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (cameraView != null)
            {
                cameraView.IsDetecting = false;
            }
        }

        // Evento que dispara ZXing cuando detecta un código
        private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results?.FirstOrDefault();
            if (result == null)
                return;

            var value = result.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            // Ejecutar en el hilo de UI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Pausamos detección para que no se dispare en bucle
                if (cameraView != null)
                    cameraView.IsDetecting = false;

                await ProcesarCodigoAsync(value);
            });
        }

        /// <summary>
        /// Procesa el texto leído del QR.
        /// Admite:
        ///  - "15"
        ///  - "HQT|1|000015"  → toma el último segmento como tree_id
        /// </summary>
        private async Task ProcesarCodigoAsync(string codigoLeido)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigoLeido))
                {
                    await DisplayAlert("QR vacío", "No se pudo leer un código válido.", "OK");
                    return;
                }

                string texto = codigoLeido.Trim();
                long treeId;

                // Si viene en formato payload HQT|1|000001
                if (texto.Contains("|"))
                {
                    var partes = texto.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var ultimo = partes.LastOrDefault();

                    if (ultimo == null || !long.TryParse(ultimo, out treeId))
                    {
                        await DisplayAlert("QR no válido",
                            $"No se pudo extraer un ID de árbol desde: {texto}", "OK");
                        return;
                    }
                }
                else
                {
                    // Formato simple: solo el ID
                    if (!long.TryParse(texto, out treeId))
                    {
                        await DisplayAlert("QR no válido",
                            $"El código leído no es un ID de árbol numérico: {texto}", "OK");
                        return;
                    }
                }

                // Mostrar el ID en el campo (solo lectura)
                TreeIdEntry.Text = treeId.ToString();

                // Buscar árbol en SQLite (GetArbolByIdAsync debe recibir long)
                var arbol = await Database.GetArbolByIdAsync(treeId.ToString());
                if (arbol == null)
                {
                    await DisplayAlert("No encontrado",
                        $"No se encontró el árbol con ID {treeId} en la base local.", "OK");
                    return;
                }

                // Rellenar campos autocompletados
                TemporadaEntry.Text = arbol.Temporada.ToString();
                GenotipoEntry.Text = arbol.Genotipo;
                EspecieEntry.Text = arbol.Especie;
                ReplicaEntry.Text = arbol.Replica.ToString();
                FilaEntry.Text = arbol.Fila.ToString();
                ColumnaEntry.Text = arbol.Columna.ToString();
                PredioEntry.Text = arbol.Predio;
                CodHuertoEntry.Text = arbol.CodHuerto.ToString();
                HuertoNombreEntry.Text = arbol.HuertoNombre;

                await DisplayAlert("Código detectado",
                    $"Árbol {treeId} cargado correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    $"Ocurrió un error al procesar el código:\n{ex.Message}", "OK");
            }
            finally
            {
                // Si quieres que siga leyendo después del mensaje:
                if (cameraView != null)
                    cameraView.IsDetecting = true;
            }
        }

        private void BtnReanudar_Clicked(object sender, EventArgs e)
        {
            if (cameraView != null)
            {
                cameraView.IsDetecting = true;
            }
        }

        private async void BtnRegistrar_Clicked(object sender, EventArgs e)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(TreeIdEntry.Text))
            {
                await DisplayAlert("Falta escanear",
                    "Debe escanear un árbol antes de registrar la cosecha.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(KilosEntry.Text) ||
                !double.TryParse(KilosEntry.Text.Replace(',', '.'),
                                 System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out double kilos))
            {
                await DisplayAlert("Kilos inválidos",
                    "Ingrese un valor numérico válido para los kilos.", "OK");
                return;
            }

            if (kilos <= 0 || kilos > 100)
            {
                await DisplayAlert("Kilos fuera de rango",
                    "Los kilos deben estar entre 0 y 100.", "OK");
                return;
            }

            var seleccionados = cosechadores
                .Where(c => c.IsSelected)
                .Select(c => c.Nombre)
                .ToList();

            if (!seleccionados.Any())
            {
                await DisplayAlert("Sin cosechadores",
                    "Debe seleccionar al menos un cosechador.", "OK");
                return;
            }

            // Solo mostramos resumen por ahora (después guardamos en SQLite)
            var mensaje =
                $"Árbol: {TreeIdEntry.Text}\n" +
                $"Fecha: {FechaEntry.Text}\n" +
                $"Kilos: {kilos}\n" +
                $"Cosechadores: {string.Join(", ", seleccionados)}\n" +
                $"Despachado: {EstadoDespachoEntry.Text}";

            await DisplayAlert("Registro de cosecha", mensaje, "OK");

            // TODO: cuando definamos el modelo RegistroCosechaLocal,
            // aquí haremos el Insert en SQLite.
        }

        private async void BtnVolver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
