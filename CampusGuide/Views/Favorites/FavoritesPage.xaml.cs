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
                Text = "Saved Locations",
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
                Text = "Loading favorites...",
                HorizontalTextAlignment = TextAlignment.Center
            });

            var loadingView = new VerticalStackLayout { loadingStack };
            loadingView.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
            contentGrid.Add(loadingView);

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
                Spacing = 15,
                Padding = 40,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            emptyStack.Add(new Label
            {
                Text = "❤️",
                FontSize = 48,
                HorizontalOptions = LayoutOptions.Center
            });
            emptyStack.Add(new Label
            {
                Text = "No Favorites Yet",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            });
            emptyStack.Add(new Label
            {
                Text = "Save your favorite locations from the map or search to see them here.",
                FontSize = 14,
                TextColor = Colors.Gray,
                HorizontalTextAlignment = TextAlignment.Center
            });

            var emptyView = new VerticalStackLayout { emptyStack };
            emptyView.SetBinding(IsVisibleProperty, new Binding("HasFavorites", source: _viewModel, converter: new InvertedBooleanConverter()));
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

        private VerticalStackLayout CreateFavoriteTemplate()
        {
            var container = new VerticalStackLayout
            {
                Padding = new Thickness(15, 12),
                Spacing = 10
            };
            container.BackgroundColor = Colors.White;

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
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            topRow.Add(nameLabel);

            // Remove button
            var removeButton = new Button
            {
                Text = "✕ Remove",
                Padding = new Thickness(12, 6),
                FontSize = 12,
                BackgroundColor = Color.FromArgb("#FF6B6B"),
                TextColor = Colors.White,
                CornerRadius = 6
            };
            SemanticProperties.SetDescription(removeButton, "Remove from favorites");
            removeButton.SetBinding(Button.CommandProperty, new Binding("RemoveFavoriteCommand", source: _viewModel));
            removeButton.SetBinding(Button.CommandParameterProperty, "Id");
            topRow.Add(removeButton);

            container.Add(topRow);

            // Location ID (for reference)
            var idLabel = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray
            };
            idLabel.SetBinding(Label.TextProperty, new Binding("LocationId", stringFormat: "ID: {0}"));
            container.Add(idLabel);

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

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnFavoriteCardTapped;
            border.GestureRecognizers.Add(tapGesture);

            return new VerticalStackLayout { border };
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
