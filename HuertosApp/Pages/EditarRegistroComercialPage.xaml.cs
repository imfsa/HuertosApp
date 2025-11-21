using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuertosApp.Pages
{
    public partial class EditarRegistroComercialPage : ContentPage
    {
        private readonly RegistroComercial _registro; // Registro a editar
        private List<CosechadorModel> _cosechadores;  // Lista de cosechadores

        public EditarRegistroComercialPage(RegistroComercial registro)
        {
            InitializeComponent();
            _registro = registro;

            // Cargar datos existentes
            CargarDatos();
            InicializarCosechadores();
        }

        // **Cargar datos del registro existente**
        private void CargarDatos()
        {
            fechaPicker.Date = DateTime.Parse(_registro.Fecha); // Fecha
            entryGenotipo.Text = _registro.Genotipo;            // Genotipo
            pickerTemporada.SelectedItem = _registro.Temporada; // Temporada
            entryPredio.Text = _registro.Predio.ToString();     // Predio
            entryRodal.Text = _registro.Rodal.ToString();       // Rodal
            entryKilos.Text = _registro.Kilos;                  // Kilos (ahora string)
            pickerModalidad.SelectedItem = _registro.Modalidad; // Modalidad
            entryArbolesDia.Text = _registro.ArbolesDia.ToString();   // Árboles por día
            entryTotalArboles.Text = _registro.TotalArboles.ToString(); // Total árboles
        }

        // **Inicializar cosechadores**
        private void InicializarCosechadores()
        {
            // Lista de cosechadores disponibles
            _cosechadores = new List<CosechadorModel>
            {
                new CosechadorModel { Nombre = "HS", IsSelected = false },
                new CosechadorModel { Nombre = "WP", IsSelected = false },
                new CosechadorModel { Nombre = "BF", IsSelected = false },
                new CosechadorModel { Nombre = "MM", IsSelected = false },
                new CosechadorModel { Nombre = "DP", IsSelected = false },
                new CosechadorModel { Nombre = "SG", IsSelected = false },
                new CosechadorModel { Nombre = "AC", IsSelected = false },
                new CosechadorModel { Nombre = "RC", IsSelected = false }
            };

            // Marcar cosechadores seleccionados previamente
            var seleccionados = _registro.Cosechador.Split(", ").ToList();
            foreach (var cosechador in _cosechadores)
            {
                if (seleccionados.Contains(cosechador.Nombre))
                {
                    cosechador.IsSelected = true; // Seleccionar
                }
            }

            // Asignar lista al CollectionView
            collectionViewCosechadores.ItemsSource = _cosechadores;
        }

        // **Guardar cambios en el registro**
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Validar campos obligatorios
                if (!ValidarCampos())
                {
                    await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                    return;
                }

                // Validar rangos numéricos
                if (!ValidarRangosNumericos(out decimal kilos))
                {
                    await DisplayAlert("Error", "Revise los valores ingresados en los campos numéricos.", "OK");
                    return;
                }

                // Actualizar valores
                _registro.Fecha = fechaPicker.Date.ToString("yyyy-MM-dd");
                _registro.Genotipo = entryGenotipo.Text;
                _registro.Temporada = pickerTemporada.SelectedItem.ToString();
                _registro.Predio = int.Parse(entryPredio.Text);
                _registro.Rodal = int.Parse(entryRodal.Text);
                _registro.Kilos = kilos.ToString(); // Convertir a string
                _registro.Modalidad = pickerModalidad.SelectedItem.ToString();
                _registro.ArbolesDia = int.Parse(entryArbolesDia.Text);
                _registro.TotalArboles = int.Parse(entryTotalArboles.Text);

                // Obtener cosechadores seleccionados
                var seleccionados = _cosechadores.FindAll(c => c.IsSelected).ConvertAll(c => c.Nombre);
                _registro.Cosechador = string.Join(", ", seleccionados);

                // Guardar cambios en la base de datos
                await Database.GetDatabase().UpdateAsync(_registro);

                await DisplayAlert("Éxito", "Registro actualizado correctamente.", "OK");
                await Navigation.PopAsync(); // Regresar
            }
            catch (Exception ex)
            {
                // Manejar errores
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
        }

        // **Validar campos obligatorios**
        private bool ValidarCampos()
        {
            return !string.IsNullOrEmpty(entryPredio.Text) &&
                   !string.IsNullOrEmpty(entryRodal.Text) &&
                   !string.IsNullOrEmpty(entryKilos.Text) &&
                   pickerTemporada.SelectedItem != null &&
                   _cosechadores.Exists(c => c.IsSelected) &&
                   pickerModalidad.SelectedItem != null &&
                   !string.IsNullOrEmpty(entryArbolesDia.Text) &&
                   !string.IsNullOrEmpty(entryTotalArboles.Text);
        }

        // **Validar rangos numéricos**
        private bool ValidarRangosNumericos(out decimal kilos)
        {
            kilos = 0;

            if (!decimal.TryParse(entryKilos.Text, out kilos) || kilos <= 0 || kilos > 100)
            {
                return false;
            }

            if (!int.TryParse(entryPredio.Text, out _) || int.Parse(entryPredio.Text) <= 0)
            {
                return false;
            }

            if (!int.TryParse(entryRodal.Text, out _) || int.Parse(entryRodal.Text) <= 0)
            {
                return false;
            }

            if (!int.TryParse(entryArbolesDia.Text, out _) || int.Parse(entryArbolesDia.Text) <= 0)
            {
                return false;
            }

            if (!int.TryParse(entryTotalArboles.Text, out _) || int.Parse(entryTotalArboles.Text) <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
