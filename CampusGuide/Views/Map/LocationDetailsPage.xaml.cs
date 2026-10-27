using Microsoft.Maui.Controls;
using CampusGuide.Models;
using CampusGuide.Services;
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
                var layout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

                // Title
                layout.Add(new Label
                {
                    Text = _location.Name,
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Parse("#333333")
                });

                // Type/Category
                layout.Add(new Label
                {
                    Text = _location.Type,
                    FontSize = 14,
                    TextColor = Color.Parse("#7A7A7A"),
                    FontAttributes = FontAttributes.Italic
                });

                // Divider
                layout.Add(new BoxView
                {
                    Color = Color.Parse("#E0E0E0"),
                    HeightRequest = 1,
                    Margin = new Thickness(0, 10, 0, 10)
                });

                // Description
                if (!string.IsNullOrEmpty(_location.Description))
                {
                    layout.Add(new Label
                    {
                        Text = "Description",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333")
                    });

                    layout.Add(new Label
                    {
                        Text = _location.Description,
                        FontSize = 13,
                        TextColor = Color.Parse("#555555"),
                        LineBreakMode = LineBreakMode.WordWrap
                    });
                }

                // Building Information
                if (!string.IsNullOrEmpty(_location.Building))
                {
                    layout.Add(new Label
                    {
                        Text = "Building",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333"),
                        Margin = new Thickness(0, 10, 0, 0)
                    });

                    layout.Add(new Label
                    {
                        Text = _location.Building,
                        FontSize = 13,
                        TextColor = Color.Parse("#666666"),
                        Padding = new Thickness(10, 5, 10, 5),
                        Background = Color.Parse("#F5F5F5")
                    });
                }

                // Floor Information
                if (!string.IsNullOrEmpty(_location.Floor))
                {
                    layout.Add(new Label
                    {
                        Text = "Floor",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333"),
                        Margin = new Thickness(0, 10, 0, 0)
                    });

                    layout.Add(new Label
                    {
                        Text = _location.Floor,
                        FontSize = 13,
                        TextColor = Color.Parse("#666666")
                    });
                }

                // Coordinates
                if (_location.Coordinates is not null)
                {
                    layout.Add(new Label
                    {
                        Text = "Coordinates",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333"),
                        Margin = new Thickness(0, 10, 0, 0)
                    });

                    var coordGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                        ColumnSpacing = 10,
                        Padding = new Thickness(0, 5)
                    };

                    coordGrid.Add(new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Latitude",
                                FontSize = 11,
                                TextColor = Color.Parse("#999999")
                            },
                            new Label
                            {
                                Text = $"{_location.Coordinates.Latitude:F6}",
                                FontSize = 13,
                                TextColor = Color.Parse("#333333"),
                                FontAttributes = FontAttributes.Bold
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
                                FontSize = 11,
                                TextColor = Color.Parse("#999999")
                            },
                            new Label
                            {
                                Text = $"{_location.Coordinates.Longitude:F6}",
                                FontSize = 13,
                                TextColor = Color.Parse("#333333"),
                                FontAttributes = FontAttributes.Bold
                            }
                        }
                    }, 1, 0);

                    layout.Add(coordGrid);
                }

                // QR Code ID
                if (!string.IsNullOrEmpty(_location.QrCodeId))
                {
                    layout.Add(new Label
                    {
                        Text = "QR Code",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333"),
                        Margin = new Thickness(0, 10, 0, 0)
                    });

                    layout.Add(new Label
                    {
                        Text = _location.QrCodeId,
                        FontSize = 11,
                        TextColor = Color.Parse("#666666"),
                        FontFamily = "Courier",
                        Padding = new Thickness(10, 5, 10, 5),
                        Background = Color.Parse("#F5F5F5")
                    });
                }

                // Tags
                if (_location.Tags is not null && _location.Tags.Count > 0)
                {
                    layout.Add(new Label
                    {
                        Text = "Tags",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Parse("#333333"),
                        Margin = new Thickness(0, 10, 0, 0)
                    });

                    var tagLayout = new HorizontalStackLayout
                    {
                        Spacing = 5,
                        Margin = new Thickness(0, 5)
                    };

                    foreach (var tag in _location.Tags)
                    {
                        var tagLabel = new Label
                        {
                            Text = tag,
                            FontSize = 11,
                            TextColor = Color.Parse("#FFFFFF"),
                            Padding = new Thickness(10, 5),
                            Margin = new Thickness(2),
                            Background = Color.Parse("#7B61FF")
                        };
                        tagLayout.Add(tagLabel);
                    }

                    layout.Add(tagLayout);
                }

                // Add spacing before button
                layout.Add(new BoxView { HeightRequest = 20, IsVisible = false });

                // Go Back Button
                var backButton = new Button
                {
                    Text = "Go Back",
                    TextColor = Color.Parse("#FFFFFF"),
                    Background = Color.Parse("#7B61FF"),
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(20, 12),
                    CornerRadius = 8,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");
                layout.Add(backButton);

                scrollView.Content = layout;
                Content = scrollView;
            });
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
