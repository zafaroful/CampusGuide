using System.Threading.Tasks;

namespace CampusGuide.Services
{
    /// <summary>
    /// Cross-platform QR/Barcode scanner using ZXing.Net.MAUI.
    /// This implements the simple ScanAsync interface; the actual camera UI 
    /// is handled by QRScannerPage which uses CameraBarcodeReaderView.
    /// </summary>
    public class ZXingQRCodeScannerService : IQRCodeScannerService
    {
        private TaskCompletionSource<ScanResult?>? _scanCompletionSource;

        /// <summary>
        /// Register a callback for when a barcode is scanned.
        /// Called from QRScannerPage when BarcodesDetected fires.
        /// </summary>
        public void OnBarcodeDetected(string barcodeValue)
        {
            if (!string.IsNullOrEmpty(barcodeValue))
            {
                _scanCompletionSource?.TrySetResult(new ScanResult { Value = barcodeValue });
            }
        }

        public async Task<ScanResult?> ScanAsync()
        {
            _scanCompletionSource = new TaskCompletionSource<ScanResult?>();

            // Set timeout of 30 seconds for scanning
            var scanTask = _scanCompletionSource.Task;
            var delayTask = Task.Delay(30000);

            var completedTask = await Task.WhenAny(scanTask, delayTask).ConfigureAwait(false);

            if (completedTask == delayTask)
            {
                // Timeout — cancel and return null
                _scanCompletionSource.TrySetResult(null);
                return null;
            }

            return await scanTask.ConfigureAwait(false);
        }
    }
}
