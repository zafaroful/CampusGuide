using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using CampusGuide.ViewModels.Map;
using CampusGuide.Services;
using CampusGuide.Models;
using CampusGuide.Views.Controls;
using System.Diagnostics;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Views.Map
{
    [QueryProperty(nameof(FocusLocationId), "focusId")]
    public partial class CampusMapPage : ContentPage
    {
        private CampusMapViewModel? _viewModel;
        private Microsoft.Maui.Controls.Maps.Map? _map;
        private string _currentFilter = "All";
        private Entry? _searchEntry;
        private string? _focusLocationId;
        private readonly List<(string Type, CategoryChip Chip)> _filterChips = new();

        public string? FocusLocationId
        {
            get => _focusLocationId;
            set => _focusLocationId = value is null ? null : Uri.UnescapeDataString(value);
        }

        public CampusMapPage()
        {
            Debug.WriteLine("==== CampusMapPage (Map View) Constructor Started ====");

            try
            {
                _viewModel = ServiceHelper.GetService<CampusMapViewModel>();

                if (_viewModel == null)
                {
                    Debug.WriteLine("ERROR: CampusMapViewModel not found in DI container!");
                    _viewModel = new CampusMapViewModel();
                }
                else
                {
                    Debug.WriteLine("SUCCESS: CampusMapViewModel obtained from DI");
                }

                BindingContext = _viewModel;
                Title = "Campus Map";

                // Subscribe to ViewModel property changes to know when data is loaded
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }

                InitializeMap();
                Debug.WriteLine("==== CampusMapPage Constructor Completed Successfully ====");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in CampusMapPage Constructor: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When MapPins or ClusterPins changes, refresh the map display
            if (e.PropertyName == nameof(CampusMapViewModel.MapPins) || e.PropertyName == nameof(CampusMapViewModel.ClusterPins))
            {
                Debug.WriteLine($"*** VIEWMODEL MAPPINS CHANGED *** Refreshing map pins");
                MainThread.BeginInvokeOnMainThread(() => RefreshMapPins());
            }
        }

        private void InitializeMap()
        {
            try
            {
                Debug.WriteLine("InitializeMap() started");
                // Create main layout with Grid for proper layout zones
                var mainLayout = new Grid
                {
                    Padding = 0,
                    RowSpacing = 0,
                    ColumnSpacing = 0,
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },     // Header
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Map (fill space)
                        new RowDefinition { Height = GridLength.Auto },     // Controls
                        new RowDefinition { Height = GridLength.Auto }      // Info label
                    }
                };

                Debug.WriteLine("Grid layout created");
                // Header with title
            var headerLayout = new VerticalStackLayout
            {
                Padding = 15,
                BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#00796B") : Color.FromArgb("#00695C"),
                Spacing = 10
            };


            var mapErrorLabel = new Label
            {
                FontSize = 14,
                TextColor = Color.Parse("#FFB3B3"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                InputTransparent = true  // Don't intercept touches
            };
            mapErrorLabel.SetBinding(Label.TextProperty, "ErrorMessage");
            mapErrorLabel.SetBinding(IsVisibleProperty, "HasError");
            headerLayout.Add(mapErrorLabel);

            // Search entry
            _searchEntry = new Entry
            {
                Placeholder = "Search locations...",
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                PlaceholderColor = Colors.Gray,
                Margin = new Thickness(0, 5, 0, 0)
            };
            SemanticProperties.SetDescription(_searchEntry, "Search campus locations");
            _searchEntry.TextChanged += OnSearchTextChanged;
            headerLayout.Add(_searchEntry);

            // Filter buttons layout
            var filterButtonsLayout = new VerticalStackLayout
            {
                Spacing = 8,
                InputTransparent = false  // Allow buttons to receive input
            };

            var row1 = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Fill
            };

            var allButton = CreateFilterButton("All");
            var libraryButton = CreateFilterButton("library");
            var cafeteriaButton = CreateFilterButton("cafeteria");
            var sportsButton = CreateFilterButton("sports");

            row1.Add(allButton);
            row1.Add(libraryButton);
            row1.Add(cafeteriaButton);
            row1.Add(sportsButton);

            var row2 = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Fill
            };

            var labButton = CreateFilterButton("lab");
            var auditoriumButton = CreateFilterButton("auditorium");
            var healthButton = CreateFilterButton("health");

            row2.Add(labButton);
            row2.Add(auditoriumButton);
            row2.Add(healthButton);

            filterButtonsLayout.Add(row1);
            filterButtonsLayout.Add(row2);

            headerLayout.Add(new Label
            {
                Text = "Filter by Type:",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                Margin = new Thickness(0, 5, 0, 0),
                InputTransparent = true  // Don't intercept touches
            });
            headerLayout.Add(filterButtonsLayout);

            // Collapsible details panel (building filter and legends)
            var detailsLayout = new VerticalStackLayout
            {
                Spacing = 8,
                Margin = new Thickness(0, 10, 0, 0),
                IsVisible = false
            };

            var toggleDetailsButton = new Button
            {
                Text = "Show building filter & legend ▼",
                BackgroundColor = Colors.Transparent,
                BorderColor = (Color)Application.Current!.Resources["HeaderText"],
                BorderWidth = 1,
                TextColor = (Color)Application.Current!.Resources["HeaderText"],
                FontSize = 12,
                CornerRadius = 8,
                Margin = new Thickness(0, 10, 0, 0),
                MinimumHeightRequest = 44
            };
            SemanticProperties.SetDescription(toggleDetailsButton, "Show or hide building filter and legend");
            toggleDetailsButton.Clicked += (s, e) =>
            {
                detailsLayout.IsVisible = !detailsLayout.IsVisible;
                toggleDetailsButton.Text = detailsLayout.IsVisible
                    ? "Hide building filter & legend ▲"
                    : "Show building filter & legend ▼";
            };
            headerLayout.Add(toggleDetailsButton);

            detailsLayout.Add(new Label
            {
                Text = "Filter by Building:",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["HeaderText"],
                InputTransparent = true  // Don't intercept touches
            });

            var buildingScrollLayout = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Fill
            };

            var buildingScrollView = new ScrollView
            {
                Content = buildingScrollLayout,
                Orientation = ScrollOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            };

            // Add "All Buildings" button
            var allBuildingsButton = CreateBuildingButton(null);
            buildingScrollLayout.Add(allBuildingsButton);

            // Bind buildings to buttons (will be updated when buildings load)
            if (_viewModel?.Buildings != null)
            {
                foreach (var building in _viewModel.Buildings)
                {
                    var buildingButton = CreateBuildingButton(building);
                    buildingScrollLayout.Add(buildingButton);
                }
            }

            detailsLayout.Add(buildingScrollView);
            headerLayout.Add(detailsLayout);

            // Pin color legend
            var legendLabel = new Label
            {
                Text = "Pin categories: 🔵 Library • 🟠 Cafeteria • 🟢 Sports • 🟣 Lab • 🔴 Auditorium • 🌸 Health",
                FontSize = 12,
                TextColor = (Color)Application.Current!.Resources["HeaderText"],
                Margin = new Thickness(0, 8, 0, 0),
                InputTransparent = true  // Don't intercept touches
            };
            detailsLayout.Add(legendLabel);

            // Building legend
            if (_viewModel?.Buildings != null && _viewModel.Buildings.Count > 0)
            {
                var buildingLegendLabel = new Label
                {
                    FontSize = 12,
                    TextColor = (Color)Application.Current!.Resources["HeaderText"],
                    Margin = new Thickness(0, 8, 0, 0),
                    LineBreakMode = LineBreakMode.WordWrap,
                    InputTransparent = true  // Don't intercept touches
                };

                // Create building legend text
                var buildingLegendText = "Buildings: " + string.Join(" • ", _viewModel.Buildings.Select(b => $"{b.Emoji} {b.Name}"));
                buildingLegendLabel.Text = buildingLegendText;

                detailsLayout.Add(buildingLegendLabel);
            }

            mainLayout.Add(headerLayout, 0, 0);

            // Map view
            _map = new Microsoft.Maui.Controls.Maps.Map
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Set initial map region (center on campus - average of all coordinates)
            var centerLocation = new Microsoft.Maui.Devices.Sensors.Location(3.1391, 101.6871); // Center of campus
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                centerLocation,
                Distance.FromMiles(0.5)
            ));

            // Pins will be loaded when ViewModel data is ready
            Debug.WriteLine("Map initialized - pins will load when data is available");

            mainLayout.Add(_map, 0, 1);
            RefreshMapPins();

            // Map controls toolbar
            var controlsLayout = new HorizontalStackLayout
            {
                Padding = 10,
                Spacing = 10,
                BackgroundColor = (Color)Application.Current!.Resources["Gray100"],
                HorizontalOptions = LayoutOptions.Fill
            };

            var centerButton = new Button
            {
                Text = "🎯 Center",
                BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#00796B") : Color.FromArgb("#00695C"),
                TextColor = Colors.White,
                CornerRadius = 5,
                Padding = 10,
                HorizontalOptions = LayoutOptions.Fill,
                FontSize = 12
            };
            SemanticProperties.SetDescription(centerButton, "Centers the map on campus");
            centerButton.Clicked += OnCenterMapClicked;
            controlsLayout.Add(centerButton);

            var zoomInButton = new Button
            {
                Text = "🔍+",
                BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#00796B") : Color.FromArgb("#00695C"),
                TextColor = Colors.White,
                CornerRadius = 5,
                Padding = 10,
                FontSize = 12
            };
            SemanticProperties.SetDescription(zoomInButton, "Zoom in");
            zoomInButton.Clicked += OnZoomInClicked;
            controlsLayout.Add(zoomInButton);

            var zoomOutButton = new Button
            {
                Text = "🔍−",
                BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#00796B") : Color.FromArgb("#00695C"),
                TextColor = Colors.White,
                CornerRadius = 5,
                Padding = 10,
                FontSize = 12
            };
            SemanticProperties.SetDescription(zoomOutButton, "Zoom out");
            zoomOutButton.Clicked += OnZoomOutClicked;
            controlsLayout.Add(zoomOutButton);

            var floorPlansButton = new Button
            {
                Text = "📐 Floor Plans",
                BackgroundColor = Color.Parse("#512BD4"),
                TextColor = Colors.White,
                CornerRadius = 5,
                Padding = 10,
                FontSize = 12
            };
            SemanticProperties.SetDescription(floorPlansButton, "Opens floor plans");
            floorPlansButton.Clicked += OnFloorPlansClicked;
            controlsLayout.Add(floorPlansButton);

            mainLayout.Add(controlsLayout, 0, 2);

            // Info label
            var infoLabel = new Label
            {
                Text = "Tap any pin to view location details",
                FontSize = 12,
                TextColor = Colors.Gray,
                Margin = new Thickness(15, 10),
                HorizontalTextAlignment = TextAlignment.Center
            };
            mainLayout.Add(infoLabel, 0, 3);

            Content = mainLayout;
                Debug.WriteLine("InitializeMap() completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in InitializeMap: {ex.GetType().Name}");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                // Fall back to a simple layout on error
                try
                {
                    Content = new StackLayout
                    {
                        Padding = 20,
                        Spacing = 10,
                        Children =
                        {
                            new Label { Text = "Map Loading Error", FontSize = 18, FontAttributes = FontAttributes.Bold },
                            new Label { Text = ex.Message, FontSize = 12, TextColor = Colors.Red },
                            new Label { Text = "Please restart the app", FontSize = 12 }
                        }
                    };
                }
                catch { /* Ignore fallback errors */ }
                throw;
            }
        }

        private void AddPinToMap(MapPin mapPin)
        {
            if (_map == null) return;

            // Create more descriptive address for the pin
            var colorEmoji = GetColorEmojiForType(mapPin.Type);
            var pinAddress = $"{colorEmoji} {mapPin.Type.ToUpper()}\n{mapPin.Building}, Floor {mapPin.Floor}";

            var pin = new Pin
            {
                Label = mapPin.Title,
                Address = pinAddress,
                Location = new Microsoft.Maui.Devices.Sensors.Location(mapPin.Coordinates.Latitude, mapPin.Coordinates.Longitude)
            };

            pin.MarkerClicked += (s, e) =>
            {
                Debug.WriteLine($"*** PIN MARKER CLICKED EVENT FIRED *** Pin: {mapPin.Title}");
                OnPinTapped(mapPin);
            };

            _map.Pins.Add(pin);
            Debug.WriteLine($"Added pin to map: {mapPin.Title} ({mapPin.Type}) at {mapPin.Coordinates.Latitude}, {mapPin.Coordinates.Longitude}");
        }

        private void AddClusterPinToMap(ClusterPin cluster)
        {
            if (_map == null || cluster == null) return;

            try
            {
                // Collect buildings in cluster
                var buildingsInCluster = cluster.Pins
                    .Select(p => p.Building)
                    .Distinct()
                    .ToList();

                var buildingInfo = buildingsInCluster.Count == 1 
                    ? buildingsInCluster[0]
                    : $"{buildingsInCluster.Count} buildings";

                var pinAddress = $"{cluster.GetClusterEmoji()} {cluster.GetClusterBadgeText()}\n{cluster.PinCount} {(cluster.PinCount == 1 ? "location" : "locations")} • {cluster.DominantType}\n📍 {buildingInfo}";

                var pin = new Pin
                {
                    Label = cluster.GetClusterBadgeText(),
                    Location = new Microsoft.Maui.Devices.Sensors.Location(
                        cluster.CenterLocation.Latitude,
                        cluster.CenterLocation.Longitude
                    ),
                    Address = pinAddress
                };

                pin.MarkerClicked += (s, e) =>
                {
                    OnClusterTapped(cluster);
                };

                _map.Pins.Add(pin);
                Debug.WriteLine($"Added cluster pin to map: {cluster.GetClusterBadgeText()} ({cluster.DominantType}) at {cluster.CenterLocation.Latitude}, {cluster.CenterLocation.Longitude} with {cluster.PinCount} pins in {buildingInfo}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding cluster pin: {ex.Message}");
            }
        }

        private string GetColorEmojiForType(string type)
        {
            return type?.ToLower() switch
            {
                "library" => "🔵",       // Blue
                "cafeteria" => "🟠",    // Orange
                "sports" => "🟢",       // Green
                "lab" => "🟣",          // Purple
                "auditorium" => "🔴",   // Red
                "health" => "🌸",       // Pink/Health
                _ => "⚪"               // Gray
            };
        }

        private CategoryChip CreateFilterButton(string type)
        {
            var chip = new CategoryChip(type, GetFilterColor(type));
            chip.IsSelected = type == _currentFilter;
            chip.Tapped += (s, e) =>
            {
                _currentFilter = type;
                FilterLocations(type);
                UpdateFilterButtonStates();
            };

            _filterChips.Add((type, chip));
            return chip;
        }

        private static Color GetFilterColor(string type) => type switch
        {
            "library" => (Color)Application.Current!.Resources["LibraryColor"],
            "cafeteria" => (Color)Application.Current!.Resources["CafeteriaColor"],
            "sports" => (Color)Application.Current!.Resources["SportsColor"],
            "lab" => (Color)Application.Current!.Resources["LabColor"],
            "auditorium" => (Color)Application.Current!.Resources["AuditoriumColor"],
            "health" => (Color)Application.Current!.Resources["HealthColor"],
            _ => Colors.Gray
        };

        private void UpdateFilterButtonStates()
        {
            foreach (var (type, chip) in _filterChips)
            {
                chip.IsSelected = type == _currentFilter;
            }
        }

        private Button CreateBuildingButton(Building? building)
        {
            var buttonText = building == null ? "All" : $"{building.Emoji} {building.Name} ({building.LocationCount})";
            var isSelected = building == null ? _viewModel?.SelectedBuilding == null : _viewModel?.SelectedBuilding?.Name == building.Name;

            var button = new Button
            {
                Text = buttonText,
                BackgroundColor = isSelected ? (Color)Application.Current!.Resources["Gray950"] : (Color)Application.Current!.Resources["Gray100"],
                TextColor = isSelected ? Colors.White : (Color)Application.Current!.Resources["Gray950"],
                FontSize = 11,
                Padding = 10,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Start
            };
            SemanticProperties.SetDescription(button, building == null ? "Show all buildings" : $"Select building {building.Name}");

            button.Clicked += (s, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectBuildingCommand.Execute(building);
                    RefreshMapPins();
                    // Update all building buttons appearance
                    Debug.WriteLine($"Selected building: {(building == null ? "All Buildings" : building.Name)}");
                }
            };

            return button;
        }

        private void FilterLocations(string type)
        {
            if (_viewModel?.MapPins == null) return;

            foreach (var pin in _viewModel.MapPins)
            {
                // Check type filter
                var typeMatch = type == "All" || pin.Type.Equals(type, StringComparison.OrdinalIgnoreCase);

                // Check building filter
                var buildingMatch = _viewModel.SelectedBuilding == null || 
                                   pin.Building.Equals(_viewModel.SelectedBuilding.Name, StringComparison.OrdinalIgnoreCase);

                // Pin is visible if it matches both filters
                pin.IsVisible = typeMatch && buildingMatch;
            }

            RefreshMapPins();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_viewModel?.MapPins == null) return;

            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

            foreach (var pin in _viewModel.MapPins)
            {
                // Check search match
                var searchMatch = string.IsNullOrEmpty(searchText) ||
                    pin.Title.ToLower().Contains(searchText) ||
                    pin.Type.ToLower().Contains(searchText) ||
                    pin.Building.ToLower().Contains(searchText) ||
                    pin.Subtitle.ToLower().Contains(searchText);

                // Check building filter
                var buildingMatch = _viewModel.SelectedBuilding == null || 
                                   pin.Building.Equals(_viewModel.SelectedBuilding.Name, StringComparison.OrdinalIgnoreCase);

                // Pin is visible if it matches both search and building filter
                pin.IsVisible = searchMatch && buildingMatch;
            }

            RefreshMapPins();
        }

        private void RefreshMapPins()
        {
            if (_map == null) return;

            _map.Pins.Clear();

            // Add cluster pins if available and have items
            if (_viewModel?.ClusterPins != null && _viewModel.ClusterPins.Count > 0)
            {
                Debug.WriteLine($"RefreshMapPins: Using ClusterPins ({_viewModel.ClusterPins.Count} total)");
                foreach (var cluster in _viewModel.ClusterPins)
                {
                    if (cluster.IsCluster && !cluster.IsExpanded)
                    {
                        // Display as cluster pin
                        AddClusterPinToMap(cluster);
                    }
                    else if (cluster.Pins.Count == 1)
                    {
                        // Single pin in cluster, display normally
                        AddPinToMap(cluster.Pins[0]);
                    }
                    else if (cluster.IsExpanded)
                    {
                        // Cluster is expanded, show individual pins
                        foreach (var pin in cluster.Pins)
                        {
                            if (pin.IsVisible)
                            {
                                AddPinToMap(pin);
                            }
                        }
                    }
                }
            }
            // Fallback: if no clusters, display regular pins
            else if (_viewModel?.MapPins != null && _viewModel.MapPins.Count > 0)
            {
                Debug.WriteLine($"RefreshMapPins: Using MapPins fallback ({_viewModel.MapPins.Count} total)");
                foreach (var pin in _viewModel.MapPins)
                {
                    if (pin.IsVisible)
                    {
                        AddPinToMap(pin);
                    }
                }
            }
            else
            {
                Debug.WriteLine("RefreshMapPins: No pins or clusters available");
            }

            Debug.WriteLine($"Map refreshed with {_map.Pins.Count} visible pins/clusters");
        }

        private void OnPinTapped(MapPin mapPin)
        {
            Debug.WriteLine($"*** OnPinTapped CALLED *** Pin tapped: {mapPin.Title} (ID: {mapPin.LocationId})");

            // Navigate to location details
            NavigateToLocation(mapPin.LocationId);
        }

        private void OnClusterTapped(ClusterPin cluster)
        {
            if (cluster == null) return;

            Debug.WriteLine($"*** CLUSTER TAPPED EVENT FIRED *** Cluster tapped: {cluster.PinCount} pins, type: {cluster.DominantType}");

            if (!cluster.IsCluster)
            {
                // Single pin, navigate to it
                if (cluster.Pins.Count == 1)
                {
                    OnPinTapped(cluster.Pins[0]);
                }
                return;
            }

            // Expand cluster with animation
            if (!cluster.IsExpanded)
            {
                AnimateClusterExpansion(cluster);
            }
            else
            {
                // Collapse cluster with animation
                AnimateClusterCollapse(cluster);
            }
        }

        private async void AnimateClusterExpansion(ClusterPin cluster)
        {
            if (_map == null) return;

            try
            {
                // Disable map interactions briefly
                _map.IsEnabled = false;

                // Animate zoom with slight fade
                uint animationDuration = 500; // milliseconds

                // Start zoom animation
                var zoomTask = MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    ZoomToCluster(cluster);
                    await Task.Delay(100);
                });

                await zoomTask;

                // Update view model
                _viewModel?.ExpandCluster(cluster);
                cluster.IsExpanded = true;

                // Slight delay before refresh for smooth animation
                await Task.Delay(300);

                // Refresh pins
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshMapPins();
                });

                Debug.WriteLine($"Cluster expanded with animation, zoomed to show {cluster.PinCount} pins");

                // Re-enable map after animation
                await Task.Delay(200);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _map.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error animating cluster expansion: {ex.Message}");
                _map.IsEnabled = true;
            }
        }

        private async void AnimateClusterCollapse(ClusterPin cluster)
        {
            if (_map == null) return;

            try
            {
                // Disable map interactions briefly
                _map.IsEnabled = false;

                // Collapse with animation
                _viewModel?.CollapseCluster(cluster);
                cluster.IsExpanded = false;

                // Fade out current pins
                await Task.Delay(150);

                // Refresh pins with collapse
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RefreshMapPins();
                });

                // Brief zoom out effect
                await Task.Delay(200);

                Debug.WriteLine($"Cluster collapsed with animation");

                // Re-enable map
                _map.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error animating cluster collapse: {ex.Message}");
                _map.IsEnabled = true;
            }
        }

        private void ZoomToCluster(ClusterPin cluster)
        {
            if (_map == null || cluster == null) return;

            try
            {
                // Calculate zoom level based on cluster radius
                // Smaller cluster = more zoom
                var radiusInMiles = cluster.ClusterRadius * 0.621371; // Convert km to miles
                var zoomDistance = Math.Max(radiusInMiles * 3, 0.25); // At least 0.25 miles

                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Microsoft.Maui.Devices.Sensors.Location(
                        cluster.CenterLocation.Latitude,
                        cluster.CenterLocation.Longitude
                    ),
                    Distance.FromMiles(zoomDistance)
                ));

                Debug.WriteLine($"Zoomed to cluster at {cluster.CenterLocation.Latitude}, {cluster.CenterLocation.Longitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error zooming to cluster: {ex.Message}");
            }
        }

        private async void NavigateToLocation(string locationId)
        {
            try
            {
                var navUrl = $"location-details?id={Uri.EscapeDataString(locationId)}";
                Debug.WriteLine($"Navigating to: {navUrl}");
                await Shell.Current.GoToAsync(navUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Could not navigate to location details: {ex.Message}", "OK");
            }
        }

        private void OnCenterMapClicked(object? sender, EventArgs e)
        {
            if (_map == null) return;

            try
            {
                var centerLocation = new Microsoft.Maui.Devices.Sensors.Location(3.1391, 101.6871);
                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    centerLocation,
                    Distance.FromMiles(0.5)
                ));
                Debug.WriteLine("Map centered on campus");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Center map error: {ex.Message}");
            }
        }

        private void OnZoomInClicked(object? sender, EventArgs e)
        {
            if (_map?.VisibleRegion == null) return;

            try
            {
                var region = _map.VisibleRegion;
                var newRegion = MapSpan.FromCenterAndRadius(
                    region.Center,
                    Distance.FromMiles(region.Radius.Miles / 2)
                );
                _map.MoveToRegion(newRegion);
                Debug.WriteLine("Zoomed in");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Zoom in error: {ex.Message}");
            }
        }

        private void OnZoomOutClicked(object? sender, EventArgs e)
        {
            if (_map?.VisibleRegion == null) return;

            try
            {
                var region = _map.VisibleRegion;
                var newRegion = MapSpan.FromCenterAndRadius(
                    region.Center,
                    Distance.FromMiles(region.Radius.Miles * 2)
                );
                _map.MoveToRegion(newRegion);
                Debug.WriteLine("Zoomed out");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Zoom out error: {ex.Message}");
            }
        }

        private async void OnFloorPlansClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_viewModel?.SelectedBuilding == null)
                {
                    await DisplayAlert("No Building Selected", "Please select a building first to view its floor plans", "OK");
                    return;
                }

                Debug.WriteLine($"Navigating to floor plan for building: {_viewModel.SelectedBuilding.Name}");

                // Get the FloorPlanPage and load data
                var floorPlanPage = ServiceHelper.GetService<FloorPlanPage>();
                if (floorPlanPage != null && _viewModel?.Locations != null)
                {
                    // Convert ObservableCollection to List and load the floor plans
                    var locationsList = _viewModel.Locations.ToList();
                    floorPlanPage.LoadBuildingFloorPlans(_viewModel.SelectedBuilding, locationsList);
                }

                // Use absolute routing with proper URI format
                await Shell.Current.GoToAsync("///floorplan");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Floor plans navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Could not open floor plans: {ex.Message}", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine($"*** CampusMapPage APPEARED *** Map pins count: {_map?.Pins.Count ?? 0}, ViewModel has {_viewModel?.MapPins?.Count ?? 0} pins");

            if (_viewModel != null)
            {
                // Load locations from ViewModel (async operation)
                // The LoadLocationsCommand will populate MapPins, which triggers PropertyChanged -> RefreshMapPins
                await _viewModel.LoadLocationsCommand.ExecuteAsync(null);
                RefreshMapPins();
                FocusOnPendingLocation();
            }
        }

        private void FocusOnPendingLocation()
        {
            if (string.IsNullOrEmpty(_focusLocationId) || _map == null || _viewModel == null)
                return;

            var focusId = _focusLocationId;
            var location = _viewModel.Locations.FirstOrDefault(l => l.Id == focusId);
            _focusLocationId = null;

            if (location?.Coordinates == null)
                return;

            var cluster = _viewModel.ClusterPins.FirstOrDefault(c => c.IsCluster && c.Pins.Any(p => p.LocationId == focusId));
            if (cluster != null && !cluster.IsExpanded)
            {
                _viewModel.ExpandCluster(cluster);
                RefreshMapPins();
            }

            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Microsoft.Maui.Devices.Sensors.Location(location.Coordinates.Latitude, location.Coordinates.Longitude),
                Distance.FromMeters(150)));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("*** CampusMapPage DISAPPEARED ***");

            // Unsubscribe to prevent memory leaks
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }
    }
}
