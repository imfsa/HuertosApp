using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace HuertosApp.Services
{
    public static class PermissionsService
    {
        public static async Task<bool> RequestCameraAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
            return status == PermissionStatus.Granted;
        }
    }
}