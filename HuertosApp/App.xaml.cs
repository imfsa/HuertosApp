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

            MainPage = new NavigationPage(new LoginPage()); // Establece la página principal de la aplicación como LoginPage envuelta en un NavigationPage
        }
    }
}
