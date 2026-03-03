using HuertosApp.Models;
using HuertosApp.Models;
using HuertosApp.Pages;
using HuertosApp.Services;
using SQLitePCL;

namespace HuertosApp
{
    public partial class App : Application
    {
        public static Usuario CurrentUser { get; set; }

        public App()
        {
            InitializeComponent(); // Inicializa los componentes de la aplicación

            Batteries_V2.Init();

            Database.InitializeDatabase(); // Inicializa la base de datos

            // Forzar tema claro (Light mode) siempre
            UserAppTheme = AppTheme.Light;

            MainPage = new NavigationPage(new LoginPage()); // Establece la página principal de la aplicación como LoginPage envuelta en un NavigationPage
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Asegurar que el tema claro se mantenga
            window.Created += (s, e) =>
            {
                UserAppTheme = AppTheme.Light;
            };

            return window;
        }
    }
}

