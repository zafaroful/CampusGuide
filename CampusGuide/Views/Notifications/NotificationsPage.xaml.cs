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
                VerticalOptions = LayoutOptions.Fill
            };

            // Grid for main content
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),  // Header
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

            // Content area (Grid for layering)
            var contentGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Star)
                },
                Padding = 0
            };
            Grid.SetRow(contentGrid, 1);
            mainGrid.Add(contentGrid, 0, 1);

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
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, new Binding("Notifications", source: _viewModel));
            collectionView.ItemTemplate = new DataTemplate(CreateNotificationTemplate);
            collectionView.SelectionChangedCommand = new Command<object>(OnNotificationSelected);

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
            emptyView.SetBinding(IsVisibleProperty, new Binding("HasNotifications", source: _viewModel, converter: new InvertedBooleanConverter()));
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

        private VerticalStackLayout CreateNotificationTemplate()
        {
            var container = new VerticalStackLayout
            {
                Padding = new Thickness(15, 10),
                Spacing = 5,
                Margin = new Thickness(10, 5)
            };

            container.BackgroundColor = Colors.White;

            // Title
            var titleLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            titleLabel.SetBinding(Label.TextProperty, "Title");
            container.Add(titleLabel);

            // Body/Content
            var bodyLabel = new Label
            {
                FontSize = 14,
                TextColor = Colors.Gray,
                LineBreakMode = LineBreakMode.WordWrap
            };
            bodyLabel.SetBinding(Label.TextProperty, "Body");
            container.Add(bodyLabel);

            // Type badge
            var typeLabel = new Label
            {
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };
            typeLabel.SetBinding(Label.TextProperty, "Type");
            container.Add(typeLabel);

            // Date
            var dateLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.DarkGray
            };
            dateLabel.SetBinding(Label.TextProperty, new Binding("CreatedAt", stringFormat: "{0:g}"));
            container.Add(dateLabel);

            // Border effect using a modern Border control
            var border = new Border
            {
                Content = container,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                Padding = 0,
                Margin = new Thickness(10, 8)
            };

            return new VerticalStackLayout { border };
        }

        private void OnNotificationSelected(object? obj)
        {
            if (obj is NotificationItem notification)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Show detailed view for the notification
                    await DisplayAlertAsync(
                        notification.Title ?? "Notification",
                        $"{notification.Body}\n\nType: {notification.Type}\nTime: {notification.CreatedAt:g}",
                        "OK"
                    );
                });
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
}
