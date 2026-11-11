using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CampusGuide.Views.Controls
{
    public static class ThemeHelper
    {
        public static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

        public static Color Themed(string light, string dark) => Color.Parse(IsDark ? dark : light);

        public static Color Accent => Themed("#00695C", "#4DB6AC");

        public static Color MutedText => Themed("#6B6B6B", "#B0B3B8");

        public static Color BodyText => Themed("#333333", "#E6E6E6");

        public static Color Danger => Themed("#D32F2F", "#FF8A80");

        public static Color CardBackground => Themed("#FFFFFF", "#1E2124");

        public static Color CardBorder => Themed("#E0E0E0", "#3A3D42");
    }
}
