using CampusGuide.ViewModels;
using CampusGuide.Services;
using CampusGuide.Models;
using System.Diagnostics;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Views.Map;

public partial class FloorPlanPage : ContentPage
{
    private FloorPlanViewModel _viewModel = new();
    private List<LocationModel> _currentLocations = new();

    public FloorPlanPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;

        // Subscribe to ViewModel property changes
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Handle location selection from list
        LocationsCollectionView.SelectionChanged += LocationsCollectionView_SelectionChanged;

        // Subscribe to back button - use absolute navigation back to map
        BackButton.Clicked += async (s, e) => 
        {
            try
            {
                await Shell.Current.GoToAsync("///map");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        };

        Debug.WriteLine("FloorPlanPage: Initialized");
    }

    /// <summary>
    /// Handle location selection from list
    /// </summary>
    private async void LocationsCollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        try
        {
            var selectedLocation = e.CurrentSelection[0] as LocationModel;
            if (selectedLocation != null)
            {
                Debug.WriteLine($"FloorPlanPage: Selected location: {selectedLocation.Name}");
                await Shell.Current.GoToAsync($"///location-details?id={selectedLocation.Id}");

                // Clear selection for next use
                LocationsCollectionView.SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FloorPlanPage: Error navigating to location: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle ViewModel property changes
    /// </summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FloorPlanViewModel.CurrentFloorPlan) ||
            e.PropertyName == nameof(FloorPlanViewModel.FloorPlanAvailable) ||
            e.PropertyName == nameof(FloorPlanViewModel.IsLoading))
        {
            UpdateFloorPlanDisplay();
        }
        else if (e.PropertyName == nameof(FloorPlanViewModel.SelectedFloorLevel))
        {
            // Update button styling when floor is selected
            UpdateFloorButtons();
        }
    }

    /// <summary>
    /// Load floor plans for a building
    /// Called when navigating to this page with building data
    /// </summary>
    public async void LoadBuildingFloorPlans(Building building, List<LocationModel> locations)
    {
        try
        {
            Debug.WriteLine($"FloorPlanPage: Loading floor plans for building: {building.Name}");

            _currentLocations = locations;

            // Update header
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BuildingNameLabel.Text = $"Building: {building.Name}";
                FloorCountLabel.Text = $"Floors: {building.Floors.Count}";
            });

            // Load floor plans
            await _viewModel.LoadBuildingFloorPlans(building.Name, locations);

            // Create floor selector buttons
            CreateFloorSelectorButtons();

            // Update visibility based on loading state
            UpdateUIVisibility();

            // Add tap gesture recognizer to floor plan image
            AddFloorPlanInteraction();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FloorPlanPage: Error loading building floor plans: {ex.Message}");
        }
    }

    /// <summary>
    /// Add tap gesture recognizer to floor plan image for location selection
    /// </summary>
    private void AddFloorPlanInteraction()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Remove existing gesture recognizers if any
            var existingGestures = FloorPlanImage.GestureRecognizers.Where(g => g is TapGestureRecognizer).ToList();
            foreach (var gesture in existingGestures)
            {
                FloorPlanImage.GestureRecognizers.Remove(gesture);
            }

            // Add new tap gesture recognizer
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await OnFloorPlanTapped(e);
            };

            FloorPlanImage.GestureRecognizers.Add(tapGesture);
            Debug.WriteLine("FloorPlanPage: Added tap gesture recognizer to floor plan image");
        });
    }

    /// <summary>
    /// Handle floor plan image tapped - not currently fully implemented due to coordinate complexity
    /// </summary>
    private async Task OnFloorPlanTapped(TappedEventArgs e)
    {
        try
        {
            if (_viewModel.CurrentFloorPlan == null || _currentLocations == null)
                return;

            Debug.WriteLine($"FloorPlanPage: Floor plan tapped");

            // In a real implementation, we would:
            // 1. Get the tap coordinates relative to the image
            // 2. Use GetLocationAtPosition() to find nearby locations
            // 3. Navigate to location details

            // For now, show the location list and allow selection from there
            if (_viewModel.LocationsOnFloor.Count > 0 && UnavailableContainer.IsVisible)
            {
                // Locations list is already visible
                Debug.WriteLine($"FloorPlanPage: {_viewModel.LocationsOnFloor.Count} locations available on floor");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FloorPlanPage: Error handling floor plan tap: {ex.Message}");
        }
    }

    /// <summary>
    /// Create floor selector buttons dynamically
    /// </summary>
    private void CreateFloorSelectorButtons()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FloorSelectorLayout.Children.Clear();

            var floors = _viewModel.GetFloorDisplayList();

            foreach (var (level, displayName) in floors)
            {
                var button = new Button
                {
                    Text = displayName,
                    StyleId = level,
                    BackgroundColor = _viewModel.SelectedFloorLevel == level ? Color.FromArgb("#FFD60A") : Colors.White,
                    TextColor = _viewModel.SelectedFloorLevel == level ? Colors.Black : Color.FromArgb("#512BD4"),
                    BorderColor = Color.FromArgb("#512BD4"),
                    BorderWidth = 2,
                    CornerRadius = 5,
                    Padding = new Thickness(8, 4),
                    FontSize = 11,
                    HorizontalOptions = LayoutOptions.Start,
                    IsEnabled = true  // Explicitly enable button
                };

                SemanticProperties.SetDescription(button, $"Show {displayName} floor plan");
                button.Clicked += (s, e) =>
                {
                    Debug.WriteLine($"Floor button clicked: {displayName}");
                    _viewModel.SelectFloorCommand.Execute(level);
                    UpdateFloorButtons();
                    UpdateFloorPlanDisplay();
                };

                FloorSelectorLayout.Children.Add(button);
            }

            Debug.WriteLine($"FloorPlanPage: Created {floors.Count} floor selector buttons");
        });
    }

    /// <summary>
    /// Update floor button styling
    /// </summary>
    private void UpdateFloorButtons()
    {
        foreach (var button in FloorSelectorLayout.Children.OfType<Button>())
        {
            var isSelected = button.StyleId == _viewModel.SelectedFloorLevel;
            button.BackgroundColor = isSelected ? Color.FromArgb("#FFD60A") : Colors.White;
            button.TextColor = isSelected ? Colors.Black : Color.FromArgb("#512BD4");
        }
    }

    /// <summary>
    /// Update floor plan display based on current state
    /// </summary>
    private void UpdateFloorPlanDisplay()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (_viewModel.CurrentFloorPlan == null)
                {
                    UpdateUIVisibility();
                    return;
                }

                // Update footer info
                FloorInfoLabel.Text = _viewModel.CurrentFloorPlan.GetSummary();
                LocationCountLabel.Text = $"Locations: {_viewModel.CurrentFloorPlan.LocationCount}";

                // Try to load floor plan image
                if (_viewModel.FloorPlanAvailable)
                {
                    FloorPlanImage.Source = _viewModel.GetFloorPlanImage();

                    if (FloorPlanImage.Source != null)
                    {
                        // Draw location pins on overlay
                        DrawLocationPins();
                    }
                }

                UpdateUIVisibility();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FloorPlanPage: Error updating floor plan display: {ex.Message}");
                UpdateUIVisibility();
            }
        });
    }

    /// <summary>
    /// Draw location pins on the floor plan overlay
    /// </summary>
    private void DrawLocationPins()
    {
        if (_viewModel.CurrentFloorPlan == null)
            return;

        Debug.WriteLine($"FloorPlanPage: Drawing {_viewModel.CurrentFloorPlan.LocationCount} location pins");

        // Set up simple drawable for pins (if GraphicsView is available)
        // For now, we'll mark this as a placeholder for future enhancement
        // MAUI GraphicsView pin drawing can be complex and depends on IDrawable implementation

        // This would typically draw circles/markers at normalized positions
        // Implementation depends on platform-specific graphics capabilities
    }

    /// <summary>
    /// Update UI element visibility based on floor plan availability
    /// </summary>
    private void UpdateUIVisibility()
    {
        var isAvailable = _viewModel.FloorPlanAvailable && _viewModel.CurrentFloorPlan?.IsAvailable == true;
        var isLoading = _viewModel.IsLoading;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Show/hide loading indicator
            LoadingContainer.IsVisible = isLoading;

            // Show/hide floor plan image
            FloorPlanImage.IsVisible = isAvailable && !isLoading;
            PinOverlay.IsVisible = isAvailable && !isLoading;

            // Show/hide unavailable message
            UnavailableContainer.IsVisible = !isAvailable && !isLoading;

            // Update location list if floor plan unavailable
            if (!isAvailable && !isLoading)
            {
                UpdateLocationsList();
            }

            Debug.WriteLine($"FloorPlanPage: UI visibility - available={isAvailable}, loading={isLoading}");
        });
    }

    /// <summary>
    /// Update the locations list in the unavailable message
    /// </summary>
    private void UpdateLocationsList()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LocationsCollectionView.ItemsSource = null; // Clear first
            LocationsCollectionView.ItemsSource = _viewModel.LocationsOnFloor;
            LocationCountLabel.Text = $"Total: {_viewModel.LocationsOnFloor.Count} location{(_viewModel.LocationsOnFloor.Count != 1 ? "s" : "")}";
            Debug.WriteLine($"FloorPlanPage: Updated locations list with {_viewModel.LocationsOnFloor.Count} items");
        });
    }

    /// <summary>
    /// Back button clicked
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("FloorPlanPage: Appeared");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe from events
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        LocationsCollectionView.SelectionChanged -= LocationsCollectionView_SelectionChanged;

        _viewModel.Clear();
        Debug.WriteLine("FloorPlanPage: Disappeared");
    }
}
