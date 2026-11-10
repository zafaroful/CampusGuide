using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CampusGuide.Services;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.ViewModels.Map
{
    public partial class CampusMapViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<LocationModel> locations = new();

        [ObservableProperty]
        private ObservableCollection<MapPin> mapPins = new();

        [ObservableProperty]
        private ObservableCollection<ClusterPin> clusterPins = new();

        [ObservableProperty]
        private ObservableCollection<Building> buildings = new();

        [ObservableProperty]
        private Building? selectedBuilding = null;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private readonly IFirestoreService _firestoreService;
        private readonly ClusteringService _clusteringService;
        private readonly BuildingService _buildingService;
        private bool _dataLoaded = false;

        public CampusMapViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
            _clusteringService = new ClusteringService();
            _buildingService = new BuildingService();
            if (_firestoreService == null)
            {
                Debug.WriteLine("ERROR: IFirestoreService not found in DI container!");
            }
            else
            {
                Debug.WriteLine("CampusMapViewModel: IFirestoreService obtained successfully");
            }

            // Load test data immediately
            LoadTestData();
        }

        private void LoadTestData()
        {
            var testLocations = new List<LocationModel>
            {
                new LocationModel 
                { 
                    Id = "lib-001",
                    Name = "Central Library", 
                    Type = "library",
                    Description = "Main campus library with study areas and computer labs",
                    Building = "Academic Block A",
                    Floor = "3",
                    Coordinates = new Coordinates { Latitude = 3.1390, Longitude = 101.6869 },
                    QrCodeId = "campus://location/lib-001",
                    Images = new List<string> { "library.jpg" },
                    Tags = new List<string> { "library", "study", "computers" }
                },
                new LocationModel 
                { 
                    Id = "cafe-001",
                    Name = "Student Cafeteria", 
                    Type = "cafeteria",
                    Description = "Main cafeteria with food options",
                    Building = "Student Center",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 3.1395, Longitude = 101.6875 },
                    QrCodeId = "campus://location/cafe-001",
                    Images = new List<string> { "cafeteria.jpg" },
                    Tags = new List<string> { "food", "cafeteria", "dining" }
                },
                new LocationModel 
                { 
                    Id = "gym-001",
                    Name = "Sports Complex", 
                    Type = "sports",
                    Description = "Multi-purpose sports facility",
                    Building = "Recreation Building",
                    Floor = "B1",
                    Coordinates = new Coordinates { Latitude = 3.1385, Longitude = 101.6860 },
                    QrCodeId = "campus://location/gym-001",
                    Images = new List<string> { "gym.jpg" },
                    Tags = new List<string> { "sports", "gym", "fitness" }
                },
                new LocationModel 
                { 
                    Id = "lab-001",
                    Name = "Computer Science Lab", 
                    Type = "lab",
                    Description = "State-of-the-art computer lab for research",
                    Building = "Engineering Block",
                    Floor = "2",
                    Coordinates = new Coordinates { Latitude = 3.1388, Longitude = 101.6872 },
                    QrCodeId = "campus://location/lab-001",
                    Images = new List<string> { "lab.jpg" },
                    Tags = new List<string> { "lab", "computers", "research" }
                },
                new LocationModel 
                { 
                    Id = "aud-001",
                    Name = "Main Auditorium", 
                    Type = "auditorium",
                    Description = "Large auditorium for lectures and events",
                    Building = "Academic Block",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 3.1392, Longitude = 101.6866 },
                    QrCodeId = "campus://location/aud-001",
                    Images = new List<string> { "auditorium.jpg" },
                    Tags = new List<string> { "auditorium", "lecture", "events" }
                },
                new LocationModel 
                { 
                    Id = "health-001",
                    Name = "Health Center", 
                    Type = "health",
                    Description = "Campus health and medical facilities",
                    Building = "Medical Building",
                    Floor = "1",
                    Coordinates = new Coordinates { Latitude = 3.1398, Longitude = 101.6878 },
                    QrCodeId = "campus://location/health-001",
                    Images = new List<string> { "health.jpg" },
                    Tags = new List<string> { "health", "medical", "clinic" }
                },
                new LocationModel 
                { 
                    Id = "student-center-001",
                    Name = "Student Center",
                    Type = "Facility",
                    Description = "Central gathering place",
                    Building = "Center Block",
                    Coordinates = new Coordinates { Latitude = 3.1393, Longitude = 101.6870 },
                    QrCodeId = "campus://location/student-center-001",
                    Tags = new List<string> { "facility", "student", "gathering" }
                },
                new LocationModel 
                { 
                    Id = "cafeteria-001",
                    Name = "Cafeteria",
                    Type = "Food",
                    Description = "Main campus dining",
                    Building = "Food Court",
                    Coordinates = new Coordinates { Latitude = 3.1396, Longitude = 101.6876 },
                    QrCodeId = "campus://location/cafeteria-001",
                    Tags = new List<string> { "food", "dining" }
                }
            };

            foreach (var location in testLocations)
            {
                Locations.Add(location);
                MapPins.Add(MapPin.FromLocation(location));
            }
            Debug.WriteLine($"CampusMapViewModel: Loaded {testLocations.Count} test locations and map pins");

            // Load buildings and generate clusters
            LoadBuildings();
            RegenerateClusters();

            // Explicitly notify that data has changed so UI refreshes
            OnPropertyChanged(nameof(MapPins));
            OnPropertyChanged(nameof(Buildings));
            _dataLoaded = true;
        }

        [RelayCommand]
        public async Task LoadLocations()
        {
            if (_dataLoaded)
                return;

            await LoadLocationsAsync();
        }

        [RelayCommand]
        public async Task SelectLocation(LocationModel location)
        {
            if (location == null)
                return;

            Debug.WriteLine($"SelectLocation: {location.Name} selected");
            Debug.WriteLine($"SelectLocation: Location ID = '{location.Id}' (length: {location.Id?.Length ?? 0})");

            // Navigate to location details page
            try
            {
                var navUrl = $"location-details?id={location.Id}";
                Debug.WriteLine($"SelectLocation: Navigating to: {navUrl}");
                await Shell.Current.GoToAsync(navUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SelectLocation: Navigation error: {ex.Message}");
                ErrorMessage = $"Could not open location details: {ex.Message}";
            }
        }

        public async Task LoadLocationsAsync()
        {
            try
            {
                Debug.WriteLine("==== LoadLocationsAsync Started ====");
                Debug.WriteLine($"IFirestoreService is: {(_firestoreService == null ? "NULL" : "NOT NULL")}");

                if (_firestoreService == null)
                {
                    Debug.WriteLine("ERROR: Firestore service is NULL, cannot load data");
                    ErrorMessage = "Map data is unavailable. Please restart the app.";
                    return;
                }

                IsLoading = true;
                ErrorMessage = string.Empty;
                Debug.WriteLine("CampusMapViewModel: Loading locations from Firestore...");

                var locations = await _firestoreService.GetAllLocationsAsync();

                if (locations != null && locations.Count > 0)
                {
                    Debug.WriteLine($"CampusMapViewModel: Loaded {locations.Count} locations");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Locations.Clear();
                        MapPins.Clear();
                        foreach (var location in locations)
                        {
                            Locations.Add(location);
                            MapPins.Add(MapPin.FromLocation(location));
                        }
                        Debug.WriteLine($"CampusMapViewModel: Map updated with {Locations.Count} locations and {MapPins.Count} pins");

                        // Explicitly notify that MapPins collection has changed
                        OnPropertyChanged(nameof(MapPins));

                        _dataLoaded = true;
                    });
                }
                else
                {
                    Debug.WriteLine("CampusMapViewModel: No locations found in Firestore");
                    ErrorMessage = "No campus locations are available yet.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load map locations: {ex.Message}";
                Debug.WriteLine($"CampusMapViewModel: Error loading locations: {ex.Message}");
                Debug.WriteLine($"CampusMapViewModel: StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;

                // Generate clusters after loading
                RegenerateClusters();
            }
        }

        private void RegenerateClusters()
        {
            try
            {
                if (MapPins == null || MapPins.Count == 0)
                {
                    Debug.WriteLine("CampusMapViewModel: No map pins available for clustering");
                    return;
                }

                var visiblePins = MapPins.Where(p => p.IsVisible).ToList();
                Debug.WriteLine($"CampusMapViewModel: Clustering {visiblePins.Count} visible pins");

                var clusters = _clusteringService.ClusterPins(visiblePins);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var clusterPinsCollection = ClusterPins;
                    if (clusterPinsCollection != null)
                    {
                        clusterPinsCollection.Clear();
                        foreach (var cluster in clusters)
                        {
                            clusterPinsCollection.Add(cluster);
                        }

                        var stats = _clusteringService.GetStatistics(clusters);
                        Debug.WriteLine($"CampusMapViewModel: Clustering complete - {stats.totalClusters} clusters, {stats.totalPins} pins, avg size: {stats.avgClusterSize:F2}");

                        // Explicitly notify that ClusterPins collection has changed
                        OnPropertyChanged(nameof(ClusterPins));
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CampusMapViewModel: Error generating clusters: {ex.Message}");
                Debug.WriteLine($"CampusMapViewModel: StackTrace: {ex.StackTrace}");
            }
        }

        public void ExpandCluster(ClusterPin cluster)
        {
            if (cluster == null) return;
            _clusteringService.ExpandCluster(cluster);
            Debug.WriteLine($"CampusMapViewModel: Expanded cluster");
        }

        public void CollapseCluster(ClusterPin cluster)
        {
            if (cluster == null) return;
            _clusteringService.CollapseCluster(cluster);
            Debug.WriteLine($"CampusMapViewModel: Collapsed cluster");
        }

        private void LoadBuildings()
        {
            try
            {
                if (Locations == null || Locations.Count == 0)
                {
                    Debug.WriteLine("CampusMapViewModel: No locations available for building extraction");
                    return;
                }

                var buildingsList = _buildingService.ExtractBuildingsFromLocations(Locations.ToList());

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Buildings.Clear();
                    foreach (var building in buildingsList)
                    {
                        Buildings.Add(building);
                    }

                    var stats = _buildingService.GetStatistics(buildingsList);
                    Debug.WriteLine($"CampusMapViewModel: Loaded {stats.totalBuildings} buildings with {stats.totalLocations} locations, avg {stats.avgLocationsPerBuilding:F1} per building");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CampusMapViewModel: Error loading buildings: {ex.Message}");
                Debug.WriteLine($"CampusMapViewModel: StackTrace: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        public void SelectBuilding(Building? building)
        {
            try
            {
                SelectedBuilding = building;

                if (building == null)
                {
                    Debug.WriteLine("CampusMapViewModel: Cleared building filter (showing all buildings)");
                }
                else
                {
                    Debug.WriteLine($"CampusMapViewModel: Selected building '{building.Name}' with {building.LocationCount} locations");
                }

                // Update map pins to show only selected building
                UpdateMapPinsForBuilding();

                // Regenerate clusters with filtered pins
                RegenerateClusters();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CampusMapViewModel: Error selecting building: {ex.Message}");
            }
        }

        private void UpdateMapPinsForBuilding()
        {
            try
            {
                if (SelectedBuilding == null)
                {
                    // Show all pins
                    foreach (var pin in MapPins)
                    {
                        pin.IsVisible = true;
                    }
                    Debug.WriteLine("CampusMapViewModel: Showing all map pins");
                }
                else
                {
                    // Show only pins in selected building
                    foreach (var pin in MapPins)
                    {
                        pin.IsVisible = pin.Building.Equals(SelectedBuilding.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    Debug.WriteLine($"CampusMapViewModel: Filtered {MapPins.Count(p => p.IsVisible)} pins to building '{SelectedBuilding.Name}'");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CampusMapViewModel: Error updating map pins for building: {ex.Message}");
            }
        }

        public List<MapPin> GetVisiblePinsInBuilding()
        {
            if (SelectedBuilding == null)
                return MapPins?.Where(p => p.IsVisible).ToList() ?? new List<MapPin>();

            return MapPins?
                .Where(p => p.IsVisible && p.Building.Equals(SelectedBuilding.Name, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<MapPin>();
        }
    }
}
