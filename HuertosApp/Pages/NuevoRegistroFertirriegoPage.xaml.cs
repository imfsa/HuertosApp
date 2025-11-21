using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.Media;
using SkiaSharp;

namespace HuertosApp.Pages
{
    public partial class NuevoRegistroFertirriegoPage : ContentPage
    {
        private string _base64Image; // Para almacenar la imagen en Base64
        private List<FertilizanteModel> _fertilizantes; // Lista de fertilizantes
        private List<SectorModel> _sectores; // Lista de sectores

        public NuevoRegistroFertirriegoPage()
        {
            InitializeComponent();
            CargarFertilizantes(); // Cargar fertilizantes
            CargarSectores();      // Cargar sectores
            LimpiarCampos();       // Inicializar el formulario limpio
        }

        // Método para cargar fertilizantes
        private void CargarFertilizantes()
        {
            _fertilizantes = new List<FertilizanteModel>
            {
                new FertilizanteModel { Nombre = "Urea" },
                new FertilizanteModel { Nombre = "Muriato Potasio" },
                new FertilizanteModel { Nombre = "Sulfato de Magnesio" },
                new FertilizanteModel { Nombre = "Nitrato de Calcio" },
                new FertilizanteModel { Nombre = "Ácido fosfórico" },
                new FertilizanteModel { Nombre = "FMA" },
                new FertilizanteModel { Nombre = "Boronatro" },
                new FertilizanteModel { Nombre = "Sulfato Zinc" },
                new FertilizanteModel { Nombre = "Agroq. 1" },
                new FertilizanteModel { Nombre = "Ninguno" }
            };

            FertilizantesListView.ItemsSource = _fertilizantes;
        }

        // Método para cargar sectores
        private void CargarSectores()
        {
            _sectores = new List<SectorModel>
            {
                new SectorModel { Nombre = "1" },
                new SectorModel { Nombre = "2" },
                new SectorModel { Nombre = "Back. En" },
                new SectorModel { Nombre = "Pr. 4ta." },
                new SectorModel { Nombre = "820" }
            };

            SectoresListView.ItemsSource = _sectores;
        }

        // Evento para capturar la foto
        private async void OnCapturePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    _base64Image = await ConvertToBase64(photo);

                    FotoCapturada.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(_base64Image)));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al capturar la foto: {ex.Message}", "OK");
            }
        }

        private async Task<string> ConvertToBase64(FileResult photo)
        {
            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();

            var processedImage = await ResizeImageAsync(stream, 800, 600, 70);
            await processedImage.CopyToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();
            return Convert.ToBase64String(photoBytes);
        }

        private async Task<Stream> ResizeImageAsync(Stream input, int maxWidth, int maxHeight, int quality)
        {
            using var original = SKBitmap.Decode(input);
            int width = original.Width;
            int height = original.Height;
            float aspectRatio = (float)width / height;

            if (width > height)
            {
                width = maxWidth;
                height = (int)(maxWidth / aspectRatio);
            }
            else
            {
                height = maxHeight;
                width = (int)(maxHeight * aspectRatio);
            }

            var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            var imageStream = new MemoryStream();
            using var imageCodec = SKImage.FromBitmap(resized);
            imageCodec.Encode(SKEncodedImageFormat.Jpeg, quality).SaveTo(imageStream);
            imageStream.Seek(0, SeekOrigin.Begin);
            return imageStream;
        }

        // Evento para guardar el registro
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!ValidateRequiredFields())
            {
                await DisplayAlert("Error", "Por favor completa todos los campos obligatorios.", "OK");
                return;
            }

            if (!ValidateNumericFields())
            {
                await DisplayAlert("Error", "Uno o más campos tienen valores fuera del rango permitido.", "OK");
                return;
            }

            string NombreFoto = null;

            try
            {
                var database = Database.GetDatabase();

                if (!string.IsNullOrEmpty(_base64Image))
                {
                    var nuevaFoto = new Foto
                    {
                        Base64String = _base64Image,
                        Nombre = $"Foto_{DateTime.Now:yyyyMMdd_HHmmss}" + ".jpg",
                        Enviado = false
                    };

                    await database.InsertAsync(nuevaFoto);
                    NombreFoto = nuevaFoto.Nombre;
                }

                var fertilizantesSeleccionados = string.Join(", ",
                    _fertilizantes.Where(f => f.IsSelected).Select(f => f.Nombre));

                var sectoresSeleccionados = string.Join(", ",
                    _sectores.Where(s => s.IsSelected).Select(s => s.Nombre));

                var nuevoRegistro = new Fertirriego
                {
                    Huerto = PickerHuerto.SelectedItem?.ToString(),
                    EtapaFertilizacion = PickerEtapa.SelectedItem?.ToString(),
                    Anio = DateTime.Now.Year.ToString(),
                    Fertilizantes = fertilizantesSeleccionados,
                    TipoRiego = PickerTipoRiego.SelectedItem?.ToString(),
                    Sector = sectoresSeleccionados,
                    FechaRiego = DatePickerFechaRiego.Date.ToString("yyyy-MM-dd"),
                    TiempoRiego = EntryTiempoRiego.Text,
                    M3Sistema = EntryM3Sistema.Text,
                    MmSistema = EntryMmSistema.Text,
                    EVP_Prom = EntryEVPProm.Text,
                    FertilizanteEst1 = EntryFertilizanteEst1.Text,
                    FertilizanteEst2 = EntryFertilizanteEst2.Text,
                    FertilizanteEst3 = EntryFertilizanteEst3.Text,
                    FertilizanteKg = EntryFertilizanteKg.Text,
                    Observacion = EntryObservacion.Text,
                    NombreFoto = NombreFoto,
                    UsuarioId = App.CurrentUser.Id,
                };

                await database.InsertAsync(nuevoRegistro);

                var respuesta = await DisplayAlert("Registro Guardado", "¿Deseas ingresar otro registro?", "Sí", "No");
                if (respuesta)
                {
                    LimpiarCampos();
                }
                else
                {
                    await Navigation.PushAsync(new SubmenuFertirriegoPage());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
            }
        }

        // Validar campos obligatorios
        private bool ValidateRequiredFields()
        {
            return PickerHuerto.SelectedIndex > -1 &&
                   PickerEtapa.SelectedIndex > -1 &&
                   PickerTipoRiego.SelectedIndex > -1 &&
                   _fertilizantes.Any(f => f.IsSelected) &&
                   _sectores.Any(s => s.IsSelected) &&
                   !string.IsNullOrWhiteSpace(EntryTiempoRiego.Text) &&
                   !string.IsNullOrWhiteSpace(EntryM3Sistema.Text) &&
                   !string.IsNullOrWhiteSpace(EntryMmSistema.Text) &&
                   !string.IsNullOrWhiteSpace(EntryEVPProm.Text) &&
                   !string.IsNullOrWhiteSpace(EntryFertilizanteEst1.Text) &&
                   !string.IsNullOrWhiteSpace(EntryFertilizanteEst2.Text) &&
                   !string.IsNullOrWhiteSpace(EntryFertilizanteEst3.Text) &&
                   !string.IsNullOrWhiteSpace(EntryFertilizanteKg.Text);
        }

        // Validar campos numéricos
        private bool ValidateNumericFields()
        {
            return decimal.TryParse(EntryTiempoRiego.Text, out var tiempoRiego) && tiempoRiego >= 0 && tiempoRiego <= 48;
        }

        // Método para limpiar campos
        private void LimpiarCampos()
        {
            PickerHuerto.SelectedIndex = -1;
            PickerEtapa.SelectedIndex = -1;
            PickerTipoRiego.SelectedIndex = -1;

            foreach (var fertilizante in _fertilizantes) fertilizante.IsSelected = false;
            foreach (var sector in _sectores) sector.IsSelected = false;

            DatePickerFechaRiego.Date = DateTime.Now;
            EntryTiempoRiego.Text = string.Empty;
            EntryM3Sistema.Text = string.Empty;
            EntryMmSistema.Text = string.Empty;
            EntryEVPProm.Text = string.Empty;
            EntryFertilizanteEst1.Text = string.Empty;
            EntryFertilizanteEst2.Text = string.Empty;
            EntryFertilizanteEst3.Text = string.Empty;
            EntryFertilizanteKg.Text = string.Empty;
            EntryObservacion.Text = string.Empty;
            FotoCapturada.Source = null;
            _base64Image = null;
        }
    }
}
