using Microsoft.Maui.Controls;

namespace HuertosApp.Pages
{
    public partial class SubmenuDespachoPage : ContentPage
    {
        public SubmenuDespachoPage()
        {
            InitializeComponent();
        }

        private async void OnNuevoDespachoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NuevoDespachoPage());
        }

        private async void OnConsultarDespachosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarDespachosPage());
        }

        private async void OnSalirClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
