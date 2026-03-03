using HuertosApp.Models;
using HuertosApp.Services;

namespace HuertosApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        private CancellationTokenSource? _cancellationTokenSource;

        public LoginPage()
        {
            InitializeComponent();
            // Valores por defecto para pruebas rápidas
            idEntry.Text = "Admin";
            passwordEntry.Text = "123";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async void eventBtnLogin(object sender, EventArgs e)
        {
            if (_cancellationTokenSource?.IsCancellationRequested == true)
                return;

            if (!Database.DatabaseExists())
            {
                await DisplayAlert("Error", "Base de datos no encontrada. Actualiza los datos.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(idEntry.Text) || string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert("Atención", "Ingresa credenciales válidas.", "OK");
                return;
            }

            var database = Database.GetDatabase();
            var usuario = await database.Table<Usuario>()
                .Where(u => u.Nombre == idEntry.Text && u.Password == passwordEntry.Text)
                .FirstOrDefaultAsync();

            if (usuario != null)
            {
                App.CurrentUser = usuario;

                try
                {
                    // Descarga de árboles antes de entrar al menú
                    bool okArboles = await Database.DescargarArbolesOperacionalesAsync(
                        "https://api.imf.cl:8443/huertosappV2/datos_arboles_operacional.php"
                    );

                    if (!okArboles && _cancellationTokenSource?.IsCancellationRequested == false)
                    {
                        await DisplayAlert("Sincronización", "Modo offline: No se actualizaron árboles nuevos.", "OK");
                    }

                    if (_cancellationTokenSource?.IsCancellationRequested == false)
                    {
                        await Navigation.PushAsync(new MenuPage());
                    }
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                {
                    // Operación cancelada, no hacer nada
                }
            }
            else
            {
                await DisplayAlert("Acceso Denegado", "Usuario o contraseña incorrectos.", "OK");
            }
        }

        // ==================== MENÚ DESPLEGABLE ====================

        private void OnMenuClicked(object sender, EventArgs e)
        {
            menuPopup.IsVisible = !menuPopup.IsVisible;
        }

        private void OnCerrarMenu(object sender, EventArgs e)
        {
            menuPopup.IsVisible = false;
        }

        private async void OnDescargarArboles(object sender, EventArgs e)
        {
            if (_cancellationTokenSource?.IsCancellationRequested == true)
                return;

            menuPopup.IsVisible = false;

            try
            {
                bool ok = await Database.DescargarArbolesOperacionalesAsync(
                    "https://api.imf.cl:8443/huertosappV2/datos_arboles_operacional.php"
                );

                if (_cancellationTokenSource?.IsCancellationRequested == false)
                {
                    if (ok)
                    {
                        await DisplayAlert("Éxito", "Árboles descargados correctamente.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudieron descargar los árboles. Verifica tu conexión.", "OK");
                    }
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // Operación cancelada, no hacer nada
            }
        }

        private async void OnDescargarUsuarios(object sender, EventArgs e)
        {
            if (_cancellationTokenSource?.IsCancellationRequested == true)
                return;

            menuPopup.IsVisible = false;

            try
            {
                string urlUsuarios = "https://api.imf.cl:8443/huertosapp/usuario.php";
                await Database.UpdateDatabaseFromJsonAsync(urlUsuarios);

                if (_cancellationTokenSource?.IsCancellationRequested == false)
                {
                    await DisplayAlert("Éxito", "Usuarios descargados correctamente.", "OK");
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // Operación cancelada, no hacer nada
            }
        }

        private async void UpdateData(object sender, EventArgs e)
        {
            if (_cancellationTokenSource?.IsCancellationRequested == true)
                return;

            menuPopup.IsVisible = false;

            try
            {
                string urlUsuarios = "https://api.imf.cl:8443/huertosapp/usuario.php";

                // Sincronización general
                bool okArboles = await Database.DescargarArbolesOperacionalesAsync(
                    "https://api.imf.cl:8443/huertosappV2/datos_arboles_operacional.php"
                );

                await Database.UpdateDatabaseFromJsonAsync(urlUsuarios);

                if (_cancellationTokenSource?.IsCancellationRequested == false)
                {
                    await DisplayAlert("Sincronización",
                        okArboles ? "Datos actualizados correctamente." : "Usuarios actualizados. Error al actualizar árboles.",
                        "OK");
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // Operación cancelada, no hacer nada
            }
        }
    }
}