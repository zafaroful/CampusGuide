using Microsoft.Maui.Controls;
using CampusGuide.ViewModels.Search;
using CampusGuide.Models;
using CampusGuide.Services;
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

            var layout = new VerticalStackLayout { Padding = 20 };

            // Search Bar with binding
            var searchBar = new SearchBar 
            { 
                Placeholder = "Search locations..."
            };
            searchBar.SetBinding(SearchBar.TextProperty, 
                new Binding("SearchQuery", mode: BindingMode.TwoWay, source: _viewModel));
            layout.Add(searchBar);

            // Loading indicator
            var loadingLabel = new Label
            {
                Text = "Loading locations...",
                FontSize = 14,
                Opacity = 0.7
            };
            loadingLabel.SetBinding(IsVisibleProperty, "IsLoading");
            layout.Add(loadingLabel);

            // Create and bind CollectionView to results
            var collectionView = new CollectionView 
            { 
                SelectionMode = SelectionMode.Single
            };

            // Bind ItemsSource to ViewModel results
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, 
                new Binding("Results", source: _viewModel));

            // Set ItemTemplate for display
            collectionView.ItemTemplate = new DataTemplate(() => CreateLocationCell());

            layout.Add(collectionView);

            Content = layout;
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
                    new RowDefinition(GridLength.Auto)
                },
                Padding = 10,
                RowSpacing = 5
            };

            var nameLabel = new Label 
            { 
                FontAttributes = FontAttributes.Bold,
                FontSize = 16 
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            grid.Add(nameLabel, 0, 0);

            var descriptionLabel = new Label 
            { 
                FontSize = 14,
                Opacity = 0.7 
            };
            descriptionLabel.SetBinding(Label.TextProperty, "Description");
            Grid.SetRow(descriptionLabel, 1);
            grid.Add(descriptionLabel, 0, 1);

            var buildingLabel = new Label
            {
                FontSize = 12,
                Opacity = 0.6
            };
            buildingLabel.SetBinding(Label.TextProperty, new Binding { Path = "Building", StringFormat = "📍 {0}" });
            Grid.SetRow(buildingLabel, 2);
            grid.Add(buildingLabel, 0, 2);

            return grid;
        }
    }
}
