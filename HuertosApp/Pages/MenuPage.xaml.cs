using Microsoft.Maui.Controls;

namespace HuertosApp.Pages
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        // Evento para navegar a la página del Submenu de Fertirriego
        private async void OnFertirriegoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SubmenuFertirriegoPage());
        }

        // Evento para navegar a la página del Submenu de Cosecha Rodal Comercial
        private async void OnCosechaComercialClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SubmenuCosechaComercial());
        }


        private async void OnCosechaOperacionalClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SubmenuCosechaOperacional());
        }

    }
}
