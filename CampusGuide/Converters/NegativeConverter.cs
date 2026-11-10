using System.Globalization;
using Microsoft.Maui.Controls;

namespace CampusGuide.Converters
{
    /// <summary>
    /// Converts a numeric value to its negation (inverts boolean-like numeric values).
    /// Used to show "no results" message when result count is 0.
    /// </summary>
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return true;

            if (int.TryParse(value.ToString(), out int intValue))
            {
                return intValue == 0;
            }

            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
