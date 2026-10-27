using System.Threading.Tasks;

namespace CampusGuide.Services
{
    public class ScanResult
    {
        public string Value { get; set; } = string.Empty;
    }

    public interface IQRCodeScannerService
    {
        Task<ScanResult?> ScanAsync();
    }
}
