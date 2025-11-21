using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace HuertosApp.Pages
{
    public partial class NuevoRegistroComercialPage : ContentPage
    {
        private List<CosechadorModel> cosechadores; // Lista de cosechadores

        public NuevoRegistroComercialPage()
        {
            InitializeComponent();
            datePickerFecha.Date = DateTime.Now; // Fecha automática
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

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

        // **Botón Registrar**
        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            try
            {
                // **Validar campos obligatorios**
                if (!ValidarCampos())
                {
                    await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                    return;
                }

                // **Validar Kilos (números y rango)**
                string kilos = entryKilos.Text?.Trim();
                if (!ValidarKilos(kilos))
                {
                    await DisplayAlert("Error", "Los kilos deben ser un número entre 0 y 100.", "OK");
                    return;
                }

                // Obtener cosechadores seleccionados
                var seleccionados = cosechadores.FindAll(c => c.IsSelected).ConvertAll(c => c.Nombre);
                string cosechadoresSeleccionados = string.Join(", ", seleccionados);

                // Crear nuevo registro
                var nuevoRegistro = new RegistroComercial
                {
                    Fecha = datePickerFecha.Date.ToString("yyyy-MM-dd"),
                    Genotipo = entryGenotipo.Text,
                    Temporada = pickerTemporada.SelectedItem.ToString(),
                    Predio = int.Parse(entryPredio.Text),
                    Rodal = int.Parse(entryRodal.Text),
                    Kilos = kilos, // Guardar como string
                    Modalidad = pickerModalidad.SelectedItem.ToString(),
                    ArbolesDia = int.Parse(entryArbolesDia.Text),
                    TotalArboles = int.Parse(entryTotalArboles.Text),
                    Cosechador = cosechadoresSeleccionados,
                    UsuarioId = App.CurrentUser.Id // Usuario actual
                };

                // Guardar registro en la base de datos
                await Database.InsertRegistroComercialAsync(nuevoRegistro);

                // Mostrar mensaje de éxito
                await DisplayAlert("Éxito", "Registro guardado correctamente.", "OK");

                // Confirmar si desea agregar otro registro
                var respuesta = await DisplayAlert("Registro Guardado", "¿Deseas ingresar otro registro?", "Sí", "No");
                if (respuesta)
                {
                    LimpiarCampos(); // Limpiar formulario
                }
                else
                {
                    await Navigation.PushAsync(new SubmenuCosechaComercial());
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                await DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
            }
        }

        // **Validar Campos Obligatorios**
        private bool ValidarCampos()
        {
            return !string.IsNullOrEmpty(entryPredio.Text) &&
                   !string.IsNullOrEmpty(entryRodal.Text) &&
                   !string.IsNullOrEmpty(entryKilos.Text) &&
                   pickerTemporada.SelectedItem != null &&
                   cosechadores.Exists(c => c.IsSelected) && // Al menos un cosechador seleccionado
                   pickerModalidad.SelectedItem != null &&
                   !string.IsNullOrEmpty(entryArbolesDia.Text) &&
                   !string.IsNullOrEmpty(entryTotalArboles.Text);
        }

        // **Validar Kilos (Rango y formato decimal)**
        private bool ValidarKilos(string kilos)
        {
            if (string.IsNullOrWhiteSpace(kilos))
                return false;

            if (decimal.TryParse(kilos, out decimal kilosDecimal))
            {
                // Validar rango entre 0 y 100
                return kilosDecimal >= 0 && kilosDecimal <= 100;
            }

            return false;
        }

        // **Limpiar Campos del Formulario**
        private void LimpiarCampos()
        {
            entryPredio.Text = string.Empty;
            entryRodal.Text = string.Empty;
            entryKilos.Text = string.Empty;
            pickerTemporada.SelectedItem = null;
            pickerModalidad.SelectedItem = null;
            entryArbolesDia.Text = string.Empty;
            entryTotalArboles.Text = string.Empty;

            // Reiniciar cosechadores
            foreach (var cosechador in cosechadores)
            {
                cosechador.IsSelected = false;
            }

            // Actualizar la lista visual
            collectionViewCosechadores.ItemsSource = null;
            collectionViewCosechadores.ItemsSource = cosechadores;
        }
    }
}
