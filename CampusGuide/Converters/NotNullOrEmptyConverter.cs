using System.Globalization;
using Microsoft.Maui.Controls;

namespace CampusGuide.Converters
{
    /// <summary>
    /// Returns true when the value is a non-empty string (used to hide empty optional labels).
    /// </summary>
    public class NotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
