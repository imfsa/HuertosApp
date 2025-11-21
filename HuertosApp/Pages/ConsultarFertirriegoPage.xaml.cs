using HuertosApp.Models;
using HuertosApp.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HuertosApp.Pages
{
    public partial class ConsultarFertirriegoPage : ContentPage
    {
        public ConsultarFertirriegoPage()
        {
            InitializeComponent();
        }

        // Evento para consultar datos
        private async void OnConsultarClicked(object sender, EventArgs e)
        {
            await ConsultarDatosFertirriegoAsync();
        }

        // Método para consultar los datos de fertirriego
        private async Task ConsultarDatosFertirriegoAsync()
        {
            List<Fertirriego> registros = new List<Fertirriego>();

            try
            {
                // Obtener la fecha seleccionada del DatePicker como string
                string fechaSeleccionada = fechaPicker.Date.ToString("yyyy-MM-dd");

                // Consultar registros que coincidan con la fecha seleccionada
                registros = await Database.GetDatabase().Table<Fertirriego>()
                    .Where(r => r.FechaRiego == fechaSeleccionada)
                    .ToListAsync();

                registrosLayout.Children.Clear();

                // Verificar si hay registros
                if (registros.Count == 0)
                {
                    await DisplayAlert("Información", "No se encontraron registros para la fecha seleccionada.", "OK");
                    return;
                }

                // Crear filas para cada registro
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

        // Método para crear una fila de registro
        private View CrearRegistroRow(Fertirriego registro)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Columna: Huerto
            var huertoLabel = new Label
            {
                Text = registro.Huerto,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            row.Add(huertoLabel, 0);

            // Columna: Fecha
            var fechaLabel = new Label
            {
                Text = registro.FechaRiego,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            row.Add(fechaLabel, 1);

            // Columna: Sector
            var sectorLabel = new Label
            {
                Text = registro.Sector,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            row.Add(sectorLabel, 2);

            // Botón: Editar
            var editButton = new Button
            {
                Text = "✏️",
                CommandParameter = registro,
                BackgroundColor = Colors.Transparent,
                WidthRequest = 40,
                HeightRequest = 40
            };
            editButton.Clicked += async (s, e) => await OnEditClicked(s, e);
            row.Add(editButton, 3);

            // Botón: Eliminar
            var deleteButton = new Button
            {
                Text = "❌",
                CommandParameter = registro,
                BackgroundColor = Colors.Transparent,
                WidthRequest = 40,
                HeightRequest = 40
            };
            deleteButton.Clicked += async (s, e) => await OnDeleteClicked(s, e);
            row.Add(deleteButton, 4);

            return row;
        }

        // Método para editar un registro
        // Método para editar un registro
        private async Task OnEditClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var registro = (Fertirriego)button.CommandParameter;

            // Crear un resumen del registro
            string resumen = $"Huerto: {registro.Huerto}\n" +
                             $"Fecha de Riego: {registro.FechaRiego}\n" +
                             $"Sector: {registro.Sector}\n" +
                             $"Fertilizantes: {registro.Fertilizantes}\n" +
                             $"Tipo de Riego: {registro.TipoRiego}\n" +
                             $"Tiempo de Riego: {registro.TiempoRiego}\n" +
                             $"M³ Sistema: {registro.M3Sistema}\n" +
                             $"mm Sistema: {registro.MmSistema}\n" +
                             $"EVP Promedio: {registro.EVP_Prom}\n" +
                             $"Fertilizante Est. 1: {registro.FertilizanteEst1}\n" +
                             $"Fertilizante Est. 2: {registro.FertilizanteEst2}\n" +
                             $"Fertilizante Est. 3: {registro.FertilizanteEst3}\n" +
                             $"Fertilizante (Kg): {registro.FertilizanteKg}\n" +
                             $"Observación: {registro.Observacion}";

            // Mostrar el resumen en un DisplayAlert
            bool editar = await DisplayAlert("Editar", resumen, "Editar", "Cancelar");

            // Si el usuario selecciona "Editar", redirigir a la página de edición
            if (editar)
            {
                await Navigation.PushAsync(new EditarRegistroFertirriegoPage(registro));
            }
        }


        // Método para eliminar un registro
        private async Task OnDeleteClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var registro = (Fertirriego)button.CommandParameter;

            bool confirm = await DisplayAlert("Confirmación", "¿Deseas eliminar este registro?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    await Database.GetDatabase().DeleteAsync(registro);
                    await DisplayAlert("Éxito", "Registro eliminado correctamente.", "OK");
                    await ConsultarDatosFertirriegoAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
                }
            }
        }
    }
}
