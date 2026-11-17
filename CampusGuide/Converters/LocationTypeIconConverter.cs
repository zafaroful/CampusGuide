using System.Globalization;
using Microsoft.Maui.Controls;

namespace CampusGuide.Converters
{
    public class LocationTypeIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var type = (value as string)?.ToLowerInvariant() ?? string.Empty;

            if (type.Contains("librar")) return "📚";
            if (type.Contains("lab")) return "🧪";
            if (type.Contains("cafe") || type.Contains("food") || type.Contains("dining")) return "🍽️";
            if (type.Contains("office") || type.Contains("admin")) return "🏢";
            if (type.Contains("sport") || type.Contains("gym")) return "🏃";
            if (type.Contains("lecture") || type.Contains("class")) return "🎓";
            if (type.Contains("parking")) return "🅿️";

            return "📍";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
