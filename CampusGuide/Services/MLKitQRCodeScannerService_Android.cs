using System.Threading.Tasks;

namespace CampusGuide.Services
{
#if ANDROID
    // Android-specific ML Kit barcode scanner stub - implement camera preview + ML Kit processing.
    public class MLKitQRCodeScannerService_Android : IQRCodeScannerService
    {
        public Task<ScanResult?> ScanAsync()
        {
            // Real implementation should open camera, feed frames to ML Kit barcode scanner, and return the result.
            throw new System.NotImplementedException();
        }
    }
#endif
}
