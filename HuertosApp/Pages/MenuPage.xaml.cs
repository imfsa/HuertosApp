using Microsoft.Maui.Controls;

namespace HuertosApp.Pages
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private async void OnFertirriegoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SubmenuFertirriegoPage());
        }

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
