using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CampusGuide.Services;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.ViewModels.Search
{
    public partial class SearchViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<LocationModel> results = new();

        [ObservableProperty]
        private ObservableCollection<LocationModel> allLocations = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private readonly IFirestoreService _firestoreService;
        private bool _dataLoaded = false;

        public SearchViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
            if (_firestoreService == null)
            {
                Debug.WriteLine("ERROR: IFirestoreService not found in DI container!");
            }
            else
            {
                Debug.WriteLine("SearchViewModel: IFirestoreService obtained successfully");
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
                    Name = "Library", 
                    Type = "Building",
                    Description = "Main campus library", 
                    Building = "Library Block",
                    Coordinates = new Coordinates { Latitude = 5.3971, Longitude = 100.3084 }
                },
                new LocationModel 
                { 
                    Id = "student-center-001",
                    Name = "Student Center", 
                    Type = "Facility",
                    Description = "Central gathering place", 
                    Building = "Center Block",
                    Coordinates = new Coordinates { Latitude = 5.3975, Longitude = 100.3090 }
                },
                new LocationModel 
                { 
                    Id = "cafeteria-001",
                    Name = "Cafeteria", 
                    Type = "Food",
                    Description = "Main campus dining", 
                    Building = "Food Court",
                    Coordinates = new Coordinates { Latitude = 5.3968, Longitude = 100.3080 }
                }
            };

            foreach (var location in testLocations)
            {
                AllLocations.Add(location);
                Results.Add(location);
            }
            Debug.WriteLine($"SearchViewModel: Loaded {testLocations.Count} test locations");
        }

        [RelayCommand]
        public async Task LoadLocations()
        {
            if (_dataLoaded)
                return;

            await LoadLocationsAsync();
        }

        partial void OnSearchQueryChanged(string oldValue, string newValue)
        {
            Debug.WriteLine($"SearchViewModel: SearchQuery changed from '{oldValue}' to '{newValue}'");
            FilterResults(newValue);
        }

        public async Task LoadLocationsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                Debug.WriteLine("SearchViewModel: Loading locations from Firestore...");

                var locations = await _firestoreService.GetAllLocationsAsync();

                if (locations != null && locations.Count > 0)
                {
                    Debug.WriteLine($"SearchViewModel: Loaded {locations.Count} locations");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AllLocations.Clear();
                        Results.Clear();
                        foreach (var location in locations)
                        {
                            AllLocations.Add(location);
                            Results.Add(location);
                        }
                        Debug.WriteLine($"SearchViewModel: UI updated with {Results.Count} locations");
                        _dataLoaded = true;
                    });
                }
                else
                {
                    Debug.WriteLine("SearchViewModel: No locations found in Firestore");
                    ErrorMessage = "No campus locations are available yet.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load locations: {ex.Message}";
                Debug.WriteLine($"SearchViewModel: Error loading locations: {ex.Message}");
                Debug.WriteLine($"SearchViewModel: StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Results.Clear();
                foreach (var location in AllLocations)
                {
                    Results.Add(location);
                }
                Debug.WriteLine($"SearchViewModel: Cleared filter, showing {Results.Count} results");
                return;
            }

            var searchLower = query.ToLowerInvariant();
            var filteredLocations = AllLocations
                .Where(l => l.Name.ToLowerInvariant().Contains(searchLower) ||
                            l.Description.ToLowerInvariant().Contains(searchLower) ||
                            l.Building.ToLowerInvariant().Contains(searchLower) ||
                            l.Type.ToLowerInvariant().Contains(searchLower) ||
                            (l.Tags?.Any(tag => tag.ToLowerInvariant().Contains(searchLower)) ?? false))
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Results.Clear();
                foreach (var location in filteredLocations)
                {
                    Results.Add(location);
                }
                Debug.WriteLine($"SearchViewModel: Filter '{query}' returned {Results.Count} results");
            });
        }
    }
}
