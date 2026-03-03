using Android.App;
using Android.Content;
using Android.Runtime;

namespace HuertosApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnTerminate()
        {
            try
            {
                base.OnTerminate();
            }
            catch (System.Exception)
            {
                // Ignorar excepciones durante la terminación
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                base.OnLowMemory();
                Java.Lang.JavaSystem.Gc();
            }
            catch (System.Exception)
            {
                // Ignorar excepciones
            }
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            try
            {
                base.OnTrimMemory(level);
                if (level >= TrimMemory.RunningCritical)
                {
                    Java.Lang.JavaSystem.Gc();
                }
            }
            catch (System.Exception)
            {
                // Ignorar excepciones
            }
        }
    }
}
