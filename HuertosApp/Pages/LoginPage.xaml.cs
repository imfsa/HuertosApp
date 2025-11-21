using Microsoft.Maui.Controls;
using HuertosApp.Models;
using HuertosApp.Services;
using System;

namespace HuertosApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            idEntry.Text = "Admin";
            passwordEntry.Text = "123";
        }

        // ======== LOGIN =========
        private async void eventBtnLogin(object sender, EventArgs e)
        {
            if (!Database.DatabaseExists())
            {
                await DisplayAlert("Error",
                    "La base de datos no existe. Por favor, actualiza los datos primero.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(idEntry.Text) ||
                string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert("Error",
                    "Usuario y Contraseña son obligatorios.",
                    "OK");
                return;
            }

            var database = Database.GetDatabase();

            var usuario = await database.Table<Usuario>()
                                        .Where(u => u.Nombre == idEntry.Text &&
                                                    u.Password == passwordEntry.Text)
                                        .FirstOrDefaultAsync();

            if (usuario != null)
            {
                App.CurrentUser = usuario;

                // IMPORTANTE: Descarga de Arboles antes de ir al menú
                bool okArboles = await Database.DescargarArbolesOperacionalesAsync(
                    "http://179.61.13.234:8089/ws_huertosappV2/datos_arboles_operacional.php"
                );

                if (!okArboles)
                {
                    await DisplayAlert("Advertencia",
                        "No se pudieron descargar los árboles operacionales.",
                        "OK");
                }

                await Navigation.PushAsync(new MenuPage());
            }
            else
            {
                await DisplayAlert("Error",
                    "Usuario o contraseña incorrectos.",
                    "OK");
            }
        }

        // ======== ACTUALIZAR DATOS =========
        private async void UpdateData(object sender, EventArgs e)
        {
            string urlUsuarios = "http://179.61.13.234:8089/ws_huertosapp/usuario.php";

            bool ok = await Database.DescargarArbolesOperacionalesAsync(
                "http://179.61.13.234:8089/ws_huertosappV2/datos_arboles_operacional.php"
            );

            await Database.UpdateDatabaseFromJsonAsync(urlUsuarios);

            await DisplayAlert("Actualización",
                ok ? "Datos actualizados correctamente."
                   : "Usuarios actualizados. Arboles NO pudieron descargarse.",
                "OK");
        }
    }
}
