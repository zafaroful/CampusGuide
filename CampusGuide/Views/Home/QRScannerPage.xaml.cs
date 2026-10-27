using Microsoft.Maui.Controls;
using CampusGuide.Services;
using CampusGuide.Models;

namespace CampusGuide.Views.Home
{
    public partial class QRScannerPage : ContentPage
    {
        private readonly IFirestoreService? _firestoreService;
        private readonly IQRCodeScannerService? _qrScannerService;

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
                    new RowDefinition(GridLength.Auto),    // Bottom controls
                },
                Padding = 0,
                RowSpacing = 0,
                BackgroundColor = Colors.Black
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
            mainGrid.Add(headerFrame, 0, 0);

            // Placeholder content area (Camera would go here)
            var contentArea = new StackLayout
            {
                Spacing = 20,
                Padding = 20,
                VerticalOptions = LayoutOptions.FillAndExpand,
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
                        Text = "Camera Scanner",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Real-time QR code detection will appear here",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#AAAAAA"),
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };
            mainGrid.Add(contentArea, 0, 1);

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
            testButton.Clicked += OnTestScanClicked;

            var cancelButton = new Button
            {
                Text = "Cancel",
                BackgroundColor = Color.FromArgb("#E74C3C"),
                TextColor = Colors.White,
                Padding = new Thickness(15, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            cancelButton.Clicked += OnCancelClicked;

            buttonLayout.Add(testButton);
            buttonLayout.Add(cancelButton);

            bottomFrame.Content = buttonLayout;
            mainGrid.Add(bottomFrame, 0, 2);

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
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Failed to request camera permission: {ex.Message}", "OK");
                });
            }
        }

        private async void OnTestScanClicked(object? sender, EventArgs e)
        {
            try
            {
                // Simulate a QR scan for testing
                var testQRValue = "campus://location/lib-001";

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Notify the service of the scanned value
                    if (_qrScannerService is ZXingQRCodeScannerService zsvc)
                    {
                        zsvc.OnBarcodeDetected(testQRValue);
                    }

                    // Process the scanned value
                    await ProcessScannedQRCodeAsync(testQRValue).ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Test scan failed: {ex.Message}", "OK");
            }
        }

        private async Task ProcessScannedQRCodeAsync(string scannedValue)
        {
            try
            {
                var locationId = ExtractLocationIdFromQRCode(scannedValue);

                if (string.IsNullOrEmpty(locationId))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Invalid QR Code", "Could not extract location ID from QR code.", "OK");
                    });
                    return;
                }

                if (_firestoreService != null)
                {
                    var location = await _firestoreService.GetLocationByIdAsync(locationId).ConfigureAwait(false);

                    if (location != null)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Shell.Current.GoToAsync($"home?locationId={locationId}");
                        });
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            var action = await DisplayAlert(
                                "Location Not Found",
                                $"No location found for QR code: {locationId}",
                                "Try Again",
                                "Cancel"
                            );

                            if (!action)
                            {
                                await Shell.Current.GoToAsync("..");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Failed to process QR code: {ex.Message}", "OK");
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

