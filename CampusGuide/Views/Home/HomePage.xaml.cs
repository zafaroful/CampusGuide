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
