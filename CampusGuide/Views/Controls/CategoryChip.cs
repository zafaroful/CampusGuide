using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace CampusGuide.Views.Controls
{
    public class CategoryChip : ContentView
    {
        private readonly Border _border;
        private readonly Label _label;
        private bool _isSelected;

        public event EventHandler? Tapped;

        public CategoryChip(string text, Color chipColor)
        {
            _label = new Label
            {
                Text = text,
                FontSize = 13,
                VerticalTextAlignment = TextAlignment.Center
            };

            _border = new Border
            {
                Padding = new Thickness(14, 8),
                MinimumHeightRequest = 44,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Stroke = MapBorderColor,
                Content = _label
            };

            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += (s, e) => Tapped?.Invoke(this, EventArgs.Empty);
            _border.GestureRecognizers.Add(tapRecognizer);

            Content = _border;
            SemanticProperties.SetDescription(this, $"Filter locations by {text}");
            UpdateVisualState();
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateVisualState();
            }
        }

        private static Color MapPrimaryColor => (Color)Application.Current!.Resources["MapPrimary"];

        private static Color MapBorderColor => (Color)Application.Current!.Resources["MapBorder"];

        private static Color MapCardColor => (Color)Application.Current!.Resources["MapCard"];

        private static Color MapTextPrimaryColor => (Color)Application.Current!.Resources["MapTextPrimary"];

        private void UpdateVisualState()
        {
            _border.BackgroundColor = _isSelected ? MapPrimaryColor : MapCardColor;
            _border.Stroke = _isSelected ? MapPrimaryColor : MapBorderColor;
            _label.TextColor = _isSelected ? Colors.White : MapTextPrimaryColor;
            _label.FontAttributes = _isSelected ? FontAttributes.Bold : FontAttributes.None;
        }
    }
}

