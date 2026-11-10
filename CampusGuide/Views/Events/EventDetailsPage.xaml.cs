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
        private EventsViewModel? _viewModel;
        private EventItem? _currentEvent;

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
            _viewModel = ServiceHelper.GetService<EventsViewModel>();

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main scroll layout
            var scrollView = new ScrollView
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Default
            };

            var mainStack = new VerticalStackLayout
            {
                Padding = 0,
                Spacing = 0
            };

            // Header with placeholder
            var headerBox = new BoxView
            {
                HeightRequest = 200,
                Color = Color.FromArgb("#E0E0E0")
            };
            mainStack.Add(headerBox);

            // Content grid
            var contentGrid = new Grid
            {
                Padding = new Thickness(20, 15),
                RowSpacing = 15
            };

            // Title
            var titleLabel = new Label
            {
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap
            };
            contentGrid.Add(titleLabel, 0, 0);

            // Date and time section
            var dateTimeStack = new VerticalStackLayout { Spacing = 8 };
            dateTimeStack.Add(new Label
            {
                Text = "📅 Date & Time",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var dateRangeLabel = new Label
            {
                FontSize = 13,
                TextColor = Colors.DarkGray
            };
            dateTimeStack.Add(dateRangeLabel);

            Grid.SetRow(dateTimeStack, 1);
            contentGrid.Add(dateTimeStack, 0, 1);

            // Location section
            var locationStack = new VerticalStackLayout { Spacing = 8 };
            locationStack.Add(new Label
            {
                Text = "📍 Location",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var locationLabel = new Label
            {
                FontSize = 13,
                TextColor = Colors.DarkGray
            };
            locationStack.Add(locationLabel);

            var viewOnMapButton = new Button
            {
                Text = "View Location on Map",
                Padding = new Thickness(15, 10),
                CornerRadius = 6,
                BackgroundColor = Colors.Blue,
                TextColor = Colors.White,
                FontSize = 12,
                Margin = new Thickness(0, 8, 0, 0)
            };
            SemanticProperties.SetDescription(viewOnMapButton, "View this event's location on the map");
            viewOnMapButton.Clicked += OnViewOnMapClicked;
            locationStack.Add(viewOnMapButton);

            Grid.SetRow(locationStack, 2);
            contentGrid.Add(locationStack, 0, 2);

            // Description section
            var descriptionStack = new VerticalStackLayout { Spacing = 8 };
            descriptionStack.Add(new Label
            {
                Text = "📝 Description",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var descriptionLabel = new Label
            {
                FontSize = 13,
                TextColor = Colors.DarkGray,
                LineBreakMode = LineBreakMode.WordWrap
            };
            descriptionStack.Add(descriptionLabel);

            Grid.SetRow(descriptionStack, 3);
            contentGrid.Add(descriptionStack, 0, 3);

            mainStack.Add(contentGrid);
            scrollView.Content = mainStack;

            // Container grid for layering views
            var containerGrid = new Grid();
            containerGrid.Add(scrollView);

            Content = containerGrid;
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

                DisplayEventDetails();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventDetailsPage: Error loading event: {ex.Message}");
                DisplayNotFoundState();
            }
        }

        private void DisplayEventDetails()
        {
            if (_currentEvent == null)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Clear and recreate content with event details
                var mainStack = new VerticalStackLayout
                {
                    Padding = 0,
                    Spacing = 0
                };

                // Header
                var headerBox = new BoxView
                {
                    HeightRequest = 200,
                    Color = Color.FromArgb("#E0E0E0")
                };
                mainStack.Add(headerBox);

                // Content grid
                var contentGrid = new Grid
                {
                    Padding = new Thickness(20, 15),
                    RowSpacing = 15
                };

                // Title
                contentGrid.Add(new Label
                {
                    Text = _currentEvent.Title,
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.WordWrap
                }, 0, 0);

                // Date and time section
                var dateTimeStack = new VerticalStackLayout { Spacing = 8 };
                dateTimeStack.Add(new Label
                {
                    Text = "📅 Date & Time",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    Margin = new Thickness(0, 8, 0, 0)
                });

                var dateRangeText = $"{_currentEvent.StartTime:MMMM dd, yyyy} at {_currentEvent.StartTime:h:mm tt}\nto {_currentEvent.EndTime:h:mm tt}";
                dateTimeStack.Add(new Label
                {
                    Text = dateRangeText,
                    FontSize = 13,
                    TextColor = Colors.DarkGray
                });

                Grid.SetRow(dateTimeStack, 1);
                contentGrid.Add(dateTimeStack, 0, 1);

                // Location section
                var locationStack = new VerticalStackLayout { Spacing = 8 };
                locationStack.Add(new Label
                {
                    Text = "📍 Location",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    Margin = new Thickness(0, 8, 0, 0)
                });

                locationStack.Add(new Label
                {
                    Text = _currentEvent.LocationId ?? "Location not specified",
                    FontSize = 13,
                    TextColor = Colors.DarkGray
                });

                var viewMapButton = new Button
                {
                    Text = "View Location on Map",
                    Padding = new Thickness(15, 10),
                    CornerRadius = 6,
                    BackgroundColor = Colors.Blue,
                    TextColor = Colors.White,
                    FontSize = 12,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                SemanticProperties.SetDescription(viewMapButton, "View this event's location on the map");
                viewMapButton.Clicked += OnViewOnMapClicked;
                locationStack.Add(viewMapButton);

                Grid.SetRow(locationStack, 2);
                contentGrid.Add(locationStack, 0, 2);

                // Description section
                var descriptionStack = new VerticalStackLayout { Spacing = 8 };
                descriptionStack.Add(new Label
                {
                    Text = "📝 Description",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    Margin = new Thickness(0, 8, 0, 0)
                });

                descriptionStack.Add(new Label
                {
                    Text = _currentEvent.Description ?? "No description available",
                    FontSize = 13,
                    TextColor = Colors.DarkGray,
                    LineBreakMode = LineBreakMode.WordWrap
                });

                Grid.SetRow(descriptionStack, 3);
                contentGrid.Add(descriptionStack, 0, 3);

                mainStack.Add(contentGrid);

                // Update content
                Content = new ScrollView { Content = mainStack };
            });
        }

        private void DisplayNotFoundState()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var notFoundStack = new VerticalStackLayout
                {
                    Spacing = 15,
                    Padding = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                notFoundStack.Add(new Label
                {
                    Text = "❌",
                    FontSize = 48,
                    HorizontalOptions = LayoutOptions.Center
                });
                notFoundStack.Add(new Label
                {
                    Text = "Event Not Found",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                });

                var backButton = new Button
                {
                    Text = "Back to Events",
                    Padding = new Thickness(20, 10),
                    CornerRadius = 6,
                    BackgroundColor = Colors.Blue,
                    TextColor = Colors.White,
                    Margin = new Thickness(20, 0)
                };
                SemanticProperties.SetDescription(backButton, "Returns to the events list");
                backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

                notFoundStack.Add(backButton);

                Content = notFoundStack;
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
