using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
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

            // Soft filled search field with search icon, placeholder and clear icon
            var searchEntry = new Entry
            {
                Placeholder = "Search locations by name, type, building...",
                PlaceholderColor = Color.Parse("#8A8A8A"),
                TextColor = Color.Parse("#333333"),
                FontSize = 16,
                BackgroundColor = Colors.Transparent,
                VerticalOptions = LayoutOptions.Center,
                ReturnType = ReturnType.Search
            };
            searchEntry.SetBinding(Entry.TextProperty,
                new Binding("SearchQuery", mode: BindingMode.TwoWay, source: _viewModel));
            SemanticProperties.SetDescription(searchEntry, "Search campus locations");

            var searchIcon = new Label
            {
                Text = "\U0001F50D",
                FontSize = 16,
                TextColor = Color.Parse("#8A8A8A"),
                VerticalOptions = LayoutOptions.Center
            };

            var clearIcon = new Label
            {
                Text = "\u2715",
                FontSize = 16,
                TextColor = Color.Parse("#00695C"),
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(8, 0, 0, 0)
            };
            SemanticProperties.SetDescription(clearIcon, "Clear search");
            clearIcon.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => _viewModel!.SearchQuery = string.Empty)
            });
            clearIcon.SetBinding(IsVisibleProperty, new Binding("SearchQuery",
                converter: new NotNullOrEmptyConverter(), source: _viewModel));

            var searchGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 10
            };
            searchGrid.Add(searchIcon, 0, 0);
            searchGrid.Add(searchEntry, 1, 0);
            searchGrid.Add(clearIcon, 2, 0);

            var searchField = new Border
            {
                BackgroundColor = Color.Parse("#F1F3F4"),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 24 },
                Padding = new Thickness(16, 10),
                Margin = new Thickness(0, 0, 0, 15),
                Content = searchGrid
            };

            layout.Add(searchField);

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
            emptyLabel.SetBinding(IsVisibleProperty, "ShowEmptyState");
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
            var frame = new Border
            {
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Padding = new Thickness(18, 16),
                Margin = new Thickness(0, 8, 0, 8),
                BackgroundColor = Color.Parse("#FFFFFF"),
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.Parse("#000000")),
                    Offset = new Point(0, 2),
                    Radius = 8,
                    Opacity = 0.12f
                }
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
                RowSpacing = 6
            };

            // Name and Type in a horizontal layout
            var nameTypeStack = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(0, 0, 0, 2)
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
                TextColor = Color.Parse("#00695C"),
                FontAttributes = FontAttributes.Bold
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
            floorLabel.SetBinding(IsVisibleProperty, new Binding("Floor", converter: new NotNullOrEmptyConverter()));
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
