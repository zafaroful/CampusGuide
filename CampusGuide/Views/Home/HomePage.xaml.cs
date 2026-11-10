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

                var header = new VerticalStackLayout { Spacing = 4 };
                header.Add(new Label { Text = "Welcome to CampusGuide", FontSize = 26, FontAttributes = FontAttributes.Bold });
                header.Add(new Label { Text = "Campus Navigation & Assistance", FontSize = 15, TextColor = Colors.Gray });
                layout.Add(header);

                var scanCard = CreateCard("Scan QR Code", "Find a location instantly", "#3949AB", Colors.White, _viewModel.GoToScannerCommand);
                SemanticProperties.SetDescription(scanCard, "Opens the QR code scanner");
                layout.Add(scanCard);

                var grid = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                    RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                    ColumnSpacing = 12,
                    RowSpacing = 12
                };
                var mapTile = CreateTile("Map", "Browse buildings", _viewModel.GoToMapCommand);
                var searchTile = CreateTile("Search", "Find rooms", _viewModel.GoToSearchCommand);
                var eventsTile = CreateTile("Events", "Campus events", _viewModel.GoToEventsCommand);
                var favTile = CreateTile("Favorites", "Saved places", _viewModel.GoToFavoritesCommand);
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
                    BackgroundColor = Color.FromArgb("#F3F4F8")
                };
                var updatesContent = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                updatesContent.Add(new Label { Text = "Campus Updates", FontAttributes = FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center });

                var homeLoadingStack = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                homeLoadingStack.Add(new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#3949AB") });
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

        private static Border CreateCard(string title, string subtitle, string background, Color textColor, System.Windows.Input.ICommand command)
        {
            var card = new Border
            {
                Padding = 20,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                BackgroundColor = Color.FromArgb(background),
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label { Text = title, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = textColor },
                        new Label { Text = subtitle, FontSize = 14, TextColor = textColor }
                    }
                }
            };
            card.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
            return card;
        }

        private static Border CreateTile(string title, string subtitle, System.Windows.Input.ICommand command)
        {
            var tile = new Border
            {
                Padding = 16,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Stroke = Color.FromArgb("#D0D5E2"),
                BackgroundColor = Colors.Transparent,
                MinimumHeightRequest = 48,
                Content = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold },
                        new Label { Text = subtitle, FontSize = 12, TextColor = Colors.Gray }
                    }
                }
            };
            SemanticProperties.SetDescription(tile, subtitle);
            tile.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
            return tile;
        }
    }
}
