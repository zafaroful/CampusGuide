using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace CampusGuide.Views.Controls
{
    public class CategoryChip : ContentView
    {
        private readonly Border _border;
        private readonly Label _label;
        private readonly Color _chipColor;
        private bool _isSelected;

        public event EventHandler? Tapped;

        public CategoryChip(string text, Color chipColor)
        {
            _chipColor = chipColor;

            _label = new Label
            {
                Text = text,
                FontSize = 12,
                VerticalTextAlignment = TextAlignment.Center
            };

            _border = new Border
            {
                Padding = new Thickness(10, 6),
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 2,
                Stroke = chipColor,
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

        private void UpdateVisualState()
        {
            _border.BackgroundColor = _isSelected ? _chipColor : Colors.White;
            _label.TextColor = _isSelected && _chipColor.GetLuminosity() < 0.5f ? Colors.White : Colors.Black;
            _label.FontAttributes = _isSelected ? FontAttributes.Bold : FontAttributes.None;
        }
    }
}
