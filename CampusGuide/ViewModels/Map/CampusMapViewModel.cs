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
        private bool isLoading = false;

        private readonly IFirestoreService _firestoreService;
        private bool _dataLoaded = false;

        public CampusMapViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
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
                Locations.Add(location);
            }
            Debug.WriteLine($"CampusMapViewModel: Loaded {testLocations.Count} test locations");
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
                    return;
                }

                IsLoading = true;
                Debug.WriteLine("CampusMapViewModel: Loading locations from Firestore...");

                var locations = await _firestoreService.GetAllLocationsAsync();

                if (locations != null && locations.Count > 0)
                {
                    Debug.WriteLine($"CampusMapViewModel: Loaded {locations.Count} locations");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Locations.Clear();
                        foreach (var location in locations)
                        {
                            Locations.Add(location);
                        }
                        Debug.WriteLine($"CampusMapViewModel: Map updated with {Locations.Count} locations");
                        _dataLoaded = true;
                    });
                }
                else
                {
                    Debug.WriteLine("CampusMapViewModel: No locations found in Firestore");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CampusMapViewModel: Error loading locations: {ex.Message}");
                Debug.WriteLine($"CampusMapViewModel: StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
