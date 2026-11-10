using CampusGuide.ViewModels;
using CampusGuide.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.Services;

namespace CampusGuide.Views.Events
{
    public class EventsPage : ContentPage
    {
        private EventsViewModel? _viewModel;

        public EventsPage()
        {
            Title = "Events";
            _viewModel = ServiceHelper.GetService<EventsViewModel>();
            BindingContext = _viewModel;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main layout with refresh capability
            var refreshView = new RefreshView
            {
                Command = _viewModel?.RefreshEventsCommand,
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
                    new RowDefinition(GridLength.Star)   // Content
                },
                Padding = 0
            };

            // Header
            var headerLabel = new Label
            {
                Text = "Upcoming Events",
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
                Text = "Loading events...",
                HorizontalTextAlignment = TextAlignment.Center
            });

            var loadingView = new VerticalStackLayout { loadingStack };
            loadingView.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
            contentGrid.Add(loadingView);

            // CollectionView for events
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.Single,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = 0
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, new Binding("Events", source: _viewModel));
            collectionView.ItemTemplate = new DataTemplate(CreateEventTemplate);
            collectionView.SelectionChanged += OnEventSelectionChanged;

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
                Text = "📅",
                FontSize = 48,
                HorizontalOptions = LayoutOptions.Center
            });
            emptyStack.Add(new Label
            {
                Text = "No Events Scheduled",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            });
            emptyStack.Add(new Label
            {
                Text = "Check back later for upcoming campus events.",
                FontSize = 14,
                TextColor = Colors.Gray,
                HorizontalTextAlignment = TextAlignment.Center
            });

            var emptyView = new VerticalStackLayout { emptyStack };
            emptyView.SetBinding(IsVisibleProperty, new Binding("IsEmpty", source: _viewModel));
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

        private VerticalStackLayout CreateEventTemplate()
        {
            var container = new VerticalStackLayout
            {
                Padding = new Thickness(15, 12),
                Spacing = 8
            };
            container.BackgroundColor = Colors.White;

            // Date badge
            var dateStack = new HorizontalStackLayout
            {
                Spacing = 5,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var monthLabel = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(8, 4)
            };
            monthLabel.SetBinding(Label.TextProperty, new Binding("StartTime", stringFormat: "{0:MMM}"));

            var monthBorder = new Border
            {
                Content = monthLabel,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
                BackgroundColor = Color.FromArgb("#4ECDC4"),
                Padding = 0,
                StrokeThickness = 0
            };
            dateStack.Add(monthBorder);

            var dayLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };
            dayLabel.SetBinding(Label.TextProperty, new Binding("StartTime", stringFormat: "{0:dd}"));
            dateStack.Add(dayLabel);

            var dayOfWeekLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center
            };
            dayOfWeekLabel.SetBinding(Label.TextProperty, new Binding("StartTime", stringFormat: "{0:ddd}"));
            dateStack.Add(dayOfWeekLabel);

            container.Add(dateStack);

            // Title
            var titleLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap
            };
            titleLabel.SetBinding(Label.TextProperty, "Title");
            container.Add(titleLabel);

            // Time range
            var timeLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray
            };
            timeLabel.SetBinding(Label.TextProperty, new Binding("StartTime", stringFormat: "{0:h:mm tt}"));
            container.Add(timeLabel);

            // Description (limited to 2 lines)
            var descriptionLabel = new Label
            {
                FontSize = 13,
                TextColor = Colors.DarkGray,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2
            };
            descriptionLabel.SetBinding(Label.TextProperty, "Description");
            container.Add(descriptionLabel);

            // Location indicator
            var locationStack = new HorizontalStackLayout
            {
                Spacing = 5,
                Margin = new Thickness(0, 8, 0, 0)
            };

            locationStack.Add(new Label
            {
                Text = "📍",
                FontSize = 12
            });

            var locationLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.Blue
            };
            locationLabel.SetBinding(Label.TextProperty, new Binding("LocationId", stringFormat: "{0}"));
            locationStack.Add(locationLabel);

            container.Add(locationStack);

            // Border effect
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

            return new VerticalStackLayout { border };
        }

        private async void OnEventSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not CollectionView collectionView || e.CurrentSelection.FirstOrDefault() is not EventItem eventItem)
                return;

            collectionView.SelectedItem = null;
            await Shell.Current.GoToAsync($"event-details?id={eventItem.Id}");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load events when page appears
            if (_viewModel?.LoadEventsCommand.CanExecute(null) ?? false)
            {
                await _viewModel.LoadEventsCommand.ExecuteAsync(null);
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
