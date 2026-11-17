using CampusGuide.ViewModels;
using CampusGuide.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using CampusGuide.Services;

namespace CampusGuide.Views.Events
{
    [QueryProperty(nameof(EventId), "id")]
    public class EventDetailsPage : ContentPage
    {
        private static readonly Color PageBackground = Color.FromArgb("#F5F6F8");
        private static readonly Color SurfaceColor = Color.FromArgb("#FFFFFF");
        private static readonly Color AccentColor = Color.FromArgb("#00695C");
        private static readonly Color TextPrimaryColor = Color.FromArgb("#1F1F1F");
        private static readonly Color TextSecondaryColor = Color.FromArgb("#5F6368");
        private static readonly Color BorderColor = Color.FromArgb("#E0E0E0");

        private EventsViewModel? _viewModel;
        private EventItem? _currentEvent;
        private string _locationName = string.Empty;

        private string _eventId = string.Empty;
        public string EventId
        {
            get => _eventId;
            set
            {
                _eventId = value;
                LoadEventData();
            }
        }

        public EventDetailsPage()
        {
            Title = "Event Details";
            BackgroundColor = PageBackground;
            _viewModel = ServiceHelper.GetService<EventsViewModel>();

            Content = BuildLayout(null, string.Empty);
        }

        private View BuildLayout(EventItem? evt, string locationName)
        {
            var mainStack = new VerticalStackLayout
            {
                Padding = 0,
                Spacing = 0
            };

            mainStack.Add(BuildHeader(evt));

            var contentStack = new VerticalStackLayout
            {
                Padding = new Thickness(16),
                Spacing = 16
            };

            contentStack.Add(new Label
            {
                Text = evt?.Title ?? string.Empty,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = TextPrimaryColor,
                LineBreakMode = LineBreakMode.WordWrap
            });

            if (evt != null)
            {
                contentStack.Add(BuildCard(
                    "\uD83D\uDCC5 Date & Time",
                    $"{evt.StartTime:MMMM dd, yyyy} at {evt.StartTime:h:mm tt}\nto {evt.EndTime:h:mm tt}"));

                contentStack.Add(BuildLocationCard(evt, locationName));

                contentStack.Add(BuildCard(
                    "\uD83D\uDCDD Description",
                    string.IsNullOrWhiteSpace(evt.Description) ? "No description available" : evt.Description));
            }

            mainStack.Add(contentStack);

            return new ScrollView
            {
                BackgroundColor = PageBackground,
                Content = mainStack
            };
        }

        private View BuildHeader(EventItem? evt)
        {
            var headerGrid = new Grid
            {
                HeightRequest = 200,
                BackgroundColor = AccentColor
            };

            var imageUrl = evt?.ImageUrl;
            if (!string.IsNullOrWhiteSpace(imageUrl)
                && Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                headerGrid.Add(new Image
                {
                    Source = ImageSource.FromUri(uri),
                    Aspect = Aspect.AspectFill
                });
            }

            SemanticProperties.SetDescription(headerGrid, evt?.Title ?? "Event image");
            return headerGrid;
        }

        private View BuildLocationCard(EventItem evt, string locationName)
        {
            var displayName = !string.IsNullOrWhiteSpace(locationName)
                ? locationName
                : (string.IsNullOrWhiteSpace(evt.LocationId) ? "Location not specified" : evt.LocationId);

            var stack = new VerticalStackLayout { Spacing = 12 };
            stack.Add(new Label
            {
                Text = "\uD83D\uDCCD Location",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = TextPrimaryColor
            });
            stack.Add(new Label
            {
                Text = displayName,
                FontSize = 14,
                TextColor = TextSecondaryColor
            });

            var viewOnMapButton = new Button
            {
                Text = "View Location on Map",
                FontSize = 14,
                CornerRadius = 12,
                BackgroundColor = AccentColor,
                TextColor = Colors.White,
                MinimumHeightRequest = 44,
                IsEnabled = !string.IsNullOrWhiteSpace(evt.LocationId)
            };
            SemanticProperties.SetDescription(viewOnMapButton, "View this event's location on the map");
            viewOnMapButton.Clicked += OnViewOnMapClicked;
            stack.Add(viewOnMapButton);

            return WrapInCard(stack);
        }

        private View BuildCard(string heading, string body)
        {
            var stack = new VerticalStackLayout { Spacing = 12 };
            stack.Add(new Label
            {
                Text = heading,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = TextPrimaryColor
            });
            stack.Add(new Label
            {
                Text = body,
                FontSize = 14,
                TextColor = TextSecondaryColor,
                LineBreakMode = LineBreakMode.WordWrap
            });

            return WrapInCard(stack);
        }

        private static Border WrapInCard(View content)
        {
            return new Border
            {
                BackgroundColor = SurfaceColor,
                Stroke = BorderColor,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16),
                Content = content
            };
        }

        private void LoadEventData()
        {
            if (string.IsNullOrEmpty(_eventId) || _viewModel == null)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Loading event {_eventId}");
                _currentEvent = _viewModel.GetEventById(_eventId);

                if (_currentEvent == null)
                {
                    System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Event not found - {_eventId}");
                    DisplayNotFoundState();
                    return;
                }

                _ = LoadLocationNameAndDisplayAsync(_currentEvent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Error loading event: {ex.Message}");
                DisplayNotFoundState();
            }
        }

        private async Task LoadLocationNameAndDisplayAsync(EventItem evt)
        {
            var locationName = string.Empty;
            try
            {
                var firestore = ServiceHelper.GetService<IFirestoreService>();
                if (firestore != null && !string.IsNullOrWhiteSpace(evt.LocationId))
                {
                    var location = await firestore.GetLocationByIdAsync(evt.LocationId);
                    locationName = location?.Name ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Location lookup failed: {ex.Message}");
            }

            _locationName = locationName;
            DisplayEventDetails();
        }

        private void DisplayEventDetails()
        {
            if (_currentEvent == null)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Content = BuildLayout(_currentEvent, _locationName);
            });
        }

        private void DisplayNotFoundState()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var notFoundStack = new VerticalStackLayout
                {
                    Spacing = 16,
                    Padding = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                notFoundStack.Add(new Label
                {
                    Text = "\u274C",
                    FontSize = 48,
                    HorizontalOptions = LayoutOptions.Center
                });
                notFoundStack.Add(new Label
                {
                    Text = "Event Not Found",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = TextPrimaryColor,
                    HorizontalTextAlignment = TextAlignment.Center
                });

                var backButton = new Button
                {
                    Text = "Back to Events",
                    CornerRadius = 12,
                    BackgroundColor = AccentColor,
                    TextColor = Colors.White,
                    MinimumHeightRequest = 44,
                    Margin = new Thickness(20, 0)
                };
                SemanticProperties.SetDescription(backButton, "Returns to the events list");
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

                notFoundStack.Add(backButton);

                Content = new Grid
                {
                    BackgroundColor = PageBackground,
                    Children = { notFoundStack }
                };
            });
        }

        private async void OnViewOnMapClicked(object? sender, EventArgs e)
        {
            if (_currentEvent?.LocationId != null)
            {
                System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Navigating to map for location {_currentEvent.LocationId}");
                await Shell.Current.GoToAsync($"//map?focusId={Uri.EscapeDataString(_currentEvent.LocationId)}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine($"EventDetailsPage: OnAppearing with EventId={_eventId}");
        }
    }
}
