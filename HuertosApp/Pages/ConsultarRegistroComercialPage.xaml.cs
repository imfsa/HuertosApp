using HuertosApp.Models;
using HuertosApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HuertosApp.Pages
{
    public partial class ConsultarRegistroComercialPage : ContentPage
    {
        public ConsultarRegistroComercialPage()
        {
            InitializeComponent();

            // Establece la fecha actual en el DatePicker
            fechaPicker.Date = DateTime.Today;
        }

        // Evento que se ejecuta al aparecer la página
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ConsultarDatosComercialAsync(); // Carga inicial de datos
        }

        // Evento para consultar registros
        private async void OnConsultarClicked(object sender, EventArgs e)
        {
            await ConsultarDatosComercialAsync();
        }

        // Método para consultar registros comerciales
        private async Task ConsultarDatosComercialAsync()
        {
            try
            {
                // Obtener la fecha seleccionada
                string fechaSeleccionada = fechaPicker.Date.ToString("yyyy-MM-dd");

                // Consulta registros de la base de datos filtrados por fecha
                var registros = await Database.GetDatabase()
                    .Table<RegistroComercial>()
                    .Where(r => r.Fecha == fechaSeleccionada)
                    .ToListAsync();

                // Limpiar la vista anterior
                registrosLayout.Children.Clear();

                // Mostrar mensaje si no hay registros
                if (registros.Count == 0)
                {
                    await DisplayAlert("Información", "No se encontraron registros.", "OK");
                    return;
                }

                // Crear filas para los registros encontrados
                foreach (var registro in registros)
                {
                    var row = CrearRegistroRow(registro);
                    registrosLayout.Children.Add(row);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al consultar datos: {ex.Message}", "OK");
            }
        }

        // Crear fila de registro
        private View CrearRegistroRow(RegistroComercial registro)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = 40 },
                    new ColumnDefinition { Width = 40 }
                }
            };

            // Agregar datos a las columnas
            grid.Add(new Label { Text = registro.Predio.ToString(), VerticalOptions = LayoutOptions.Center }, 0, 0);
            grid.Add(new Label { Text = registro.Rodal.ToString(), VerticalOptions = LayoutOptions.Center }, 1, 0);
            grid.Add(new Label { Text = registro.Fecha, VerticalOptions = LayoutOptions.Center }, 2, 0);

            // Botón Editar
            var editButton = new Button { Text = "✏️", CommandParameter = registro };
            editButton.Clicked += async (s, e) =>
            {
                // Previsualizar registro antes de editar
                string resumen = $"Fecha: {registro.Fecha}\n" +
                                 $"Genotipo: {registro.Genotipo}\n" +
                                 $"Temporada: {registro.Temporada}\n" +
                                 $"Predio: {registro.Predio}\n" +
                                 $"Rodal: {registro.Rodal}\n" +
                                 $"Kilos: {registro.Kilos}\n" +
                                 $"Cosechador: {registro.Cosechador}\n" +
                                 $"Modalidad: {registro.Modalidad}\n" +
                                 $"Arboles/Día: {registro.ArbolesDia}\n" +
                                 $"Total Árboles: {registro.TotalArboles}";

                // Mostrar el resumen en un alert antes de editar
                bool confirm = await DisplayAlert("Editar Registro", resumen, "Editar", "Cancelar");
                if (confirm)
                {
                    // Navegar a la página de edición
                    await Navigation.PushAsync(new EditarRegistroComercialPage(registro));
                }
            };
            grid.Add(editButton, 3, 0);

            // Botón Eliminar
            var deleteButton = new Button { Text = "❌", CommandParameter = registro };
            deleteButton.Clicked += async (s, e) =>
            {
                bool confirm = await DisplayAlert("Eliminar", "¿Desea eliminar el registro?", "Sí", "No");
                if (confirm)
                {
                    await Database.DeleteRegistroComercialAsync(registro); // Eliminar registro
                    await ConsultarDatosComercialAsync(); // Recargar datos
                }
            };
            grid.Add(deleteButton, 4, 0);

            return grid;
        }
    }
}
