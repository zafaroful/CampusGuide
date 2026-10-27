using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace CampusGuide.Services
{
    public static class PermissionHelper
    {
        public static async Task<bool> EnsureCameraPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>().ConfigureAwait(false);
                if (status == PermissionStatus.Granted)
                    return true;

                status = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
                return status == PermissionStatus.Granted;
            }
            catch
            {
                return false;
            }
        }
    }
}
