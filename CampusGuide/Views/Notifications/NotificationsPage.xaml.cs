using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.ViewModels;
using CampusGuide.Models;
using CampusGuide.Services;
using CampusGuide.Views.Controls;

namespace CampusGuide.Views.Notifications
{
    public partial class NotificationsPage : ContentPage
    {
        private NotificationsViewModel? _viewModel;
        private readonly List<(Button Button, string? Type)> _filterChips = new();
        private string? _selectedFilter;

        public NotificationsPage()
        {
            _viewModel = ServiceHelper.GetService<NotificationsViewModel>();
            if (_viewModel == null)
            {
                _viewModel = new NotificationsViewModel();
            }
            BindingContext = _viewModel;
            Title = "Alerts";

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main layout with refresh capability
            var refreshView = new RefreshView
            {
                Command = _viewModel?.RefreshNotificationsCommand,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                RefreshColor = ThemeHelper.Accent
            };

            // Bind IsRefreshing to show loading state during refresh
            refreshView.SetBinding(RefreshView.IsRefreshingProperty, new Binding("IsLoading", source: _viewModel));

            // Grid for main content
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),  // Header
                    new RowDefinition(GridLength.Auto),  // Filter buttons
                    new RowDefinition(GridLength.Star)   // Content
                },
                Padding = 0
            };

            // Header
            var headerStack = new VerticalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(20, 15, 20, 10)
            };
            headerStack.Add(new Label
            {
                Text = "Notifications & Alerts",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold
            });
            headerStack.Add(new Label
            {
                Text = "Stay updated with campus alerts and reminders",
                FontSize = 14,
                TextColor = ThemeHelper.MutedText
            });
            mainGrid.Add(headerStack, 0, 0);

            // Filter buttons
            var filterStack = new HorizontalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20, 0, 20, 10),
                HorizontalOptions = LayoutOptions.Start
            };

            var allButton = CreateFilterChip("All", null, "Show all notifications");
            allButton.Clicked += (s, e) => FilterNotifications(null);
            filterStack.Add(allButton);

            var alertsButton = CreateFilterChip("Alerts", "Alert", "Show alerts only");
            alertsButton.Clicked += (s, e) => FilterNotifications("Alert");
            filterStack.Add(alertsButton);

            var notificationsButton = CreateFilterChip("Notifications", "Notification", "Show notifications only");
            notificationsButton.Clicked += (s, e) => FilterNotifications("Notification");
            filterStack.Add(notificationsButton);

            Grid.SetRow(filterStack, 1);
            mainGrid.Add(filterStack, 0, 1);
            UpdateFilterChips();

            // Content area (Grid for layering)
            var contentGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Star)
                },
                Padding = 0
            };
            Grid.SetRow(contentGrid, 2);
            mainGrid.Add(contentGrid, 0, 2);

            var loadingView = new StateView(new ActivityIndicator { IsRunning = true, Color = ThemeHelper.Accent, WidthRequest = 36, HeightRequest = 36 }, "Loading notifications...");
            loadingView.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
            contentGrid.Add(loadingView);

            // CollectionView for notifications
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.Single,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = 0
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, new Binding("FilteredNotifications", source: _viewModel));
            collectionView.ItemTemplate = new DataTemplate(CreateNotificationTemplate);

            contentGrid.Add(collectionView);

            var emptyView = new StateView(IconFactory.Inbox(ThemeHelper.MutedText, 48), "You're all caught up", "No new alerts or reminders right now.");
            emptyView.SetBinding(IsVisibleProperty, new Binding("HasFilteredNotifications", source: _viewModel, converter: new InvertedBooleanConverter()));
            contentGrid.Add(emptyView);

            var errorView = new StateView(IconFactory.Alert(ThemeHelper.Danger, 48), "Something went wrong", null, "Dismiss", () =>
            {
                if (_viewModel != null)
                    _viewModel.ErrorMessage = string.Empty;
            });
            errorView.MessageLabel.SetBinding(Label.TextProperty, new Binding("ErrorMessage", source: _viewModel));
            errorView.MessageLabel.TextColor = ThemeHelper.Danger;
            errorView.MessageLabel.FontAttributes = FontAttributes.Bold;
            errorView.SetBinding(IsVisibleProperty, new Binding("ErrorMessage", source: _viewModel, converter: new IsNotNullOrEmptyConverter()));
            contentGrid.Add(errorView);

            refreshView.Content = mainGrid;
            Content = refreshView;
        }

        private void FilterNotifications(string? type)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterByType(type);
            }
            _selectedFilter = type;
            UpdateFilterChips();
        }

        private Button CreateFilterChip(string text, string? type, string description)
        {
            var chip = new Button
            {
                Text = text,
                Padding = new Thickness(16, 8),
                FontSize = 12,
                CornerRadius = 20,
                BorderWidth = 1
            };
            SemanticProperties.SetDescription(chip, description);
            _filterChips.Add((chip, type));
            return chip;
        }

        private void UpdateFilterChips()
        {
            foreach (var (chip, type) in _filterChips)
            {
                var selected = type == _selectedFilter;
                chip.BackgroundColor = selected ? ThemeHelper.Accent : Colors.Transparent;
                chip.BorderColor = ThemeHelper.Accent;
                chip.TextColor = selected ? Colors.White : ThemeHelper.Accent;
            }
        }

        private VerticalStackLayout CreateNotificationTemplate()
        {
            // Header with type indicator
            var headerStack = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(0, 0, 0, 8),
                VerticalOptions = LayoutOptions.Start
            };

            // Title
            var titleLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            titleLabel.SetBinding(Label.TextProperty, "Title");
            headerStack.Add(titleLabel);

            // Type badge with dynamic color
            var typeBadge = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                Padding = new Thickness(8, 4),
                HorizontalTextAlignment = TextAlignment.Center
            };
            typeBadge.SetBinding(Label.TextProperty, new Binding("Type", stringFormat: "{0}"));
            typeBadge.SetBinding(Label.TextColorProperty, new Binding("Type", converter: new NotificationTypeColorConverter()));
            typeBadge.SetBinding(BackgroundColorProperty, new Binding("Type", converter: new NotificationTypeBgColorConverter()));
            headerStack.Add(typeBadge);

            // Main content
            var container = new VerticalStackLayout
            {
                Padding = new Thickness(15, 12),
                Spacing = 8
            };
            container.BackgroundColor = ThemeHelper.CardBackground;

            // Add header
            container.Add(headerStack);

            // Body/Content with better formatting
            var bodyLabel = new Label
            {
                FontSize = 14,
                TextColor = ThemeHelper.BodyText,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 4, 0, 0)
            };
            bodyLabel.SetBinding(Label.TextProperty, "Body");
            container.Add(bodyLabel);

            // Footer with date and metadata
            var footerStack = new HorizontalStackLayout
            {
                Spacing = 15,
                Margin = new Thickness(0, 8, 0, 0),
                VerticalOptions = LayoutOptions.End
            };

            // Date label
            var dateLabel = new Label
            {
                FontSize = 12,
                TextColor = ThemeHelper.MutedText,
                HorizontalOptions = LayoutOptions.Start
            };
            dateLabel.SetBinding(Label.TextProperty, new Binding("CreatedAt", stringFormat: "{0:MMM d, h:mm tt}"));
            footerStack.Add(dateLabel);

            // Spacer
            footerStack.Add(new BoxView
            {
                Color = Colors.Transparent,
                HorizontalOptions = LayoutOptions.FillAndExpand
            });

            // Icon or indicator (optional)
            var indicatorIcon = new Microsoft.Maui.Controls.Shapes.Path
            {
                Aspect = Stretch.Uniform,
                WidthRequest = 14,
                HeightRequest = 14,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Fill = ThemeHelper.Accent
            };
            indicatorIcon.SetBinding(Microsoft.Maui.Controls.Shapes.Path.DataProperty, new Binding("Type", converter: new NotificationTypeIconConverter()));
            footerStack.Add(indicatorIcon);

            container.Add(footerStack);

            // Border effect using a modern Border control with drop shadow effect
            var border = new Border
            {
                Content = container,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                Stroke = ThemeHelper.CardBorder,
                StrokeThickness = 1,
                Padding = 0,
                Margin = new Thickness(16, 8),
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Offset = new Point(0, 2),
                    Opacity = 0.1f,
                    Radius = 4
                }
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnNotificationCardTapped;
            border.GestureRecognizers.Add(tapGesture);

            return new VerticalStackLayout { border };
        }

        private static async void OnNotificationCardTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: NotificationItem notification })
                return;

            System.Diagnostics.Debug.WriteLine($"NotificationsPage: Notification tapped: {notification.Title} (EventId: {notification.RelatedEventId}, LocationId: {notification.RelatedLocationId})");

            try
            {
                if (!string.IsNullOrEmpty(notification.RelatedEventId))
                {
                    await Shell.Current.GoToAsync($"event-details?id={Uri.EscapeDataString(notification.RelatedEventId)}");
                }
                else if (!string.IsNullOrEmpty(notification.RelatedLocationId))
                {
                    await Shell.Current.GoToAsync($"location-details?id={Uri.EscapeDataString(notification.RelatedLocationId)}");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        notification.Title ?? "Notification",
                        $"{notification.Body}\n\nType: {notification.Type}\nTime: {notification.CreatedAt:g}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotificationsPage: Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Navigation Error", $"Could not open details: {ex.Message}", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load notifications when page appears
            if (_viewModel?.LoadNotificationsCommand.CanExecute(null) ?? false)
            {
                await _viewModel.LoadNotificationsCommand.ExecuteAsync(null);
            }
        }
    }

    /// <summary>
    /// Converter to invert boolean values for visibility bindings.
    /// </summary>
    public class InvertedBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Converter to check if a string is not null or empty.
    /// </summary>
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to get notification type text color based on type.
    /// </summary>
    public class NotificationTypeColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string typeStr)
            {
                return typeStr.Equals("Alert", StringComparison.OrdinalIgnoreCase) ? Colors.White : Colors.Blue;
            }
            return Colors.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to get notification type background color based on type.
    /// </summary>
    public class NotificationTypeBgColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string typeStr)
            {
                return typeStr.Equals("Alert", StringComparison.OrdinalIgnoreCase) 
                    ? Color.FromArgb("#FF6B6B")  // Red for alerts
                    : Color.FromArgb("#4ECDC4"); // Teal for notifications
            }
            return Colors.LightGray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to get notification type icon geometry.
    /// </summary>
    public class NotificationTypeIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string typeStr && typeStr.Equals("Alert", StringComparison.OrdinalIgnoreCase))
            {
                return IconFactory.ToGeometry(IconFactory.AlertData);
            }
            return IconFactory.ToGeometry(IconFactory.InfoData);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
