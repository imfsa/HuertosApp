using Android.App;
using Android.Content.PM;
using Android.OS;

namespace HuertosApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                
                // Forzar orientación vertical
                RequestedOrientation = ScreenOrientation.Portrait;
            }
            catch (System.Exception)
            {
                // Ignorar excepciones durante la creación
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
            }
            catch (System.Exception)
            {
                // Ignorar excepciones durante la destrucción para evitar errores del depurador
            }
            finally
            {
                // Limpiar recursos
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
            }
            catch (System.Exception)
            {
                // Ignorar excepciones
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
            }
            catch (System.Exception)
            {
                // Ignorar excepciones
            }
        }
    }
}
