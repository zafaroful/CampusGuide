using Microsoft.Maui.Controls;

namespace CampusGuide.Views.Controls
{
    public class StateView : VerticalStackLayout
    {
        public Label MessageLabel { get; }

        public StateView(View? icon, string title, string? message = null, string? actionText = null, Action? onAction = null)
        {
            Spacing = 12;
            Padding = 40;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;

            if (icon != null)
            {
                Add(icon);
            }

            Add(new Label
            {
                Text = title,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = ThemeHelper.BodyText,
                HorizontalTextAlignment = TextAlignment.Center
            });

            MessageLabel = new Label
            {
                Text = message ?? string.Empty,
                FontSize = 14,
                TextColor = ThemeHelper.MutedText,
                HorizontalTextAlignment = TextAlignment.Center
            };
            Add(MessageLabel);

            if (!string.IsNullOrEmpty(actionText) && onAction != null)
            {
                var actionButton = new Button
                {
                    Text = actionText,
                    BackgroundColor = ThemeHelper.Accent,
                    TextColor = Colors.White,
                    CornerRadius = 20,
                    Padding = new Thickness(24, 8),
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalOptions = LayoutOptions.Center
                };
                actionButton.Clicked += (s, e) => onAction();
                Add(actionButton);
            }
        }
    }
}
