using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.ViewModels;

namespace CampusGuide.Views.Home
{
    public partial class HomePage : ContentPage
    {
        private HomeViewModel _viewModel;

        public HomePage()
        {
            System.Diagnostics.Debug.WriteLine("=== HomePage constructor called ===");
            try
            {
                _viewModel = new HomeViewModel();
                BindingContext = _viewModel;
                System.Diagnostics.Debug.WriteLine("✓ HomeViewModel created and bound");

                var layout = new VerticalStackLayout { Padding = new Thickness(16, 20), Spacing = 20 };

                var headerContent = new VerticalStackLayout { Spacing = 4 };
                headerContent.Add(new Label { Text = "Welcome to CampusGuide", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
                headerContent.Add(new Label { Text = "Campus Navigation & Assistance", FontSize = 15, TextColor = Color.FromArgb("#E8EAF6") });

                var header = new Border
                {
                    Padding = 20,
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Stroke = Colors.Transparent,
                    Background = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1),
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb("#1A237E"), 0),
                            new GradientStop(Color.FromArgb("#3949AB"), 1)
                        }
                    },
                    Content = headerContent
                };
                layout.Add(header);

                var scanCard = CreateScanCard(_viewModel.GoToScannerCommand);
                layout.Add(scanCard);

                var grid = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                    RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                    ColumnSpacing = 12,
                    RowSpacing = 12
                };
                var mapTile = CreateTile("\u2316", "Map", "Browse buildings", _viewModel.GoToMapCommand);
                var searchTile = CreateTile("\u2315", "Search", "Find rooms", _viewModel.GoToSearchCommand);
                var eventsTile = CreateTile("\u25A6", "Events", "Campus events", _viewModel.GoToEventsCommand);
                var favTile = CreateTile("\u2605", "Favorites", "Saved places", _viewModel.GoToFavoritesCommand);
                grid.Add(mapTile, 0, 0);
                grid.Add(searchTile, 1, 0);
                grid.Add(eventsTile, 0, 1);
                grid.Add(favTile, 1, 1);
                layout.Add(grid);

                var updatesCard = new Border
                {
                    Padding = 20,
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Stroke = Colors.Transparent,
                    BackgroundColor = ThemeColor("#F3F4F8", "#2A2D3A")
                };
                var updatesContent = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                updatesContent.Add(new Label { Text = "Campus Updates", FontAttributes = FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center });

                var homeLoadingStack = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                homeLoadingStack.Add(new ActivityIndicator { IsRunning = true });
                homeLoadingStack.Add(new Label { Text = "Loading campus updates...", HorizontalTextAlignment = TextAlignment.Center });
                homeLoadingStack.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
                updatesContent.Add(homeLoadingStack);

                var homeEmptyStack = new VerticalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.Center };
                homeEmptyStack.Add(new Label { Text = "No campus updates right now", FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.Add(new Label { Text = "Check back later for announcements.", FontSize = 14, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.SetBinding(IsVisibleProperty, new Binding("IsEmpty", source: _viewModel));
                updatesContent.Add(homeEmptyStack);

                updatesCard.Content = updatesContent;
                layout.Add(updatesCard);

                Content = new ScrollView { Content = layout };
                Title = "Home";
                System.Diagnostics.Debug.WriteLine("✓ HomePage layout built successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ HomePage constructor failed: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private static Border CreateScanCard(System.Windows.Input.ICommand command)
        {
            var card = new Border
            {
                Padding = 20,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Stroke = Colors.Transparent,
                BackgroundColor = ThemeColor("#00695C", "#00796B"),
                Content = new HorizontalStackLayout
                {
                    Spacing = 20,
                    Children =
                    {
                        new Label { Text = "\u25A3", FontSize = 48, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center },
                        new VerticalStackLayout
                        {
                            Spacing = 4,
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Label { Text = "Scan QR Code", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                                new Label { Text = "Find a location instantly", FontSize = 14, TextColor = Colors.White }
                            }
                        }
                    }
                }
            };
            SemanticProperties.SetDescription(card, "Opens the QR code scanner");
            card.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
            return card;
        }

        private static Border CreateTile(string icon, string title, string subtitle, System.Windows.Input.ICommand command)
        {
            var tile = new Border
            {
                Padding = 16,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Stroke = ThemeColor("#D0D5E2", "#4A4F63"),
                BackgroundColor = Colors.Transparent,
                MinimumHeightRequest = 48,
                Content = new HorizontalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = icon, FontSize = 26, VerticalOptions = LayoutOptions.Center },
                        new VerticalStackLayout
                        {
                            Spacing = 2,
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold },
                                new Label { Text = subtitle, FontSize = 12, TextColor = Colors.Gray }
                            }
                        }
                    }
                }
            };
            tile.Stroke = ThemeColor("#D0D5E2", "#4A4F63");
            SemanticProperties.SetDescription(tile, subtitle);
            tile.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
            return tile;
        }

        private static Color ThemeColor(string light, string dark)
        {
            return Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb(dark)
                : Color.FromArgb(light);
        }
    }
}
