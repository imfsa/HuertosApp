using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using HuertosApp.Services;
using HuertosApp.Models;

namespace HuertosApp.Pages
{
    public partial class SubmenuCosechaOperacional : ContentPage
    {
        public SubmenuCosechaOperacional()
        {
            InitializeComponent();
        }

        // 1) Registro de Cosecha
        private async void OnRegistroCosechaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NuevoRegistroCosechaPage());

            //await DisplayAlert("Registro de Cosecha",
            //    "Aquí iremos a la página para registrar la cosecha operacional (QR + formulario).",
            //    "OK");
        }

        // 2) Consultar datos
        private async void OnConsultarCosechaClicked(object sender, EventArgs e)
        {
            // TODO: cuando tengas la pantalla:
            // await Navigation.PushAsync(new ConsultarCosechaOperacionalPage());


            
        {
            await Navigation.PushAsync(new ConsultarCosechaOperacionalPage());
        }

        //await DisplayAlert("Consultar Cosecha",
        //        "Aquí se mostrará la lista de cosechas operacionales guardadas en SQLite.",
        //        "OK");
        }

        // 3) Transmitir datos
        private async void OnTransmitirDatosClicked(object sender, EventArgs e)
        {
            // Aquí más adelante usaremos la misma idea que en comercial:
            // - leer cosechas no enviadas de SQLite
            // - llamar a la API de cosecha operacional
            // - marcar como enviadas
            await Navigation.PushAsync(new TransmitirCosechaOperacionalPage());
            //await DisplayAlert("Transmitir Datos",
            //    "Más adelante conectaremos esta opción con la API de cosecha operacional.",
            //    "OK");
        }

        // 4) Registro de Despacho
        private async void OnRegistroDespachoClicked(object sender, EventArgs e)
        {
            // Aquí luego:
            await Navigation.PushAsync(new SubmenuDespachoPage());

            //await DisplayAlert("Registro de Despacho",
            //    "Aquí podrás agrupar cosechas en un despacho (folio, fecha, documento).",
            //    "OK");
        }

        // 5) Salir
        private async void OnSalirClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmación", "¿Deseas volver al menú principal?", "Sí", "No");
            if (confirm)
            {
                await Navigation.PopAsync(); // volvemos a la pantalla anterior
            }
        }
    }
}
