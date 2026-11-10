using Microsoft.Maui.Controls;
using CampusGuide.Services;
using CampusGuide.Models;

namespace CampusGuide.Views.Home
{
    public partial class QRScannerPage : ContentPage
    {
        private readonly IFirestoreService? _firestoreService;
        private readonly IQRCodeScannerService? _qrScannerService;
        private bool _isProcessing = false;

        public QRScannerPage()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
            _qrScannerService = ServiceHelper.GetService<IQRCodeScannerService>();

            Title = "Scan QR Code";

            // Build UI in code-only since XAML control integration has issues
            BuildUI();
        }

        private void BuildUI()
        {
            // Main grid layout
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),    // Header
                    new RowDefinition(GridLength.Star),    // Content area
                    new RowDefinition(GridLength.Auto),    // Status/instructions
                    new RowDefinition(GridLength.Auto),    // Bottom controls
                },
                Padding = 0,
                RowSpacing = 0,
                BackgroundColor = Colors.Black,
                ColumnSpacing = 0
            };

            // Header with title
            var headerFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#2C3E50"),
                BorderColor = Colors.Transparent,
                Padding = 15,
                CornerRadius = 0,
                HasShadow = false
            };

            var headerLabel = new Label
            {
                Text = "Point camera at QR code",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };
            headerFrame.Content = headerLabel;
            Grid.SetRow(headerFrame, 0);
            Grid.SetColumn(headerFrame, 0);
            mainGrid.Add(headerFrame);

            // Camera placeholder area with better visual feedback
            var cameraPlaceholder = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = 20,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label
                    {
                        Text = "📷",
                        FontSize = 80,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    },
                    new Label
                    {
                        Text = "Camera Preview",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Use the Test Scan button below to simulate scanning a QR code.\n\nQR Format: campus://location/LOCATION_ID",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#AAAAAA"),
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };
            Grid.SetRow(cameraPlaceholder, 1);
            Grid.SetColumn(cameraPlaceholder, 0);
            mainGrid.Add(cameraPlaceholder);

            // Status/instructions area with enhanced visual feedback
            var statusStack = new VerticalStackLayout
            {
                Padding = new Thickness(15, 10),
                Spacing = 8,
                BackgroundColor = Color.FromArgb("#1C2833")
            };

            var statusLabel = new Label
            {
                Text = "Ready to scan QR codes",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#3498DB"),
                HorizontalTextAlignment = TextAlignment.Center
            };
            statusStack.Add(statusLabel);

            // Progress indicator
            var progressIndicator = new Label
            {
                Text = "",
                FontSize = 11,
                TextColor = Color.FromArgb("#F39C12"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };
            statusStack.Add(progressIndicator);

            var scannedValueLabel = new Label
            {
                Text = "",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2ECC71"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };
            statusStack.Add(scannedValueLabel);

            // Error message label
            var errorLabel = new Label
            {
                Text = "",
                FontSize = 11,
                TextColor = Color.FromArgb("#E74C3C"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };
            statusStack.Add(errorLabel);

            Grid.SetRow(statusStack, 2);
            Grid.SetColumn(statusStack, 0);
            mainGrid.Add(statusStack);

            // Bottom controls
            var bottomFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#2C3E50"),
                BorderColor = Colors.Transparent,
                Padding = 10,
                CornerRadius = 0,
                HasShadow = false
            };

            var buttonLayout = new HorizontalStackLayout
            {
                Spacing = 10,
                Padding = 10
            };

            var testButton = new Button
            {
                Text = "Test Scan",
                BackgroundColor = Color.FromArgb("#3498DB"),
                TextColor = Colors.White,
                Padding = new Thickness(15, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            SemanticProperties.SetDescription(testButton, "Simulates a QR code scan for testing");
            testButton.Clicked += OnTestScanClicked;

            var cancelButton = new Button
            {
                Text = "Cancel",
                BackgroundColor = Color.FromArgb("#E74C3C"),
                TextColor = Colors.White,
                Padding = new Thickness(15, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            SemanticProperties.SetDescription(cancelButton, "Cancels scanning and returns");
            cancelButton.Clicked += OnCancelClicked;

            buttonLayout.Add(testButton);
            buttonLayout.Add(cancelButton);

            bottomFrame.Content = buttonLayout;
            Grid.SetRow(bottomFrame, 3);
            Grid.SetColumn(bottomFrame, 0);
            mainGrid.Add(bottomFrame);

            Content = mainGrid;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Request camera permission
                var granted = await PermissionHelper.EnsureCameraPermissionAsync().ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!granted)
                    {
                        DisplayAlert("Permission Denied", "Camera permission is required to scan QR codes.", "OK");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("QRScannerPage: Camera permission granted");
                    }
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Failed to request camera permission: {ex.Message}", "OK");
                    System.Diagnostics.Debug.WriteLine($"QRScannerPage: Permission error: {ex.Message}");
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Clean up any resources when leaving page
            _isProcessing = false;
        }

        private async void OnTestScanClicked(object? sender, EventArgs e)
        {
            if (_isProcessing)
            {
                await DisplayAlert("Processing", "Please wait while the previous scan is being processed.", "OK");
                return;
            }

            try
            {
                _isProcessing = true;

                // Show loading state
                UpdateUI("Simulating scan...", "", false);

                // Simulate a QR scan for testing
                var testQRValue = "campus://location/lib-001";

                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Test scan initiated with value: {testQRValue}");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Update UI with detected value
                        UpdateUI("QR code detected!", testQRValue, true);

                        // Brief delay to show detected value
                        await Task.Delay(500).ConfigureAwait(false);

                        // Notify the service of the scanned value
                        if (_qrScannerService is ZXingQRCodeScannerService zsvc)
                        {
                            zsvc.OnBarcodeDetected(testQRValue);
                        }

                        // Process the scanned value
                        await ProcessScannedQRCodeAsync(testQRValue).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"QRScannerPage: Error in test scan: {ex.Message}");
                        UpdateUI("Scan failed", ex.Message, false);
                        await DisplayAlert("Error", $"Test scan failed: {ex.Message}", "OK");
                    }
                    finally
                    {
                        _isProcessing = false;
                    }
                });
            }
            catch (Exception ex)
            {
                _isProcessing = false;
                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Test scan exception: {ex.Message}");
                await DisplayAlert("Error", $"Test scan failed: {ex.Message}", "OK");
            }
        }

        private async Task ProcessScannedQRCodeAsync(string scannedValue)
        {
            try
            {
                UpdateUI("Processing QR code...", "", false);

                var locationId = ExtractLocationIdFromQRCode(scannedValue);

                if (string.IsNullOrEmpty(locationId))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        UpdateUI("Invalid QR code", "Could not extract location ID", false);
                        await DisplayAlert("Invalid QR Code", "Could not extract location ID from QR code.\n\nFormat should be: campus://location/LOCATION_ID", "OK");
                        UpdateUI("Ready to scan QR codes", "", false);
                    });
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Extracted location ID: {locationId}");
                UpdateUI("Fetching location data...", "", false);

                if (_firestoreService != null)
                {
                    var location = await _firestoreService.GetLocationByIdAsync(locationId).ConfigureAwait(false);

                    if (location != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"QRScannerPage: Location found: {location.Name}");
                        UpdateUI("Opening location...", "", false);

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                await Shell.Current.GoToAsync($"location-details?id={locationId}");
                            }
                            catch (Exception navEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Navigation error: {navEx.Message}");
                                UpdateUI("Navigation failed", navEx.Message, false);
                                await DisplayAlert("Navigation Error", $"Failed to open location: {navEx.Message}", "OK");
                                UpdateUI("Ready to scan QR codes", "", false);
                            }
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"QRScannerPage: Location not found: {locationId}");
                        UpdateUI("Location not found", locationId, false);

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            var action = await DisplayAlert(
                                "Location Not Found",
                                $"No location found for QR code: {locationId}\n\nTry scanning another QR code.",
                                "Try Again",
                                "Cancel"
                            );

                            UpdateUI("Ready to scan QR codes", "", false);

                            if (!action)
                            {
                                await Shell.Current.GoToAsync("..");
                            }
                        });
                    }
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        UpdateUI("Service unavailable", "Firestore service not configured", false);
                        await DisplayAlert("Error", "Firestore service is not available.", "OK");
                        UpdateUI("Ready to scan QR codes", "", false);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Error processing QR code: {ex.Message}");
                UpdateUI("Error processing scan", ex.Message, false);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Failed to process QR code: {ex.Message}", "OK");
                    UpdateUI("Ready to scan QR codes", "", false);
                });
            }
        }

        private string ExtractLocationIdFromQRCode(string qrValue)
        {
            if (string.IsNullOrWhiteSpace(qrValue))
                return string.Empty;

            qrValue = qrValue.Trim();

            // Handle URL format: campus://location/LOC001
            if (qrValue.Contains("://"))
            {
                try
                {
                    var uri = new Uri(qrValue);
                    var segments = uri.Segments;
                    if (segments.Length > 0)
                    {
                        return segments[^1].TrimEnd('/');
                    }
                }
                catch { }
            }

            // Handle HTTPS URLs: https://campus.edu/locations/LOC001
            if (qrValue.StartsWith("http://") || qrValue.StartsWith("https://"))
            {
                try
                {
                    var uri = new Uri(qrValue);
                    var segments = uri.Segments;
                    if (segments.Length > 0)
                    {
                        return segments[^1].TrimEnd('/');
                    }
                }
                catch { }
            }

            // Assume it's a plain location ID
            return qrValue;
        }

        private void UpdateUI(string statusText, string scannedText, bool showScanned)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var mainGrid = (Grid)Content;
                    if (mainGrid == null)
                        return;

                    // Find the status stack layout (row 2)
                    foreach (var child in mainGrid.Children)
                    {
                        var childRow = mainGrid.GetRow(child);
                        if (childRow == 2 && child is VerticalStackLayout statusStack)
                        {
                            // Update status label
                            if (statusStack.Children.Count > 0 && statusStack.Children[0] is Label statusLabel)
                            {
                                statusLabel.Text = statusText;
                                // Change color based on status
                                if (statusText.Contains("Error") || statusText.Contains("Failed"))
                                {
                                    statusLabel.TextColor = Color.FromArgb("#E74C3C");
                                }
                                else if (statusText.Contains("Processing") || statusText.Contains("Fetching") || statusText.Contains("Opening"))
                                {
                                    statusLabel.TextColor = Color.FromArgb("#F39C12");
                                }
                                else if (statusText.Contains("detected"))
                                {
                                    statusLabel.TextColor = Color.FromArgb("#2ECC71");
                                }
                                else
                                {
                                    statusLabel.TextColor = Color.FromArgb("#3498DB");
                                }
                            }

                            // Update progress indicator label
                            if (statusStack.Children.Count > 1 && statusStack.Children[1] is Label progressLabel)
                            {
                                if (statusText.Contains("Processing") || statusText.Contains("Fetching") || statusText.Contains("Opening"))
                                {
                                    progressLabel.Text = "⏳ Please wait...";
                                    progressLabel.IsVisible = true;
                                }
                                else
                                {
                                    progressLabel.IsVisible = false;
                                }
                            }

                            // Update scanned value label if it exists
                            if (statusStack.Children.Count > 2 && statusStack.Children[2] is Label scannedLabel)
                            {
                                if (showScanned && !string.IsNullOrEmpty(scannedText))
                                {
                                    scannedLabel.Text = $"✓ QR: {scannedText}";
                                    scannedLabel.IsVisible = true;
                                }
                                else
                                {
                                    scannedLabel.IsVisible = false;
                                }
                            }

                            // Update error label if it exists
                            if (statusStack.Children.Count > 3 && statusStack.Children[3] is Label errorLabel)
                            {
                                if (statusText.Contains("Error") || statusText.Contains("Failed"))
                                {
                                    errorLabel.Text = $"✗ {scannedText}";
                                    errorLabel.IsVisible = true;
                                }
                                else
                                {
                                    errorLabel.IsVisible = false;
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"QRScannerPage: Error updating UI: {ex.Message}");
                }
            });
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            try
            {
                // Navigate back to home tab using absolute routing
                await Shell.Current.GoToAsync("//home");
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Navigation Error", $"Failed to go back: {ex.Message}", "OK");
                });
            }
        }
    }
}

