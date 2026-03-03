using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using HuertosApp.Models;
using HuertosApp.Services;

namespace HuertosApp.Pages
{
    public partial class NuevoRegistroCosechaPage : ContentPage
    {
        private readonly List<CosechadorModel> cosechadores;
        private bool _isScanning = false;
        private bool _hasShownHelp = false;
        private bool _isFullScreen = false;

        public NuevoRegistroCosechaPage()
        {
            InitializeComponent();

            // Fecha del día actual
            FechaEntry.Text = DateTime.Now.ToString("dd/MM/yyyy");

            // Estado de despacho por defecto
            EstadoDespachoEntry.Text = "NO";

            // Lista de cosechadores
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Mensaje inicial de ayuda (una vez por instancia)
            if (!_hasShownHelp)
            {
                _hasShownHelp = true;
                await DisplayAlert(
                    "Registro de Cosecha",
                    "Aquí podrás escanear el código QR del árbol y registrar los kilos cosechados.",
                    "Entendido");
            }

            // Configurar lector QR
            if (cameraView != null)
            {
                cameraView.Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormat.QrCode,
                    AutoRotate = true,
                    Multiple = false
                };

                cameraView.IsDetecting = false;
            }

            _isScanning = false;
            ScannerOverlay.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DetenerScanner();
        }

        // ==================== CONTROL DEL POPUP / SCANNER ====================

        private void BtnMostrarScanner_Clicked(object sender, EventArgs e)
        {
            if (cameraView == null)
                return;

            // Iniciar en modo normal (no pantalla completa)
            _isFullScreen = false;
            ConfigurarTamañoScanner();

            ScannerOverlay.IsVisible = true;
            cameraView.IsDetecting = true;
            _isScanning = true;
        }

        private void BtnCerrarScanner_Clicked(object sender, EventArgs e)
        {
            DetenerScanner();
        }

        private void BtnToggleTorch_Clicked(object sender, EventArgs e)
        {
            if (cameraView == null)
                return;

            cameraView.IsTorchOn = !cameraView.IsTorchOn;
            
            // Actualizar el texto del botón
            if (btnTorch != null)
            {
                btnTorch.Text = cameraView.IsTorchOn ? "💡 ON" : "💡 OFF";
                btnTorch.BackgroundColor = cameraView.IsTorchOn 
                    ? Color.FromArgb("#FCD34D") 
                    : Color.FromArgb("#374151");
            }
        }

        private void BtnToggleFullScreen_Clicked(object sender, EventArgs e)
        {
            _isFullScreen = !_isFullScreen;
            ConfigurarTamañoScanner();
            
            // Actualizar ícono del botón
            if (btnFullScreen != null)
            {
                btnFullScreen.Text = _isFullScreen ? "⛶" : "⛶";
                btnFullScreen.BackgroundColor = _isFullScreen 
                    ? Color.FromArgb("#2563EB") 
                    : Color.FromArgb("#374151");
            }
        }

        private void ConfigurarTamañoScanner()
        {
            if (ScannerContainer == null || cameraView == null)
                return;

            if (_isFullScreen)
            {
                // MODO PANTALLA COMPLETA
                ScannerContainer.WidthRequest = -1;
                ScannerContainer.HeightRequest = -1;
                ScannerContainer.Margin = new Thickness(0);
                ScannerContainer.HorizontalOptions = LayoutOptions.Fill;
                ScannerContainer.VerticalOptions = LayoutOptions.Fill;
                
                cameraView.HeightRequest = -1;
                cameraView.WidthRequest = -1;
            }
            else
            {
                // MODO POPUP RESPONSIVE
                // Obtener el ancho de la pantalla
                var screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
                var screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
                
                // Calcular dimensiones con padding de 40px (20 a cada lado)
                var popupWidth = Math.Min(screenWidth - 40, 500);  // Máximo 500px
                var popupHeight = Math.Min(screenHeight * 0.7, 600); // Máximo 70% de la pantalla o 600px
                
                // Calcular tamaño de la cámara (90% del popup menos padding)
                var cameraSize = Math.Min(popupWidth - 60, popupHeight - 150);
                
                ScannerContainer.WidthRequest = popupWidth;
                ScannerContainer.HeightRequest = popupHeight;
                ScannerContainer.Margin = new Thickness(20);
                ScannerContainer.HorizontalOptions = LayoutOptions.Center;
                ScannerContainer.VerticalOptions = LayoutOptions.Center;
                
                // Tamaño cuadrado para la cámara
                cameraView.HeightRequest = cameraSize;
                cameraView.WidthRequest = cameraSize;
            }
        }

        private void DetenerScanner()
        {
            if (cameraView != null)
            {
                cameraView.IsDetecting = false;
                cameraView.IsTorchOn = false;
            }

            _isScanning = false;
            _isFullScreen = false;
            ScannerOverlay.IsVisible = false;
            
            // Resetear botón de linterna
            if (btnTorch != null)
            {
                btnTorch.Text = "💡 OFF";
                btnTorch.BackgroundColor = Color.FromArgb("#374151");
            }
        }

        // EVENTO DE ZXING AL DETECTAR CÓDIGO
        private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results?.FirstOrDefault();
            if (result == null)
                return;

            var value = result.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Detenemos el escaneo y ocultamos popup
                DetenerScanner();
                await ProcesarCodigoAsync(value);
            });
        }

        /// <summary>
        /// Procesa el texto leído del QR.
        /// Soporta:
        ///  - "15"
        ///  - "HQT|1|000015" → toma el último segmento como tree_id
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
                long treeIdLong;

                // Formato payload HQT|1|000001
                if (texto.Contains("|"))
                {
                    var partes = texto.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var ultimo = partes.LastOrDefault();

                    if (ultimo == null || !long.TryParse(ultimo, out treeIdLong))
                    {
                        await DisplayAlert("QR no válido",
                            $"No se pudo extraer un ID de árbol desde: {texto}", "OK");
                        return;
                    }
                }
                else
                {
                    if (!long.TryParse(texto, out treeIdLong))
                    {
                        await DisplayAlert("QR no válido",
                            $"El código leído no es un ID de árbol numérico: {texto}", "OK");
                        return;
                    }
                }

                // Mostrar ID
                TreeIdEntry.Text = treeIdLong.ToString();

                // Buscar árbol en SQLite (usa string como definimos en Database)
                var arbol = await Database.GetArbolByIdAsync(treeIdLong.ToString());
                if (arbol == null)
                {
                    await DisplayAlert("No encontrado",
                        $"No se encontró el árbol con ID {treeIdLong} en la base local.", "OK");
                    return;
                }

                // Rellenar campos autocompletados
                TemporadaEntry.Text = arbol.Temporada;
                GenotipoEntry.Text = arbol.Genotipo;
                EspecieEntry.Text = arbol.Especie;
                ReplicaEntry.Text = arbol.Replica;
                FilaEntry.Text = arbol.Fila;
                ColumnaEntry.Text = arbol.Columna;
                PredioEntry.Text = arbol.Predio;
                CodHuertoEntry.Text = arbol.CodHuerto;
                HuertoNombreEntry.Text = arbol.HuertoNombre;

                await DisplayAlert("Código detectado",
                    $"Árbol {treeIdLong} cargado correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    $"Ocurrió un error al procesar el código:\n{ex.Message}", "OK");
            }
        }

        // ==================== REGISTRO EN SQLITE ====================

        //private async void BtnRegistrar_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // Validaciones básicas
        //        if (string.IsNullOrWhiteSpace(TreeIdEntry.Text))
        //        {
        //            await DisplayAlert("Falta escanear",
        //                "Debe escanear un árbol antes de registrar la cosecha.", "OK");
        //            return;
        //        }

        //        if (!decimal.TryParse(
        //                KilosEntry.Text?.Replace(',', '.'),
        //                System.Globalization.NumberStyles.Any,
        //                System.Globalization.CultureInfo.InvariantCulture,
        //                out decimal kilos))
        //        {
        //            await DisplayAlert("Kilos inválidos",
        //                "Ingrese un valor numérico válido para los kilos.", "OK");
        //            return;
        //        }

        //        if (kilos <= 0 || kilos > 100)
        //        {
        //            await DisplayAlert("Kilos fuera de rango",
        //                "Los kilos deben estar entre 1 y 100.", "OK");
        //            return;
        //        }

        //        var seleccionados = cosechadores
        //            .Where(c => c.IsSelected)
        //            .Select(c => c.Nombre)
        //            .ToList();

        //        if (!seleccionados.Any())
        //        {
        //            await DisplayAlert("Sin cosechadores",
        //                "Debe seleccionar al menos un cosechador.", "OK");
        //            return;
        //        }

        //        // Construir RegistroCosecha EXACTO a tu modelo
        //        var registro = new RegistroCosecha
        //        {
        //            TreeId = long.Parse(TreeIdEntry.Text),
        //            FechaCosecha = FechaEntry.Text ?? DateTime.Now.ToString("yyyy-MM-dd"),

        //            Temporada = int.Parse(TemporadaEntry.Text),
        //            Genotipo = GenotipoEntry.Text ?? "",
        //            Especie = EspecieEntry.Text ?? "",
        //            Replica = int.Parse(ReplicaEntry.Text),
        //            Fila = int.Parse(FilaEntry.Text),
        //            Columna = int.Parse(ColumnaEntry.Text),
        //            Predio = PredioEntry.Text ?? "",
        //            CodHuerto = int.Parse(CodHuertoEntry.Text),
        //            HuertoNombre = HuertoNombreEntry.Text ?? "",

        //            Kilos = kilos,
        //            Cosechador = string.Join(", ", seleccionados),

        //            Despachado = false,
        //            DespachoId = null,
        //            CreatedAt = DateTime.Now
        //        };

        //        await Database.InsertRegistroCosechaAsync(registro);

        //        var accion = await DisplayActionSheet(
        //            "Registro guardado correctamente",
        //            "Cancelar",
        //            null,
        //            "Nuevo registro",
        //            "Volver al menú");

        //        if (accion == "Nuevo registro")
        //        {
        //            LimpiarFormulario();
        //        }
        //        else if (accion == "Volver al menú")
        //        {
        //            await Navigation.PopAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error",
        //            $"No se pudo guardar el registro:\n{ex.Message}", "OK");
        //    }
        //}



        // BOTÓN REGISTRAR: guarda en SQLite con tu modelo RegistroCosecha
        private async void BtnRegistrar_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(TreeIdEntry.Text))
                {
                    await DisplayAlert("Falta escanear",
                        "Debe escanear un árbol antes de registrar la cosecha.", "OK");
                    return;
                }

                if (!decimal.TryParse(
                        KilosEntry.Text?.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal kilos))
                {
                    await DisplayAlert("Kilos inválidos",
                        "Ingrese un valor numérico válido para los kilos.", "OK");
                    return;
                }

                if (kilos <= 0 || kilos > 100)
                {
                    await DisplayAlert("Kilos fuera de rango",
                        "Los kilos deben estar entre 1 y 100.", "OK");
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

                // ================== NUEVO VALIDADOR DE DUPLICADO ==================
                // Fecha interna SIEMPRE en formato ISO, independiente de lo que se muestre
                var fechaCosecha = DateTime.Today.ToString("yyyy-MM-dd");
                long treeId = long.Parse(TreeIdEntry.Text);

                // Traemos todos los registros de cosecha de hoy
                var registrosHoy = await Database.GetRegistrosCosechaByFechaAsync(fechaCosecha);

                // ¿Existe ya un registro para el mismo árbol hoy?
                bool yaExisteMismoArbolHoy = registrosHoy.Any(r => r.TreeId == treeId);

                if (yaExisteMismoArbolHoy)
                {
                    bool continuar = await DisplayAlert(
                        "Aviso",
                        "Este árbol ya tiene un registro de cosecha para el día de hoy.\n\n" +
                        "¿Deseas registrar de todos modos?",
                        "Sí, registrar",
                        "No, cancelar");

                    if (!continuar)
                    {
                        // El usuario decidió no registrar
                        return;
                    }
                }
                // ================================================================

                // Construir RegistroCosecha (modelo EXACTO que enviaste)
                //var registro = new RegistroCosecha
                //{
                //    TreeId = treeId,
                //    FechaCosecha = fechaCosecha,

                //    Temporada = int.Parse(TemporadaEntry.Text),
                //    Genotipo = GenotipoEntry.Text ?? "",
                //    Especie = EspecieEntry.Text ?? "",
                //    Replica = int.Parse(ReplicaEntry.Text),
                //    Fila = int.Parse(FilaEntry.Text),
                //    Columna = int.Parse(ColumnaEntry.Text),
                //    Predio = PredioEntry.Text ?? "",
                //    CodHuerto = int.Parse(CodHuertoEntry.Text),
                //    HuertoNombre = HuertoNombreEntry.Text ?? "",

                //    Kilos = kilos,
                //    Cosechador = string.Join(", ", seleccionados),

                //    Despachado = false,
                //    DespachoId = null,
                //    CreatedAt = DateTime.Now


                //};
                // Obtén el usuario actual desde donde lo guardes tú (Preferences, singleton, etc.)
                var nombreUsuario = Preferences.Get("NombreUsuario", "sin_usuario"); // ejemplo


                // Construir RegistroCosecha (modelo EXACTO que enviaste)
                var registro = new RegistroCosecha
                {
                    TreeId = treeId,
                    FechaCosecha = fechaCosecha,

                    Temporada = int.Parse(TemporadaEntry.Text),
                    Genotipo = GenotipoEntry.Text ?? "",
                    Especie = EspecieEntry.Text ?? "",
                    Replica = int.Parse(ReplicaEntry.Text),
                    Fila = int.Parse(FilaEntry.Text),
                    Columna = int.Parse(ColumnaEntry.Text),
                    Predio = PredioEntry.Text ?? "",
                    CodHuerto = int.Parse(CodHuertoEntry.Text),
                    HuertoNombre = HuertoNombreEntry.Text ?? "",

                    Kilos = kilos,
                    Cosechador = string.Join(", ", seleccionados),

                    Despachado = false,
                    DespachoId = null,
                    CreatedAt = DateTime.Now,

                    // TODO: aquí cuando tengas el usuario logueado real, lo reemplazas
                    UsuarioId = App.CurrentUser.Id, // Usuario actual
                    Sincronizado = false
                };


                // Guardar en SQLite
                await Database.InsertRegistroCosechaAsync(registro);

                // Flujo post-registro
                var accion = await DisplayActionSheet(
                    "Registro guardado correctamente",
                    "Cancelar",
                    null,
                    "Nuevo registro",
                    "Volver al menú");

                if (accion == "Nuevo registro")
                {
                    LimpiarFormulario();
                }
                else if (accion == "Volver al menú")
                {
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    $"No se pudo guardar el registro:\n{ex.Message}", "OK");
            }
        }

        private async void BtnVolver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void LimpiarFormulario()
        {
            TreeIdEntry.Text = "";
            TemporadaEntry.Text = "";
            GenotipoEntry.Text = "";
            EspecieEntry.Text = "";
            ReplicaEntry.Text = "";
            FilaEntry.Text = "";
            ColumnaEntry.Text = "";
            PredioEntry.Text = "";
            CodHuertoEntry.Text = "";
            HuertoNombreEntry.Text = "";
            KilosEntry.Text = "";

            foreach (var c in cosechadores)
                c.IsSelected = false;

            collectionViewCosechadores.ItemsSource = null;
            collectionViewCosechadores.ItemsSource = cosechadores;

            FechaEntry.Text = DateTime.Now.ToString("dd/MM/yyyy");  // solo visual
            EstadoDespachoEntry.Text = "NO";

            // Scanner apagado y oculto, el usuario decide si vuelve a escanear
            DetenerScanner();
        }
    }
}
