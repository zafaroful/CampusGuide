using CampusGuide.Models;
using CampusGuide.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.Services;

namespace CampusGuide.Views.Favorites
{
    public class FavoritesPage : ContentPage
    {
        private FavoritesViewModel? _viewModel;

        public FavoritesPage()
        {
            Title = "Favorites";
            _viewModel = ServiceHelper.GetService<FavoritesViewModel>();
            BindingContext = _viewModel;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main layout with refresh capability
            var refreshView = new RefreshView
            {
                Command = _viewModel?.RefreshFavoritesCommand,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                RefreshColor = Themed("#00695C", "#4DB6AC")
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
                FontSize = 14,
                TextColor = Colors.Gray,
                Margin = new Thickness(16, 12, 16, 8)
            };
            headerLabel.SetBinding(Label.TextProperty, new Binding(nameof(FavoritesViewModel.CountText), source: _viewModel));
            headerLabel.SetBinding(IsVisibleProperty, new Binding(nameof(FavoritesViewModel.HasFavorites), source: _viewModel));
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
                Padding = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            loadingStack.Add(new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                Color = Themed("#00695C", "#4DB6AC"),
                Scale = 1.5
            });
            loadingStack.Add(new Label
            {
                Text = "Loading favorites...",
                HorizontalTextAlignment = TextAlignment.Center
            });

            loadingStack.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
            contentGrid.Add(loadingStack);

            // CollectionView for favorites
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.Single,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = 0
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, new Binding("Favorites", source: _viewModel));
            collectionView.ItemTemplate = new DataTemplate(CreateFavoriteTemplate);

            contentGrid.Add(collectionView);

            // Empty state
            var emptyStack = new VerticalStackLayout
            {
                Spacing = 12,
                Padding = new Thickness(16, 40),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var emptyIconCircle = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 40 },
                Stroke = Colors.Transparent,
                BackgroundColor = Themed("#FFEBEE", "#4A2A2A"),
                WidthRequest = 80,
                HeightRequest = 80,
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = "\u2764\uFE0F",
                    FontSize = 36,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            emptyStack.Add(emptyIconCircle);

            emptyStack.Add(new Label
            {
                Text = "No Favorites Yet",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Themed("#333333", "#F1F1F1"),
                Margin = new Thickness(0, 8, 0, 0)
            });
            emptyStack.Add(new Label
            {
                Text = "Save your favorite locations from the map or search to see them here.",
                FontSize = 14,
                TextColor = Themed("#6B6B6B", "#B0B3B8"),
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                MaximumWidthRequest = 300
            });

            var browseButton = new Button
            {
                Text = "Browse locations",
                BackgroundColor = Themed("#00695C", "#4DB6AC"),
                TextColor = Themed("#FFFFFF", "#121212"),
                CornerRadius = 12,
                Padding = new Thickness(24, 10),
                Margin = new Thickness(0, 16, 0, 0),
                HorizontalOptions = LayoutOptions.Center
            };
            SemanticProperties.SetDescription(browseButton, "Browse campus locations");
            browseButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("//search");
            emptyStack.Add(browseButton);

            emptyStack.SetBinding(IsVisibleProperty, new Binding("IsEmpty", source: _viewModel));
            contentGrid.Add(emptyStack);

            // Error message
            var errorStack = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 10
            };
            var errorLabel = new Label
            {
                TextColor = Themed("#C62828", "#EF9A9A"),
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

            errorStack.SetBinding(IsVisibleProperty, new Binding("ErrorMessage", source: _viewModel, converter: new IsNotNullOrEmptyConverter()));
            contentGrid.Add(errorStack);

            refreshView.Content = mainGrid;
            Content = refreshView;
        }

        private static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

        private static Color Themed(string light, string dark) => Color.Parse(IsDark ? dark : light);

        private View CreateFavoriteTemplate()
        {
            var container = new VerticalStackLayout
            {
                Padding = new Thickness(18, 16),
                Spacing = 8
            };

            // Top row with name and remove button
            var topRow = new HorizontalStackLayout
            {
                Spacing = 10,
                VerticalOptions = LayoutOptions.Start
            };

            // Location name
            var nameLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                TextColor = Themed("#333333", "#F1F1F1")
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            topRow.Add(nameLabel);

            // Remove button
            var removeButton = new Button
            {
                Text = "✕ Remove",
                Padding = new Thickness(12, 6),
                FontSize = 12,
                BackgroundColor = Themed("#FFEBEE", "#4A2A2A"),
                TextColor = Themed("#D32F2F", "#FF8A80"),
                CornerRadius = 12
            };
            SemanticProperties.SetDescription(removeButton, "Remove from favorites");
            removeButton.SetBinding(Button.CommandProperty, new Binding("RemoveFavoriteCommand", source: _viewModel));
            removeButton.SetBinding(Button.CommandParameterProperty, "Id");
            topRow.Add(removeButton);

            container.Add(topRow);

            var descriptionLabel = new Label
            {
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
                TextColor = Themed("#555555", "#D0D0D0")
            };
            descriptionLabel.SetBinding(Label.TextProperty, new Binding("Description"));
            descriptionLabel.SetBinding(IsVisibleProperty, new Binding("Description", converter: new IsNotNullOrEmptyConverter()));
            container.Add(descriptionLabel);

            var metaLabel = new Label
            {
                FontSize = 13,
                TextColor = Themed("#6B6B6B", "#B0B3B8")
            };
            metaLabel.SetBinding(Label.TextProperty, new Binding("MetaText"));
            metaLabel.SetBinding(IsVisibleProperty, new Binding("MetaText", converter: new IsNotNullOrEmptyConverter()));
            container.Add(metaLabel);

            var idLabel = new Label
            {
                FontSize = 12,
                TextColor = Themed("#6B6B6B", "#B0B3B8")
            };
            idLabel.SetBinding(Label.TextProperty, new Binding("LocationId", stringFormat: "ID: {0}"));
            container.Add(idLabel);

            var card = new Border
            {
                Content = container,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Padding = new Thickness(0),
                Margin = new Thickness(16, 8, 16, 8),
                BackgroundColor = Themed("#FFFFFF", "#1E1F22"),
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Offset = new Point(0, 2),
                    Opacity = 0.12f,
                    Radius = 8
                }
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnFavoriteCardTapped;
            card.GestureRecognizers.Add(tapGesture);

            var removeSwipeItem = new SwipeItem
            {
                Text = "Remove",
                BackgroundColor = Themed("#D32F2F", "#B71C1C")
            };
            removeSwipeItem.SetBinding(SwipeItem.CommandProperty, new Binding("RemoveFavoriteCommand", source: _viewModel));
            removeSwipeItem.SetBinding(SwipeItem.CommandParameterProperty, "Id");

            var swipeView = new SwipeView
            {
                Content = card,
                RightItems = new SwipeItems { removeSwipeItem }
            };

            return swipeView;
        }

        private static async void OnFavoriteCardTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: FavoriteItem favorite })
                return;

            var locationId = string.IsNullOrEmpty(favorite.LocationId) ? favorite.Id : favorite.LocationId;
            System.Diagnostics.Debug.WriteLine($"FavoritesPage: Favorite tapped: {favorite.Name} (ID: {locationId})");

            try
            {
                await Shell.Current.GoToAsync($"location-details?id={Uri.EscapeDataString(locationId)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FavoritesPage: Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Navigation Error", $"Could not open location details: {ex.Message}", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load favorites when page appears
            if (_viewModel?.LoadFavoritesCommand.CanExecute(null) ?? false)
            {
                await _viewModel.LoadFavoritesCommand.ExecuteAsync(null);
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
