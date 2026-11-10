using Microsoft.Maui.Controls;
using CampusGuide.ViewModels.Search;
using CampusGuide.Models;
using CampusGuide.Services;
using CampusGuide.Converters;
using LocationModel = CampusGuide.Models.Location;
using System.Diagnostics;

namespace CampusGuide.Views.Search
{
    public partial class SearchPage : ContentPage
    {
        private SearchViewModel? _viewModel;

        public SearchPage()
        {
            _viewModel = ServiceHelper.GetService<SearchViewModel>();
            if (_viewModel == null)
            {
                Debug.WriteLine("ERROR: SearchViewModel not found in DI container!");
                _viewModel = new SearchViewModel();
            }
            BindingContext = _viewModel;

            var layout = new VerticalStackLayout { Spacing = 15 };

            // Search Bar with binding and styling
            var searchBar = new SearchBar 
            { 
                Placeholder = "Search locations by name, type, building...",
                FontSize = 16,
                CancelButtonColor = Color.Parse("#7B61FF"),
                PlaceholderColor = Color.Parse("#999999"),
                TextColor = Color.Parse("#333333"),
                BackgroundColor = Color.Parse("#F5F5F5"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            searchBar.SetBinding(SearchBar.TextProperty, 
                new Binding("SearchQuery", mode: BindingMode.TwoWay, source: _viewModel));

            // Log search query changes
            searchBar.SearchButtonPressed += (sender, args) =>
            {
                Debug.WriteLine($"SearchPage: Search button pressed with query: '{searchBar.Text}'");
            };

            SemanticProperties.SetDescription(searchBar, "Search campus locations");

            layout.Add(searchBar);

            // Loading indicator
            var loadingLabel = new Label
            {
                Text = "Loading locations...",
                FontSize = 14,
                Opacity = 0.7,
                HorizontalOptions = LayoutOptions.Center
            };
            loadingLabel.SetBinding(IsVisibleProperty, "IsLoading");
            layout.Add(loadingLabel);

            var errorLabel = new Label
            {
                FontSize = 14,
                TextColor = Color.Parse("#FF6B6B"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            errorLabel.SetBinding(Label.TextProperty, "ErrorMessage");
            errorLabel.SetBinding(IsVisibleProperty, "HasError");
            layout.Add(errorLabel);

            // Empty state message
            var emptyLabel = new Label
            {
                Text = "No results found. Try a different search.",
                FontSize = 14,
                Opacity = 0.6,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            var emptyBinding = new Binding("Results.Count", 
                converter: new NegativeConverter(), 
                source: _viewModel);
            emptyLabel.SetBinding(IsVisibleProperty, emptyBinding);
            layout.Add(emptyLabel);

            // Create and bind CollectionView to results
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.None
            };
            SemanticProperties.SetDescription(collectionView, "Search results");

            // Bind ItemsSource to ViewModel results
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, 
                new Binding("Results", source: _viewModel));

            // Set ItemTemplate for display
            collectionView.ItemTemplate = new DataTemplate(() => CreateLocationCell());

            collectionView.Header = layout;
            collectionView.Margin = new Thickness(20, 20, 20, 0);
            Content = collectionView;
            Title = "Search";

            // Add handler to load data when page appears (won't block UI anymore)
            Appearing += OnPageAppearing;
        }

        private void OnPageAppearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("SearchPage appeared, loading data...");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.LoadLocationsAsync();
                }
            });
        }

        private static View CreateLocationCell()
        {
            var frame = new Frame
            {
                BorderColor = Color.Parse("#D0D0D0"),
                CornerRadius = 10,
                Padding = 15,
                Margin = new Thickness(0, 5, 0, 5),
                Background = Color.Parse("#FFFFFF"),
                HasShadow = true
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnLocationCardTapped;
            frame.GestureRecognizers.Add(tapGesture);

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star)
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                RowSpacing = 8
            };

            // Name and Type in a horizontal layout
            var nameTypeStack = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var nameLabel = new Label 
            { 
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.Parse("#333333")
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            nameTypeStack.Add(nameLabel);

            var typeLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.Parse("#7B61FF"),
                FontAttributes = FontAttributes.Italic
            };
            typeLabel.SetBinding(Label.TextProperty, "Type");
            nameTypeStack.Add(typeLabel);

            grid.Add(nameTypeStack, 0, 0);

            // Description
            var descriptionLabel = new Label 
            { 
                FontSize = 13,
                Opacity = 0.75,
                TextColor = Color.Parse("#555555"),
                LineBreakMode = LineBreakMode.WordWrap
            };
            descriptionLabel.SetBinding(Label.TextProperty, "Description");
            Grid.SetRow(descriptionLabel, 1);
            grid.Add(descriptionLabel, 0, 1);

            // Building
            var buildingLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.Parse("#666666")
            };
            buildingLabel.SetBinding(Label.TextProperty, new Binding { Path = "Building", StringFormat = "📍 Building: {0}" });
            Grid.SetRow(buildingLabel, 2);
            grid.Add(buildingLabel, 0, 2);

            // Floor (if available)
            var floorLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.Parse("#666666")
            };
            floorLabel.SetBinding(Label.TextProperty, new Binding { Path = "Floor", StringFormat = "🏢 Floor: {0}" });
            Grid.SetRow(floorLabel, 3);
            grid.Add(floorLabel, 0, 3);

            frame.Content = grid;
            return frame;
        }

        private static async void OnLocationCardTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: LocationModel location })
                return;

            Debug.WriteLine($"SearchPage: Location tapped: {location.Name} (ID: {location.Id})");

            try
            {
                await Shell.Current.GoToAsync($"location-details?id={Uri.EscapeDataString(location.Id)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchPage: Navigation error: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Navigation Error", $"Could not open location details: {ex.Message}", "OK");
            }
        }
    }
}
