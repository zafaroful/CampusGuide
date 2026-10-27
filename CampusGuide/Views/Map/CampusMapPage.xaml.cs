using Microsoft.Maui.Controls;
using CampusGuide.ViewModels.Map;
using CampusGuide.Services;
using System.Diagnostics;

namespace CampusGuide.Views.Map
{
    public partial class CampusMapPage : ContentPage
    {
        private CampusMapViewModel? _viewModel;

        public CampusMapPage()
        {
            Debug.WriteLine("==== CampusMapPage Constructor Started ====");

            // Get the ViewModel from DI container (registered as Singleton in MauiProgram)
            _viewModel = ServiceHelper.GetService<CampusMapViewModel>();

            if (_viewModel == null)
            {
                Debug.WriteLine("ERROR: CampusMapViewModel not found in DI container!");
                Debug.WriteLine($"ServiceHelper.ServiceProvider is: {(ServiceHelper.ServiceProvider == null ? "NULL" : "NOT NULL")}");
                _viewModel = new CampusMapViewModel();
            }
            else
            {
                Debug.WriteLine("SUCCESS: CampusMapViewModel obtained from DI");
            }

            BindingContext = _viewModel;
            Debug.WriteLine("==== CampusMapPage Constructor Ended ====");

            var layout = new VerticalStackLayout { Padding = 20 };

            // Title
            layout.Add(new Label 
            { 
                Text = "Campus Map", 
                FontSize = 24, 
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Loading indicator
            var loadingLabel = new Label 
            { 
                Text = "Loading locations...",
                FontSize = 14,
                Opacity = 0.7
            };
            loadingLabel.SetBinding(IsVisibleProperty, "IsLoading");
            layout.Add(loadingLabel);

            // Scroll view for locations
            var scrollView = new ScrollView();

            // Use StackLayout for locations
            var itemsStack = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = 0
            };

            // Bind to Locations property manually
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CampusMapViewModel.Locations))
                    {
                        UpdateLocationCards(itemsStack);
                    }
                };

                // Initial load
                UpdateLocationCards(itemsStack);
            }

            scrollView.Content = itemsStack;
            layout.Add(scrollView);

            Content = layout;
            Title = "Map";

            // Add handler to load data when page appears (won't block UI anymore)
            Appearing += OnPageAppearing;
        }

        private void UpdateLocationCards(VerticalStackLayout stack)
        {
            stack.Clear();

            if (_viewModel?.Locations != null)
            {
                foreach (var location in _viewModel.Locations)
                {
                    var card = CreateLocationCardWithGesture(_viewModel, location);
                    stack.Add(card);
                }
            }
        }

        private void OnPageAppearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("CampusMapPage appeared, loading data...");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.LoadLocationsAsync();
                }
            });
        }

        private View CreateLocationCardWithGesture(CampusMapViewModel? viewModel, Models.Location location)
        {
            var frame = new Frame
            {
                BorderColor = Color.Parse("#E0E0E0"),
                CornerRadius = 10,
                HasShadow = true,
                Padding = 15,
                Margin = new Thickness(0, 5)
            };

            var nameLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                Text = location.Name
            };

            var typeLabel = new Label
            {
                FontSize = 14,
                Opacity = 0.7,
                Text = location.Type
            };

            var descriptionLabel = new Label
            {
                FontSize = 13,
                Opacity = 0.6,
                LineBreakMode = LineBreakMode.TailTruncation,
                Text = location.Description
            };

            var buildingFloorLabel = new Label
            {
                FontSize = 12,
                Opacity = 0.5,
                Text = $"Building: {location.Building}"
            };

            var coordinatesLabel = new Label
            {
                FontSize = 11,
                Opacity = 0.4,
                FontFamily = "Courier",
                Text = $"Lat: {location.Coordinates?.Latitude:F4}"
            };

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 8,
                Padding = 0
            };
            stackLayout.Add(nameLabel);
            stackLayout.Add(typeLabel);
            stackLayout.Add(descriptionLabel);
            stackLayout.Add(buildingFloorLabel);
            stackLayout.Add(coordinatesLabel);

            frame.Content = stackLayout;

            // Add tap recognizer directly
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                try
                {
                    Debug.WriteLine($"=== TAP DETECTED ON LOCATION CARD: {location.Name} ===");
                    Debug.WriteLine($"Location ID: '{location.Id}' (empty={string.IsNullOrEmpty(location.Id)})");
                    Debug.WriteLine($"Location Object: Name={location.Name}, Type={location.Type}");

                    if (location != null && _viewModel != null)
                    {
                        Debug.WriteLine($"Executing SelectLocationCommand for: {location.Name}");
                        await _viewModel.SelectLocationCommand.ExecuteAsync(location);
                    }
                    else
                    {
                        Debug.WriteLine($"Cannot execute: location={location == null}, viewModel={_viewModel == null}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR in tap: {ex.Message}\n{ex.StackTrace}");
                }
            };

            frame.GestureRecognizers.Add(tapGesture);

            return frame;
        }
    }
}
