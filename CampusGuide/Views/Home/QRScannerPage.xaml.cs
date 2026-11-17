using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.Services;
using CampusGuide.Models;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace CampusGuide.Views.Home
{
    public partial class QRScannerPage : ContentPage
    {
        private readonly IFirestoreService? _firestoreService;
        private readonly IQRCodeScannerService? _qrScannerService;
        private bool _isProcessing = false;
        private Label? _statusLabel;
        private Label? _progressLabel;
        private Label? _scannedLabel;
        private Label? _errorLabel;
        private Label? _previewIconLabel;
        private Label? _previewTitleLabel;
        private Label? _previewCaptionLabel;

        public QRScannerPage()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>();
            _qrScannerService = ServiceHelper.GetService<IQRCodeScannerService>();

            Title = "Scan QR Code";

            Shell.SetBackgroundColor(this, Token("MapCard"));
            Shell.SetForegroundColor(this, Token("MapPrimary"));
            Shell.SetTitleColor(this, Token("MapTextPrimary"));

            // Build UI in code-only since XAML control integration has issues
            BuildUI();
        }

        private static Color Token(string key) => (Color)Application.Current!.Resources[key];

        private void BuildUI()
        {
            // Main grid layout
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),    // Instruction banner
                    new RowDefinition(GridLength.Star),    // Camera preview
                    new RowDefinition(GridLength.Auto),    // Status
                    new RowDefinition(GridLength.Auto),    // Bottom controls
                },
                Padding = new Thickness(16, 12),
                RowSpacing = 12,
                BackgroundColor = Token("MapBackground")
            };

            var instructionLabel = new Label
            {
                Text = "Point camera at QR code",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Token("MapPrimary"),
                VerticalOptions = LayoutOptions.Center
            };
            SemanticProperties.SetHeadingLevel(instructionLabel, SemanticHeadingLevel.Level1);

            var instructionBorder = new Border
            {
                BackgroundColor = Token("MapPrimaryLight"),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 14),
                Content = new HorizontalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "🔳",
                            FontSize = 22,
                            VerticalOptions = LayoutOptions.Center
                        },
                        instructionLabel
                    }
                }
            };
            Grid.SetRow(instructionBorder, 0);
            mainGrid.Add(instructionBorder);

            _previewIconLabel = new Label
            {
                Text = "📷",
                FontSize = 56,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var scanFrame = new Border
            {
                WidthRequest = 200,
                HeightRequest = 200,
                BackgroundColor = Colors.Transparent,
                Stroke = Token("AccentDark"),
                StrokeThickness = 3,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                HorizontalOptions = LayoutOptions.Center,
                Content = _previewIconLabel
            };

            _previewTitleLabel = new Label
            {
                Text = "Camera Preview",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };

            _previewCaptionLabel = new Label
            {
                Text = "Simulated preview. Use Test Scan to simulate scanning a QR code.",
                FontSize = 13,
                TextColor = Token("TextSecondaryDark"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var formatLabel = new Label
            {
                Text = "QR format: campus://location/LOCATION_ID",
                FontSize = 12,
                TextColor = Token("TextSecondaryDark"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var previewBorder = new Border
            {
                BackgroundColor = Token("SurfaceDark"),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Content = new VerticalStackLayout
                {
                    Spacing = 16,
                    Padding = new Thickness(20),
                    VerticalOptions = LayoutOptions.Center,
                    Children = { scanFrame, _previewTitleLabel, _previewCaptionLabel, formatLabel }
                }
            };
            Grid.SetRow(previewBorder, 1);
            mainGrid.Add(previewBorder);

            _statusLabel = new Label
            {
                Text = "Ready to scan QR codes",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Token("MapPrimary"),
                HorizontalTextAlignment = TextAlignment.Center
            };

            _progressLabel = new Label
            {
                Text = "⏳ Please wait...",
                FontSize = 12,
                TextColor = Token("Warning"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };

            _scannedLabel = new Label
            {
                Text = "",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Token("Success"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };

            _errorLabel = new Label
            {
                Text = "",
                FontSize = 12,
                TextColor = Token("Error"),
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false
            };

            var statusStack = new VerticalStackLayout
            {
                Spacing = 6,
                Children = { _statusLabel, _progressLabel, _scannedLabel, _errorLabel }
            };

            var statusBorder = new Border
            {
                BackgroundColor = Token("MapCard"),
                Stroke = Token("MapBorder"),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 12),
                Content = statusStack
            };
            Grid.SetRow(statusBorder, 2);
            mainGrid.Add(statusBorder);

            var testButton = CreateActionButton("🔍  Test Scan", "Simulates a QR code scan for testing",
                Token("MapPrimary"), Colors.White, Token("MapPrimary"), Color.FromArgb("#066B5F"));
            testButton.Clicked += OnTestScanClicked;

            var galleryButton = CreateActionButton("🖼  Scan from Gallery", "Selects a QR code image from the gallery and opens its location on the map",
                Token("MapCard"), Token("MapPrimary"), Token("MapBorder"), Token("MapPrimaryLight"));
            galleryButton.Clicked += OnGalleryScanClicked;

            var cancelButton = CreateActionButton("✕  Cancel", "Cancels scanning and returns",
                Token("MapCard"), Token("MapTextPrimary"), Token("MapBorder"), Token("MapDisabled"));
            cancelButton.Clicked += OnCancelClicked;

            var actionGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                RowSpacing = 10,
                ColumnSpacing = 10
            };

            Grid.SetRow(testButton, 0);
            Grid.SetColumnSpan(testButton, 2);
            actionGrid.Add(testButton);

            Grid.SetRow(galleryButton, 1);
            Grid.SetColumn(galleryButton, 0);
            actionGrid.Add(galleryButton);

            Grid.SetRow(cancelButton, 1);
            Grid.SetColumn(cancelButton, 1);
            actionGrid.Add(cancelButton);

            Grid.SetRow(actionGrid, 3);
            mainGrid.Add(actionGrid);

            Content = mainGrid;
        }

        private static Button CreateActionButton(string text, string description, Color background, Color textColor, Color border, Color pressedColor)
        {
            var button = new Button
            {
                Text = text,
                BackgroundColor = background,
                TextColor = textColor,
                BorderColor = border,
                BorderWidth = 1,
                CornerRadius = 12,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 48,
                MinimumHeightRequest = 48,
                Padding = new Thickness(12, 0),
                HorizontalOptions = LayoutOptions.Fill
            };
            SemanticProperties.SetDescription(button, description);

            var states = new VisualStateGroup { Name = "CommonStates" };
            states.States.Add(new VisualState
            {
                Name = "Normal",
                Setters =
                {
                    new Setter { Property = Button.BackgroundColorProperty, Value = background },
                    new Setter { Property = Button.TextColorProperty, Value = textColor }
                }
            });
            states.States.Add(new VisualState
            {
                Name = "Pressed",
                Setters =
                {
                    new Setter { Property = Button.BackgroundColorProperty, Value = pressedColor },
                    new Setter { Property = Button.TextColorProperty, Value = textColor }
                }
            });
            states.States.Add(new VisualState
            {
                Name = "Disabled",
                Setters =
                {
                    new Setter { Property = Button.BackgroundColorProperty, Value = Token("MapDisabled") },
                    new Setter { Property = Button.TextColorProperty, Value = Token("MapTextSecondary") },
                    new Setter { Property = Button.BorderColorProperty, Value = Token("MapBorder") }
                }
            });

            var groups = new VisualStateGroupList();
            groups.Add(states);
            VisualStateManager.SetVisualStateGroups(button, groups);

            return button;
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
                        UpdateUI("QR code detected successfully!", testQRValue, true);

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

        private async void OnGalleryScanClicked(object? sender, EventArgs e)
        {
            if (_isProcessing)
            {
                await DisplayAlert("Processing", "Please wait while the previous scan is being processed.", "OK");
                return;
            }

            _isProcessing = true;

            try
            {
                var photo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a QR code image",
                    FileTypes = FilePickerFileType.Images
                });
                if (photo == null)
                {
                    return;
                }

                UpdateUI("Reading image...", "", false);

                string? qrValue;
                using (var stream = await photo.OpenReadAsync())
                {
                    qrValue = DecodeQRCodeFromImage(stream);
                }

                if (string.IsNullOrEmpty(qrValue))
                {
                    UpdateUI("No QR code found", "", false);
                    await DisplayAlert("No QR Code", "No QR code was found in the selected image.", "OK");
                    UpdateUI("Ready to scan QR codes", "", false);
                    return;
                }

                var locationId = ExtractLocationIdFromQRCode(qrValue);
                if (string.IsNullOrEmpty(locationId))
                {
                    UpdateUI("Invalid QR code", "Could not extract location ID", false);
                    await DisplayAlert("Invalid QR Code", "Could not extract location ID from QR code.\n\nFormat should be: campus://location/LOCATION_ID", "OK");
                    UpdateUI("Ready to scan QR codes", "", false);
                    return;
                }

                UpdateUI("Fetching location data...", "", false);

                var location = _firestoreService != null
                    ? await _firestoreService.GetLocationByIdAsync(locationId)
                    : null;

                if (location == null)
                {
                    UpdateUI("Location not found", locationId, false);
                    await DisplayAlert("Location Not Found", $"No location found for QR code: {locationId}", "OK");
                    UpdateUI("Ready to scan QR codes", "", false);
                    return;
                }

                UpdateUI("Opening map...", "", false);
                await Shell.Current.GoToAsync($"//map?focusId={Uri.EscapeDataString(locationId)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QRScannerPage: Gallery scan error: {ex.Message}");
                UpdateUI("Error processing image", ex.Message, false);
                await DisplayAlert("Error", $"Failed to scan image: {ex.Message}", "OK");
                UpdateUI("Ready to scan QR codes", "", false);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static string? DecodeQRCodeFromImage(Stream stream)
        {
            using var bitmap = SKBitmap.Decode(stream);
            if (bitmap == null)
            {
                return null;
            }

            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };

            return reader.Decode(bitmap)?.Text;
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
					if (_statusLabel == null || _progressLabel == null || _scannedLabel == null || _errorLabel == null)
						return;

					var isError = statusText.Contains("Error") || statusText.Contains("Failed") || statusText.Contains("Invalid") || statusText.Contains("not found") || statusText.Contains("No QR");
					var isSuccess = statusText.Contains("detected successfully");
					var isPermission = statusText.Contains("permission");
					var isBusy = statusText.Contains("Processing") || statusText.Contains("Fetching") || statusText.Contains("Opening") || statusText.Contains("Reading") || statusText.Contains("Simulating") || statusText.Contains("Scanning");

					_statusLabel.Text = statusText;
					_statusLabel.TextColor = isError
						? Token("Error")
						: isSuccess
							? Token("Success")
							: isPermission
								? Token("Warning")
								: Token("MapPrimary");

					_progressLabel.IsVisible = isBusy;

					_scannedLabel.Text = $"✓ QR: {scannedText}";
					_scannedLabel.IsVisible = showScanned && !string.IsNullOrEmpty(scannedText);

					_errorLabel.Text = $"✗ {scannedText}";
					_errorLabel.IsVisible = isError && !string.IsNullOrEmpty(scannedText);

					if (_previewCaptionLabel != null)
					{
						_previewCaptionLabel.Text = isPermission
							? "Camera access is needed to scan. Enable it in Settings."
							: "Simulated preview. Use Test Scan to simulate scanning a QR code.";
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

