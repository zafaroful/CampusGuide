using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace CampusGuide.Views.Controls
{
    public static class IconFactory
    {
        private const string CalendarData = "M 3,5 H 21 V 21 H 3 Z M 3,9 H 21 M 8,2 V 6 M 16,2 V 6";
        public const string PinData = "M 12,2 C 8.1,2 5,5.1 5,9 C 5,14.2 12,22 12,22 C 12,22 19,14.2 19,9 C 19,5.1 15.9,2 12,2 Z M 12,11.5 A 2.5,2.5 0 1 1 12,6.5 A 2.5,2.5 0 1 1 12,11.5 Z";
        private const string BellData = "M 12,22 C 13.1,22 14,21.1 14,20 H 10 C 10,21.1 10.9,22 12,22 Z M 18,16 V 11 C 18,7.9 16.4,5.5 13.5,4.8 V 4 C 13.5,3.2 12.8,2.5 12,2.5 C 11.2,2.5 10.5,3.2 10.5,4 V 4.8 C 7.6,5.5 6,7.9 6,11 V 16 L 4,18 V 19 H 20 V 18 Z";
        public const string AlertData = "M 12,3 L 22,20 H 2 Z M 12,10 V 14 M 12,17 V 17.5";
        public const string InfoData = "M 12,3 A 9,9 0 1 0 12,21 A 9,9 0 1 0 12,3 Z M 12,11 V 17 M 12,7.5 V 8";
        private const string CloseData = "M 6,6 L 18,18 M 18,6 L 6,18";
        private const string InboxData = "M 3,13 L 6,4 H 18 L 21,13 V 20 H 3 Z M 3,13 H 8 L 9,16 H 15 L 16,13 H 21";
        private const string SearchData = "M 10,3 A 7,7 0 1 0 10,17 A 7,7 0 1 0 10,3 Z M 15,15 L 21,21";

        public static Microsoft.Maui.Controls.Shapes.Path Calendar(Color color, double size = 16) => Create(CalendarData, color, size, filled: false);

        public static Microsoft.Maui.Controls.Shapes.Path Pin(Color color, double size = 14) => Create(PinData, color, size, filled: true);

        public static Microsoft.Maui.Controls.Shapes.Path Bell(Color color, double size = 16) => Create(BellData, color, size, filled: true);

        public static Microsoft.Maui.Controls.Shapes.Path Alert(Color color, double size = 16) => Create(AlertData, color, size, filled: false);

        public static Microsoft.Maui.Controls.Shapes.Path Info(Color color, double size = 16) => Create(InfoData, color, size, filled: false);

        public static Microsoft.Maui.Controls.Shapes.Path Close(Color color, double size = 12) => Create(CloseData, color, size, filled: false);

        public static Microsoft.Maui.Controls.Shapes.Path Inbox(Color color, double size = 48) => Create(InboxData, color, size, filled: false);

        public static Microsoft.Maui.Controls.Shapes.Path Search(Color color, double size = 48) => Create(SearchData, color, size, filled: false);

        public static Geometry ToGeometry(string data) => (Geometry)new PathGeometryConverter().ConvertFromInvariantString(data)!;

        private static Microsoft.Maui.Controls.Shapes.Path Create(string data, Color color, double size, bool filled) =>
            Style(new Microsoft.Maui.Controls.Shapes.Path { Data = ToGeometry(data) }, color, size, filled);

        private static Microsoft.Maui.Controls.Shapes.Path Style(Microsoft.Maui.Controls.Shapes.Path path, Color color, double size, bool filled)
        {
            path.Aspect = Stretch.Uniform;
            path.WidthRequest = size;
            path.HeightRequest = size;
            path.VerticalOptions = LayoutOptions.Center;
            path.HorizontalOptions = LayoutOptions.Center;

            if (filled)
            {
                path.Fill = color;
            }
            else
            {
                path.Stroke = color;
                path.StrokeThickness = 2;
                path.StrokeLineCap = PenLineCap.Round;
                path.StrokeLineJoin = PenLineJoin.Round;
            }

            return path;
        }
    }
}
