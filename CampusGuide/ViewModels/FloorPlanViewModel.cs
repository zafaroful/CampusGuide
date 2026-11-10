using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CampusGuide.Services;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.ViewModels
{
    public partial class FloorPlanViewModel : ObservableObject
    {
        [ObservableProperty]
        private string selectedBuildingName = string.Empty;

        [ObservableProperty]
        private string selectedFloorLevel = string.Empty;

        [ObservableProperty]
        private ObservableCollection<FloorPlan> availableFloorPlans = new();

        [ObservableProperty]
        private FloorPlan? currentFloorPlan = null;

        [ObservableProperty]
        private ObservableCollection<LocationModel> locationsOnFloor = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool floorPlanAvailable = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private readonly FloorPlanService _floorPlanService;
        private List<LocationModel> _allLocations = new List<LocationModel>();

        public FloorPlanViewModel()
        {
            _floorPlanService = new FloorPlanService();
            Debug.WriteLine("FloorPlanViewModel: Initialized");
        }

        /// <summary>
        /// Load floor plans for a building
        /// </summary>
        public async Task LoadBuildingFloorPlans(string buildingName, List<LocationModel> locations)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SelectedBuildingName = buildingName;
                _allLocations = locations;

                Debug.WriteLine($"FloorPlanViewModel: Loading floor plans for building: {buildingName}");

                var floorPlans = _floorPlanService.GetFloorPlansForBuilding(buildingName, locations);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AvailableFloorPlans.Clear();
                    foreach (var floorPlan in floorPlans)
                    {
                        AvailableFloorPlans.Add(floorPlan);
                    }

                    if (floorPlans.Count > 0)
                    {
                        // Auto-select first floor
                        SelectFloor(floorPlans[0].FloorLevel);
                    }

                    var stats = _floorPlanService.GetStatistics(floorPlans);
                    Debug.WriteLine($"FloorPlanViewModel: Loaded {stats.totalFloorPlans} floor plans ({stats.availableFloorPlans} available) with {stats.totalLocationsOnFloorPlans} locations");
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to load floor plans for this building. Please try again.";
                Debug.WriteLine($"FloorPlanViewModel: Error loading building floor plans: {ex.Message}");
                Debug.WriteLine($"FloorPlanViewModel: StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void SelectFloor(string floorLevel)
        {
            try
            {
                SelectedFloorLevel = floorLevel;

                Debug.WriteLine($"FloorPlanViewModel: Selecting floor: {floorLevel} in building: {SelectedBuildingName}");

                var floorPlan = _floorPlanService.GetFloorPlan(SelectedBuildingName, floorLevel, _allLocations);

                if (floorPlan == null)
                {
                    Debug.WriteLine($"FloorPlanViewModel: Floor plan not found for {SelectedBuildingName} {floorLevel}");
                    CurrentFloorPlan = null;
                    FloorPlanAvailable = false;
                    return;
                }

                CurrentFloorPlan = floorPlan;
                FloorPlanAvailable = floorPlan.IsAvailable;

                // Update locations on floor
                var locations = _floorPlanService.GetLocationsOnFloor(_allLocations, SelectedBuildingName, floorLevel);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationsOnFloor.Clear();
                    foreach (var location in locations)
                    {
                        LocationsOnFloor.Add(location);
                    }

                    Debug.WriteLine($"FloorPlanViewModel: Floor {floorLevel} has {LocationsOnFloor.Count} locations, floor plan available: {FloorPlanAvailable}");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FloorPlanViewModel: Error selecting floor: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the image source for the current floor plan
        /// </summary>
        public ImageSource? GetFloorPlanImage()
        {
            if (CurrentFloorPlan == null || !CurrentFloorPlan.IsAvailable)
                return null;

            try
            {
                // Try to load the image from embedded resources
                return ImageSource.FromFile(CurrentFloorPlan.ImagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FloorPlanViewModel: Error loading floor plan image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get location details for display on floor plan
        /// </summary>
        public (double X, double Y, double Radius)? GetLocationPinPosition(string locationId, int imageWidth, int imageHeight)
        {
            if (CurrentFloorPlan == null)
                return null;

            var position = CurrentFloorPlan.GetLocationPosition(locationId);
            if (position == null)
                return null;

            var pixelX = position.Value.X * imageWidth;
            var pixelY = position.Value.Y * imageHeight;
            var radius = Math.Min(imageWidth, imageHeight) * 0.03; // 3% of smaller dimension

            return (pixelX, pixelY, radius);
        }

        /// <summary>
        /// Find location at tap position on floor plan
        /// </summary>
        public LocationModel? GetLocationAtPosition(double x, double y, int imageWidth, int imageHeight)
        {
            if (CurrentFloorPlan == null)
                return null;

            const double tapRadius = 40; // pixels

            foreach (var locationId in CurrentFloorPlan.LocationIds)
            {
                var position = CurrentFloorPlan.GetLocationPosition(locationId);
                if (position == null)
                    continue;

                var pixelX = position.Value.X * imageWidth;
                var pixelY = position.Value.Y * imageHeight;

                var distance = Math.Sqrt(Math.Pow(x - pixelX, 2) + Math.Pow(y - pixelY, 2));

                if (distance <= tapRadius)
                {
                    var location = _allLocations.FirstOrDefault(l => l.Id == locationId);
                    Debug.WriteLine($"FloorPlanViewModel: Tapped location at ({x}, {y}): {location?.Name}");
                    return location;
                }
            }

            return null;
        }

        /// <summary>
        /// Clear all floor plan data
        /// </summary>
        public void Clear()
        {
            SelectedBuildingName = string.Empty;
            SelectedFloorLevel = string.Empty;
            CurrentFloorPlan = null;
            FloorPlanAvailable = false;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvailableFloorPlans.Clear();
                LocationsOnFloor.Clear();
            });

            Debug.WriteLine("FloorPlanViewModel: Cleared all floor plan data");
        }

        /// <summary>
        /// Get list of floors for UI dropdown
        /// </summary>
        public List<string> GetFloorList()
        {
            return AvailableFloorPlans.Select(fp => fp.FloorLevel).ToList();
        }

        /// <summary>
        /// Get formatted floor display names
        /// </summary>
        public List<(string Level, string DisplayName)> GetFloorDisplayList()
        {
            return AvailableFloorPlans
                .Select(fp => (fp.FloorLevel, fp.DisplayName))
                .ToList();
        }
    }
}
