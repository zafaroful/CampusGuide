using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.ViewModels;
using CampusGuide.Views.Controls;

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
                var welcomeLabel = new Label { Text = "Welcome to CampusGuide", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Colors.White };
                SemanticProperties.SetHeadingLevel(welcomeLabel, SemanticHeadingLevel.Level1);
                headerContent.Add(welcomeLabel);
                headerContent.Add(new Label { Text = "Campus Navigation & Assistance", FontSize = 15, TextColor = Colors.White, Opacity = 0.9 });

                var header = new Border
                {
                    Padding = new Thickness(20, 22),
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Stroke = Colors.Transparent,
                    BackgroundColor = (Color)Application.Current!.Resources["MapPrimary"],
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
                    Stroke = ThemeHelper.CardBorder,
                    BackgroundColor = ThemeHelper.CardBackground
                };
                var updatesContent = new VerticalStackLayout { Spacing = 12 };
                var updatesHeader = new HorizontalStackLayout { Spacing = 10 };
                updatesHeader.Add(new Label { Text = "\U0001F4E2", FontSize = 18, VerticalOptions = LayoutOptions.Center, TextColor = ThemeHelper.Accent });
                updatesHeader.Add(new Label { Text = "Campus Updates", FontAttributes = FontAttributes.Bold, FontSize = 16, TextColor = ThemeHelper.BodyText, VerticalOptions = LayoutOptions.Center });
                updatesContent.Add(updatesHeader);

                var homeLoadingStack = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                homeLoadingStack.Add(new ActivityIndicator { IsRunning = true, Color = ThemeHelper.Accent });
                homeLoadingStack.Add(new Label { Text = "Loading campus updates...", TextColor = ThemeHelper.MutedText, HorizontalTextAlignment = TextAlignment.Center });
                homeLoadingStack.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
                updatesContent.Add(homeLoadingStack);

                var homeEmptyStack = new VerticalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.Center };
                homeEmptyStack.Add(new Label { Text = "No campus updates right now", FontAttributes = FontAttributes.Bold, TextColor = ThemeHelper.BodyText, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.Add(new Label { Text = "Check back later for announcements.", FontSize = 14, TextColor = ThemeHelper.MutedText, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.SetBinding(IsVisibleProperty, new Binding("IsEmpty", source: _viewModel));
                updatesContent.Add(homeEmptyStack);

                var announcementsStack = new VerticalStackLayout { Spacing = 10 };
                announcementsStack.SetBinding(BindableLayout.ItemsSourceProperty, new Binding("Announcements", source: _viewModel));
                BindableLayout.SetItemTemplate(announcementsStack, new DataTemplate(() => CreateAnnouncementCard()));
                updatesContent.Add(announcementsStack);

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
                BackgroundColor = (Color)Application.Current!.Resources["MapPrimary"],
                MinimumHeightRequest = 64,
                Content = new HorizontalStackLayout
                {
                    Spacing = 20,
                    Children =
                    {
                        new Label { Text = "\u25A3", FontSize = 40, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center },
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
            var tap = new TapGestureRecognizer { Command = command };
            tap.Tapped += async (s, e) =>
            {
                await Task.WhenAll(card.FadeTo(0.85, 80), card.ScaleTo(0.97, 80));
                await Task.WhenAll(card.FadeTo(1, 120), card.ScaleTo(1, 120));
            };
            card.GestureRecognizers.Add(tap);
            return card;
        }

        private static Border CreateTile(string icon, string title, string subtitle, System.Windows.Input.ICommand command)
        {
            var tile = new Border
            {
                Padding = new Thickness(16, 14),
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Stroke = ThemeHelper.CardBorder,
                BackgroundColor = ThemeHelper.CardBackground,
                MinimumHeightRequest = 96,
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = icon, FontSize = 24, TextColor = ThemeHelper.Accent, HorizontalOptions = LayoutOptions.Start },
                        new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = ThemeHelper.BodyText },
                        new Label { Text = subtitle, FontSize = 12, TextColor = ThemeHelper.MutedText }
                    }
                }
            };
            SemanticProperties.SetDescription(tile, subtitle);
            tile.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
            return tile;
        }

        private static Border CreateAnnouncementCard()
        {
            var card = new Border
            {
                Padding = new Thickness(14, 12),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Stroke = ThemeHelper.CardBorder,
                BackgroundColor = ThemeHelper.CardBackground
            };
            var titleLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 15, TextColor = ThemeHelper.BodyText };
            titleLabel.SetBinding(Label.TextProperty, new Binding("Title"));
            var bodyLabel = new Label { FontSize = 14, TextColor = ThemeHelper.MutedText };
            bodyLabel.SetBinding(Label.TextProperty, new Binding("Body"));
            var dateLabel = new Label { FontSize = 12, TextColor = ThemeHelper.MutedText };
            dateLabel.SetBinding(Label.TextProperty, new Binding("CreatedAt", stringFormat: "{0:dd MMM yyyy, h:mm tt}"));
            card.Content = new VerticalStackLayout { Spacing = 4, Children = { titleLabel, bodyLabel, dateLabel } };
            return card;
        }


        private static Color ThemeColor(string light, string dark)
        {
            return Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb(dark)
                : Color.FromArgb(light);
        }
    }
}
