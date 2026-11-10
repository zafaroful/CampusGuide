using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CampusGuide.Models;
using CampusGuide.Services;

namespace CampusGuide.ViewModels
{
    public partial class EventsViewModel : ObservableObject
    {
        private readonly IFirestoreService _firestoreService;

        [ObservableProperty]
        private ObservableCollection<EventItem> events = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasEvents = false;

        public EventsViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>() ?? throw new InvalidOperationException("IFirestoreService not registered");

            // Load events on initialization
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500); // Give Firebase time to initialize
                await LoadEventsCommand.ExecuteAsync(null);
            });
        }

        /// <summary>
        /// Loads upcoming events from Firebase Firestore and sorts by date.
        /// </summary>
        [RelayCommand]
        public async Task LoadEvents()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine("EventsViewModel: Loading events from Firestore...");
                var eventsList = await _firestoreService.GetUpcomingEventsAsync().ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Events.Clear();

                    // If no events from Firestore, load test data
                    if (eventsList == null || eventsList.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("EventsViewModel: No events from Firestore, loading test data...");
                        LoadTestEvents();
                        return;
                    }

                    // Sort by start time ascending (nearest events first)
                    var sortedEvents = eventsList
                        .OrderBy(e => e.StartTime)
                        .ToList();

                    foreach (var eventItem in sortedEvents)
                    {
                        Events.Add(eventItem);
                    }

                    HasEvents = Events.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"EventsViewModel: Loaded {Events.Count} events");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EventsViewModel: Error loading events: {ex.Message}");
                ErrorMessage = $"Failed to load events: {ex.Message}";
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HasEvents = false;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads test event data when Firestore is unavailable.
        /// </summary>
        private void LoadTestEvents()
        {
            var today = DateTime.Now;
            var testEvents = new List<EventItem>
            {
                new EventItem
                {
                    Id = "event-001",
                    Title = "Campus Orientation",
                    Description = "Welcome orientation for new students. Tour the campus and meet student services.",
                    StartTime = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0).AddDays(2),
                    EndTime = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0).AddDays(2),
                    LocationId = "student-center-001",
                    ImageUrl = "https://via.placeholder.com/400x300?text=Campus+Orientation",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "event-002",
                    Title = "Library Workshop: Research Skills",
                    Description = "Learn advanced research techniques and database navigation with our librarians.",
                    StartTime = new DateTime(today.Year, today.Month, today.Day, 14, 0, 0).AddDays(3),
                    EndTime = new DateTime(today.Year, today.Month, today.Day, 15, 30, 0).AddDays(3),
                    LocationId = "lib-001",
                    ImageUrl = "https://via.placeholder.com/400x300?text=Library+Workshop",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "event-003",
                    Title = "Sports Day",
                    Description = "Annual campus sports competition. All students welcome to participate or watch.",
                    StartTime = new DateTime(today.Year, today.Month, today.Day, 9, 0, 0).AddDays(5),
                    EndTime = new DateTime(today.Year, today.Month, today.Day, 17, 0, 0).AddDays(5),
                    LocationId = "cafeteria-001",
                    ImageUrl = "https://via.placeholder.com/400x300?text=Sports+Day",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "event-004",
                    Title = "Tech Talks: AI in Education",
                    Description = "Hear from industry experts about artificial intelligence and its impact on higher education.",
                    StartTime = new DateTime(today.Year, today.Month, today.Day, 11, 0, 0).AddDays(7),
                    EndTime = new DateTime(today.Year, today.Month, today.Day, 12, 30, 0).AddDays(7),
                    LocationId = "student-center-001",
                    ImageUrl = "https://via.placeholder.com/400x300?text=Tech+Talks",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "event-005",
                    Title = "Career Fair 2024",
                    Description = "Meet recruiters from leading companies. Explore internship and job opportunities.",
                    StartTime = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0).AddDays(10),
                    EndTime = new DateTime(today.Year, today.Month, today.Day, 16, 0, 0).AddDays(10),
                    LocationId = "student-center-001",
                    ImageUrl = "https://via.placeholder.com/400x300?text=Career+Fair",
                    IsPublished = true
                }
            };

            foreach (var eventItem in testEvents)
            {
                Events.Add(eventItem);
            }

            HasEvents = Events.Count > 0;
            System.Diagnostics.Debug.WriteLine($"EventsViewModel: Loaded {Events.Count} test events");
        }

        /// <summary>
        /// Refreshes the events list from Firestore.
        /// </summary>
        [RelayCommand]
        public async Task RefreshEvents()
        {
            await LoadEventsCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Gets event details by ID.
        /// </summary>
        public EventItem? GetEventById(string eventId)
        {
            return Events.FirstOrDefault(e => e.Id == eventId);
        }
    }
}
