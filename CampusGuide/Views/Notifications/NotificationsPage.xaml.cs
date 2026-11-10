using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.ViewModels;
using CampusGuide.Models;
using CampusGuide.Services;

namespace CampusGuide.Views.Notifications
{
    public partial class NotificationsPage : ContentPage
    {
        private NotificationsViewModel? _viewModel;

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
                RefreshColor = Colors.Blue
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
            var headerLabel = new Label
            {
                Text = "Notifications & Alerts",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(20, 15, 20, 10)
            };
            mainGrid.Add(headerLabel, 0, 0);

            // Filter buttons
            var filterStack = new HorizontalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20, 0, 20, 10),
                HorizontalOptions = LayoutOptions.Start
            };

            var allButton = new Button
            {
                Text = "All",
                Padding = new Thickness(16, 8),
                FontSize = 12,
                CornerRadius = 20,
                BackgroundColor = Color.FromArgb("#4ECDC4"),
                TextColor = Colors.White
            };
            SemanticProperties.SetDescription(allButton, "Show all notifications");
            allButton.Clicked += (s, e) => FilterNotifications(null);
            filterStack.Add(allButton);

            var alertsButton = new Button
            {
                Text = "⚠️ Alerts",
                Padding = new Thickness(16, 8),
                FontSize = 12,
                CornerRadius = 20,
                BackgroundColor = Colors.LightGray,
                TextColor = Colors.Black
            };
            SemanticProperties.SetDescription(alertsButton, "Show alerts only");
            alertsButton.Clicked += (s, e) => FilterNotifications("Alert");
            filterStack.Add(alertsButton);

            var notificationsButton = new Button
            {
                Text = "ℹ️ Notifications",
                Padding = new Thickness(16, 8),
                FontSize = 12,
                CornerRadius = 20,
                BackgroundColor = Colors.LightGray,
                TextColor = Colors.Black
            };
            SemanticProperties.SetDescription(notificationsButton, "Show notifications only");
            notificationsButton.Clicked += (s, e) => FilterNotifications("Notification");
            filterStack.Add(notificationsButton);

            Grid.SetRow(filterStack, 1);
            mainGrid.Add(filterStack, 0, 1);

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

            // Loading indicator
            var loadingStack = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            loadingStack.Add(new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                Color = Colors.Blue,
                Scale = 1.5
            });
            loadingStack.Add(new Label
            {
                Text = "Loading notifications...",
                HorizontalTextAlignment = TextAlignment.Center
            });

            var loadingView = new VerticalStackLayout { loadingStack };
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

            // Empty state
            var emptyStack = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            emptyStack.Add(new Label
            {
                Text = "📭",
                FontSize = 48,
                HorizontalOptions = LayoutOptions.Center
            });
            emptyStack.Add(new Label
            {
                Text = "No Notifications",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            });
            emptyStack.Add(new Label
            {
                Text = "You're all caught up! Check back later for new alerts and reminders.",
                FontSize = 14,
                TextColor = Colors.Gray,
                HorizontalTextAlignment = TextAlignment.Center
            });

            var emptyView = new VerticalStackLayout { emptyStack };
            emptyView.SetBinding(IsVisibleProperty, new Binding("HasFilteredNotifications", source: _viewModel, converter: new InvertedBooleanConverter()));
            contentGrid.Add(emptyView);

            // Error message
            var errorStack = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 10
            };
            var errorLabel = new Label
            {
                TextColor = Colors.Red,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap
            };
            errorLabel.SetBinding(Label.TextProperty, new Binding("ErrorMessage", source: _viewModel));
            errorStack.Add(errorLabel);

            var dismissErrorButton = new Button
            {
                Text = "Dismiss",
                Margin = new Thickness(0, 5, 0, 0)
            };
            SemanticProperties.SetDescription(dismissErrorButton, "Dismiss error message");
            dismissErrorButton.Clicked += (s, e) =>
            {
                if (_viewModel != null)
                    _viewModel.ErrorMessage = string.Empty;
            };
            errorStack.Add(dismissErrorButton);

            var errorView = new VerticalStackLayout { errorStack };
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
            container.BackgroundColor = Colors.White;

            // Add header
            container.Add(headerStack);

            // Body/Content with better formatting
            var bodyLabel = new Label
            {
                FontSize = 14,
                TextColor = Colors.DarkGray,
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
                TextColor = Colors.Gray,
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
            var indicatorLabel = new Label
            {
                FontSize = 14,
                HorizontalOptions = LayoutOptions.End
            };
            indicatorLabel.SetBinding(Label.TextProperty, new Binding("Type", converter: new NotificationTypeIconConverter()));
            footerStack.Add(indicatorLabel);

            container.Add(footerStack);

            // Border effect using a modern Border control with drop shadow effect
            var border = new Border
            {
                Content = container,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                Padding = 0,
                Margin = new Thickness(12, 8),
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
    /// Converter to get notification type icon emoji.
    /// </summary>
    public class NotificationTypeIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string typeStr)
            {
                return typeStr.Equals("Alert", StringComparison.OrdinalIgnoreCase) ? "⚠️" : "ℹ️";
            }
            return "📌";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
