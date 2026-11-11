using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.Models;
using CampusGuide.Services;
using CampusGuide.ViewModels;
using CampusGuide.ViewModels.Search;
using CampusGuide.Views.Controls;
using System.Diagnostics;
using Microsoft.Maui.Layouts;

namespace CampusGuide.Views.Map
{
    [QueryProperty(nameof(LocationId), "id")]
    public partial class LocationDetailsPage : ContentPage
    {
        private string? _locationId;
        private Models.Location? _location;
        private IFirestoreService? _firestoreService;

        public string? LocationId
        {
            get => _locationId;
            set
            {
                if (value != null && value != _locationId)
                {
                    _locationId = Uri.UnescapeDataString(value);
                    Debug.WriteLine($"LocationDetailsPage: LocationId set to '{_locationId}' (length: {_locationId?.Length ?? 0})");
                    if (string.IsNullOrEmpty(_locationId))
                    {
                        Debug.WriteLine("LocationDetailsPage: WARNING - LocationId is empty or null!");
                    }
                    LoadLocationData();
                }
            }
        }

        public LocationDetailsPage()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
            BuildLoadingUI();
        }

        private void BuildLoadingUI()
        {
            var layout = new VerticalStackLayout { Padding = 20 };
            layout.Add(new Label 
            { 
                Text = "Loading location details...", 
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });

            Content = layout;
        }

        private async void LoadLocationData()
        {
            try
            {
                Debug.WriteLine($"LoadLocationData: Loading location with ID: {_locationId}");

                if (_firestoreService == null)
                {
                    _firestoreService = ServiceHelper.GetService<IFirestoreService>();
                }

                if (_firestoreService != null && !string.IsNullOrEmpty(_locationId))
                {
                    _location = await _firestoreService.GetLocationByIdAsync(_locationId);

                    _location ??= ServiceHelper.GetService<SearchViewModel>()?.AllLocations
                        .FirstOrDefault(l => l.Id == _locationId);

                    if (_location is not null)
                    {
                        Debug.WriteLine($"LoadLocationData: Successfully loaded location: {_location.Name}");
                        DisplayLocationDetails();
                    }
                    else
                    {
                        Debug.WriteLine($"LoadLocationData: Location not found for ID: {_locationId}");
                        DisplayLocationNotFound();
                    }
                }
                else
                {
                    Debug.WriteLine("LoadLocationData: FirestoreService or LocationId is null");
                    DisplayLocationNotFound();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadLocationData: Error loading location: {ex.Message}");
                DisplayError(ex.Message);
            }
        }

        private void DisplayLocationDetails()
        {
            if (_location is null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var scrollView = new ScrollView();
                var layout = new VerticalStackLayout { Padding = new Thickness(24, 20), Spacing = 4 };

                // Title
                layout.Add(new Label
                {
                    Text = _location.Name,
                    FontSize = 26,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = ThemeHelper.BodyText
                });

                // Type/Category
                layout.Add(new Label
                {
                    Text = _location.Type,
                    FontSize = 14,
                    TextColor = ThemeHelper.MutedText,
                    FontAttributes = FontAttributes.Italic
                });

                // Divider
                layout.Add(new BoxView
                {
                    Color = ThemeHelper.CardBorder,
                    HeightRequest = 1,
                    Margin = new Thickness(0, 16)
                });

                // Description
                if (!string.IsNullOrEmpty(_location.Description))
                {
                    layout.Add(CreateSectionLabel("Description"));
                    layout.Add(CreateValueLabel(_location.Description));
                }

                // Building Information
                if (!string.IsNullOrEmpty(_location.Building))
                {
                    layout.Add(CreateSectionLabel("Building"));
                    layout.Add(CreateValueLabel(_location.Building));
                }

                // Floor Information
                if (!string.IsNullOrEmpty(_location.Floor))
                {
                    layout.Add(CreateSectionLabel("Floor"));
                    layout.Add(CreateValueLabel(_location.Floor));
                }

                // Coordinates
                if (_location.Coordinates is not null)
                {
                    layout.Add(CreateSectionLabel("Coordinates"));

                    var coordGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                        ColumnSpacing = 10,
                        Padding = new Thickness(0, 4)
                    };

                    coordGrid.Add(new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Latitude",
                                FontSize = 12,
                                TextColor = ThemeHelper.MutedText
                            },
                            new Label
                            {
                                Text = $"{_location.Coordinates.Latitude:F6}",
                                FontSize = 14,
                                TextColor = ThemeHelper.BodyText
                            }
                        }
                    }, 0, 0);

                    coordGrid.Add(new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Longitude",
                                FontSize = 12,
                                TextColor = ThemeHelper.MutedText
                            },
                            new Label
                            {
                                Text = $"{_location.Coordinates.Longitude:F6}",
                                FontSize = 14,
                                TextColor = ThemeHelper.BodyText
                            }
                        }
                    }, 1, 0);

                    layout.Add(coordGrid);
                }

                // QR Code ID
                if (!string.IsNullOrEmpty(_location.QrCodeId))
                {
                    layout.Add(CreateSectionLabel("QR Code"));
                    layout.Add(CreateValueLabel(_location.QrCodeId, 12, "Courier"));
                }

                // Tags
                if (_location.Tags is not null && _location.Tags.Count > 0)
                {
                    layout.Add(CreateSectionLabel("Tags"));

                    var tagLayout = new FlexLayout
                    {
                        Wrap = FlexWrap.Wrap,
                        Direction = FlexDirection.Row,
                        JustifyContent = FlexJustify.Start
                    };

                    foreach (var tag in _location.Tags)
                    {
                        tagLayout.Add(new Border
                        {
                            Padding = new Thickness(10, 4),
                            Margin = new Thickness(0, 0, 8, 8),
                            BackgroundColor = ThemeHelper.Themed("#F0F0F0", "#2C2F33"),
                            StrokeThickness = 0,
                            StrokeShape = new RoundRectangle { CornerRadius = 12 },
                            Content = new Label
                            {
                                Text = tag,
                                FontSize = 12,
                                TextColor = ThemeHelper.MutedText
                            }
                        });
                    }

                    layout.Add(tagLayout);
                }

                var saveButton = new Button
                {
                    Text = "Save to Favorites",
                    TextColor = Colors.White,
                    BackgroundColor = ThemeHelper.Accent,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(20, 12),
                    CornerRadius = 10,
                    Margin = new Thickness(0, 24, 0, 0)
                };
                SemanticProperties.SetDescription(saveButton, "Save this location to your favorites");
                saveButton.Clicked += async (s, e) =>
                {
                    var favoritesViewModel = ServiceHelper.GetService<FavoritesViewModel>();
                    if (favoritesViewModel is null || _location is null)
                        return;

                    saveButton.IsEnabled = false;
                    await favoritesViewModel.AddFavorite(_location);
                    await UpdateSaveButtonStateAsync(saveButton);
                };
                layout.Add(saveButton);
                _ = UpdateSaveButtonStateAsync(saveButton);

                // Add spacing before button
                layout.Add(new BoxView { HeightRequest = 20, IsVisible = false });

                var viewOnMapButton = new Button
                {
                    Text = "View on Map",
                    TextColor = ThemeHelper.Accent,
                    BackgroundColor = Colors.Transparent,
                    BorderColor = ThemeHelper.Accent,
                    BorderWidth = 1,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(20, 12),
                    CornerRadius = 10,
                    Margin = new Thickness(0, 12, 0, 0)
                };
                SemanticProperties.SetDescription(viewOnMapButton, "View this location on the campus map");
                viewOnMapButton.Clicked += async (s, e) => await Shell.Current.GoToAsync($"//map?focusId={Uri.EscapeDataString(_location.Id)}");
                layout.Add(viewOnMapButton);

                // Go Back Button
                var backButton = new Button
                {
                    Text = "Go Back",
                    TextColor = ThemeHelper.MutedText,
                    BackgroundColor = Colors.Transparent,
                    FontSize = 14,
                    Padding = new Thickness(20, 12),
                    Margin = new Thickness(0, 4, 0, 0)
                };
                SemanticProperties.SetDescription(backButton, "Go back to the previous page");
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");
                layout.Add(backButton);

                scrollView.Content = layout;
                Content = scrollView;
            });
        }

        private static Label CreateSectionLabel(string text) => new()
        {
            Text = text,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = ThemeHelper.MutedText,
            Margin = new Thickness(0, 16, 0, 2)
        };

        private static Label CreateValueLabel(string text, double fontSize = 14, string? fontFamily = null) => new()
        {
            Text = text,
            FontSize = fontSize,
            FontFamily = fontFamily,
            TextColor = ThemeHelper.BodyText,
            LineBreakMode = LineBreakMode.WordWrap
        };

        private async Task UpdateSaveButtonStateAsync(Button saveButton)
        {
            var favoritesService = ServiceHelper.GetService<IFavoritesService>();
            if (favoritesService is null || _location is null)
                return;

            var favorites = await favoritesService.GetFavoritesAsync();
            var isSaved = favorites.Any(f => f.LocationId == _location.Id);

            saveButton.Text = isSaved ? "Saved to Favorites" : "Save to Favorites";
            saveButton.BackgroundColor = isSaved ? ThemeHelper.Themed("#D0D0D0", "#3A3D42") : ThemeHelper.Accent;
            saveButton.IsEnabled = !isSaved;
        }

        private void DisplayLocationNotFound()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var layout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

                layout.Add(new Label
                {
                    Text = "Location Not Found",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Parse("#FF4444"),
                    Margin = new Thickness(0, 40, 0, 0)
                });

                layout.Add(new Label
                {
                    Text = $"Unable to find location with ID: {_locationId}",
                    FontSize = 14,
                    TextColor = Color.Parse("#666666"),
                    LineBreakMode = LineBreakMode.WordWrap
                });

                var backButton = new Button
                {
                    Text = "Go Back to Map",
                    TextColor = Color.Parse("#FFFFFF"),
                    Background = Color.Parse("#7B61FF"),
                    FontSize = 14,
                    Padding = new Thickness(20, 12),
                    CornerRadius = 8,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                SemanticProperties.SetDescription(backButton, "Go back to the map");
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");
                layout.Add(backButton);

                Content = layout;
            });
        }

        private void DisplayError(string errorMessage)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var layout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

                layout.Add(new Label
                {
                    Text = "Error Loading Location",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Parse("#FF4444"),
                    Margin = new Thickness(0, 40, 0, 0)
                });

                layout.Add(new Label
                {
                    Text = errorMessage,
                    FontSize = 13,
                    TextColor = Color.Parse("#999999"),
                    LineBreakMode = LineBreakMode.WordWrap
                });

                var backButton = new Button
                {
                    Text = "Go Back",
                    TextColor = Color.Parse("#FFFFFF"),
                    Background = Color.Parse("#7B61FF"),
                    FontSize = 14,
                    Padding = new Thickness(20, 12),
                    CornerRadius = 8,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");
                layout.Add(backButton);

                Content = layout;
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("LocationDetailsPage.OnAppearing()");
        }
    }
}
