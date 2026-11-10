using Microsoft.Maui.Controls;
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

                var layout = new VerticalStackLayout { Padding = 20 };
                layout.Add(new Label { Text = "Welcome to CampusGuide", FontSize = 24, FontAttributes = FontAttributes.Bold });
                layout.Add(new Label { Text = "Campus Navigation & Assistance", FontSize = 16, Margin = new Thickness(0, 10) });

                var scanButton = new Button { Text = "Scan QR Code", Margin = new Thickness(0, 20) };
                SemanticProperties.SetDescription(scanButton, "Opens the QR code scanner");
                scanButton.Clicked += async (s, e) => await _viewModel.GoToScanner();
                layout.Add(scanButton);

                var favButton = new Button { Text = "View Favorites", Margin = new Thickness(0, 10) };
                SemanticProperties.SetDescription(favButton, "Opens your favorite locations");
                favButton.Clicked += async (s, e) => await _viewModel.GoToFavorites();
                layout.Add(favButton);

                var eventsButton = new Button { Text = "Events", Margin = new Thickness(0, 10) };
                SemanticProperties.SetDescription(eventsButton, "Opens campus events");
                eventsButton.Clicked += async (s, e) => await _viewModel.GoToEvents();
                layout.Add(eventsButton);

                var statusStack = new VerticalStackLayout { Spacing = 10, Margin = new Thickness(0, 20, 0, 0), HorizontalOptions = LayoutOptions.Center };
                var homeLoadingStack = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
                homeLoadingStack.Add(new ActivityIndicator { IsRunning = true, Color = Colors.Blue });
                homeLoadingStack.Add(new Label { Text = "Loading campus updates...", HorizontalTextAlignment = TextAlignment.Center });
                homeLoadingStack.SetBinding(IsVisibleProperty, new Binding("IsLoading", source: _viewModel));
                statusStack.Add(homeLoadingStack);

                var homeEmptyStack = new VerticalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.Center };
                homeEmptyStack.Add(new Label { Text = "No campus updates right now", FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.Add(new Label { Text = "Check back later for announcements.", FontSize = 14, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center });
                homeEmptyStack.SetBinding(IsVisibleProperty, new Binding("IsEmpty", source: _viewModel));
                statusStack.Add(homeEmptyStack);

                layout.Add(statusStack);

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
    }
}
