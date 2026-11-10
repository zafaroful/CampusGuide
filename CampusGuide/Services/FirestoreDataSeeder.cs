using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

#if ANDROID
using Android.Util;
using Firebase.Firestore;
#endif

namespace CampusGuide.Services
{
    /// <summary>
    /// Service to seed Firestore with test data for development/demonstration.
    /// This ensures the app has realistic data to work with.
    /// </summary>
    public class FirestoreDataSeeder
    {
        private const string TAG = "FirestoreDataSeeder";
        private readonly IFirestoreService _firestoreService;

        public FirestoreDataSeeder(IFirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        /// <summary>
        /// Seeds Firestore with test data if collections are empty.
        /// Should be called once during app initialization.
        /// </summary>
        public async Task SeedDataIfEmptyAsync()
        {
            try
            {
#if ANDROID
                Log.Debug(TAG, "Checking if Firestore needs test data...");
#endif

                // Check if data already exists
                var locations = await _firestoreService.GetAllLocationsAsync();

                // If we have old/incomplete data, force a reseed
                // This helps when the seeder logic changes
                bool needsReseed = locations.Count > 0 && locations.Count < 6;

                var events = await _firestoreService.GetUpcomingEventsAsync();
                var notifications = await _firestoreService.GetNotificationsAsync();

                if (locations.Count > 0 && !needsReseed && events.Count > 0 && notifications.Count > 0)
                {
#if ANDROID
                    Log.Debug(TAG, $"✅ Firestore already has {locations.Count} complete locations. Skipping seed.");
#endif
                    return;
                }

                if (needsReseed)
                {
#if ANDROID
                    Log.Debug(TAG, $"⚠️ Firestore has incomplete data ({locations.Count} locations found, expected 6). Clearing and re-seeding...");
#endif
                    // Clear old incomplete data
                    await _firestoreService.DeleteAllLocationsAsync().ConfigureAwait(false);
                }

#if ANDROID
                Log.Debug(TAG, "Seeding Firestore with complete test data...");
#endif
                await SeedLocationsAsync();
                await SeedEventsAsync();
                await SeedNotificationsAsync();

#if ANDROID
                Log.Debug(TAG, "✅ Firestore seeding completed successfully!");
#endif
            }
            catch (Exception ex)
            {
#if ANDROID
                Log.Error(TAG, $"Error seeding Firestore: {ex.Message}");
#endif
            }
        }

        private async Task SeedLocationsAsync()
        {
            var locations = new List<LocationModel>
            {
                new LocationModel
                {
                    Id = "lib-001",
                    Name = "Central Library",
                    Description = "Main library with study areas, computer labs, and quiet zones",
                    Type = "library",
                    Building = "Academic Block A",
                    Floor = "3",
                    Coordinates = new Coordinates { Latitude = 5.3971, Longitude = 100.3084 },
                    Images = new List<string> { "library1.jpg", "library2.jpg" },
                    Tags = new List<string> { "library", "study", "quiet", "computers" },
                    QrCodeId = "campus://location/lib-001"
                },
                new LocationModel
                {
                    Id = "cafeteria-001",
                    Name = "Cafeteria",
                    Description = "Main campus dining",
                    Type = "Food",
                    Building = "Food Court",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 5.3968, Longitude = 100.3080 },
                    Images = new List<string> { "cafeteria.jpg" },
                    Tags = new List<string> { "food", "cafeteria", "seating", "halal", "vegetarian" },
                    QrCodeId = "campus://location/cafe-001"
                },
                new LocationModel
                {
                    Id = "gym-001",
                    Name = "Sports Complex",
                    Description = "Multi-purpose sports facility with gym, courts, and pool",
                    Type = "sports",
                    Building = "Recreation Building",
                    Floor = "B1-2",
                    Coordinates = new Coordinates { Latitude = 5.3965, Longitude = 100.3075 },
                    Images = new List<string> { "gym.jpg", "courts.jpg" },
                    Tags = new List<string> { "sports", "gym", "fitness", "court", "swimming" },
                    QrCodeId = "campus://location/gym-001"
                },
                new LocationModel
                {
                    Id = "lab-001",
                    Name = "Computer Science Lab",
                    Description = "State-of-the-art computer lab for programming and AI research",
                    Type = "lab",
                    Building = "Engineering Block",
                    Floor = "2",
                    Coordinates = new Coordinates { Latitude = 5.3978, Longitude = 100.3088 },
                    Images = new List<string> { "lab.jpg" },
                    Tags = new List<string> { "lab", "computers", "programming", "research", "ai" },
                    QrCodeId = "campus://location/lab-001"
                },
                new LocationModel
                {
                    Id = "aud-001",
                    Name = "Main Auditorium",
                    Description = "Large auditorium for lectures, seminars, and events",
                    Type = "auditorium",
                    Building = "Academic Block",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 5.3972, Longitude = 100.3080 },
                    Images = new List<string> { "auditorium.jpg" },
                    Tags = new List<string> { "auditorium", "lecture", "events", "seating" },
                    QrCodeId = "campus://location/aud-001"
                },
                new LocationModel
                {
                    Id = "health-001",
                    Name = "Health Center",
                    Description = "Student health services and medical clinic",
                    Type = "health",
                    Building = "Medical Building",
                    Floor = "1",
                    Coordinates = new Coordinates { Latitude = 5.3978, Longitude = 100.3093 },
                    Images = new List<string> { "health.jpg" },
                    Tags = new List<string> { "health", "medical", "clinic", "doctor" },
                    QrCodeId = "campus://location/health-001"
                },
                new LocationModel
                {
                    Id = "student-center-001",
                    Name = "Student Center",
                    Description = "Central gathering place",
                    Type = "Facility",
                    Building = "Center Block",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 5.3975, Longitude = 100.3090 },
                    Images = new List<string>(),
                    Tags = new List<string> { "facility", "student", "gathering" },
                    QrCodeId = "campus://location/student-center-001"
                }
            };

#if ANDROID
            Log.Debug(TAG, $"Adding {locations.Count} locations to Firestore...");
#endif

            // Actually write locations to Firestore
            var success = await _firestoreService.AddLocationsAsync(locations).ConfigureAwait(false);

            if (success)
            {
#if ANDROID
                Log.Debug(TAG, $"✅ Successfully added {locations.Count} locations to Firestore");
#endif
            }
            else
            {
#if ANDROID
                Log.Warn(TAG, $"⚠️ Failed to add locations to Firestore. Locations may not be accessible.");
#endif
            }
        }

        private async Task SeedEventsAsync()
        {
            var events = new List<EventItem>
            {
                new EventItem
                {
                    Id = "evt-001",
                    Title = "Campus Tour 2024",
                    Description = "Welcome tour for new students exploring campus facilities",
                    StartTime = DateTime.UtcNow.AddDays(3),
                    EndTime = DateTime.UtcNow.AddDays(3).AddHours(2),
                    LocationId = "aud-001",
                    Coordinates = new Coordinates { Latitude = 5.3972, Longitude = 100.3080 },
                    ImageUrl = "tour.jpg",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "evt-002",
                    Title = "Tech Talk: AI in Campus Life",
                    Description = "Special lecture on artificial intelligence applications in education",
                    StartTime = DateTime.UtcNow.AddDays(5),
                    EndTime = DateTime.UtcNow.AddDays(5).AddHours(1),
                    LocationId = "aud-001",
                    Coordinates = new Coordinates { Latitude = 5.3972, Longitude = 100.3080 },
                    ImageUrl = "tech_talk.jpg",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "evt-003",
                    Title = "Sports Day 2024",
                    Description = "Annual inter-school sports competition and games",
                    StartTime = DateTime.UtcNow.AddDays(7),
                    EndTime = DateTime.UtcNow.AddDays(7).AddHours(6),
                    LocationId = "gym-001",
                    Coordinates = new Coordinates { Latitude = 5.3965, Longitude = 100.3075 },
                    ImageUrl = "sports_day.jpg",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "evt-004",
                    Title = "Library Workshop: Research Skills",
                    Description = "Learn effective research methods and library resources",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(1).AddMinutes(30),
                    LocationId = "lib-001",
                    Coordinates = new Coordinates { Latitude = 5.3971, Longitude = 100.3084 },
                    ImageUrl = "workshop.jpg",
                    IsPublished = true
                },
                new EventItem
                {
                    Id = "evt-005",
                    Title = "Networking Lunch with Alumni",
                    Description = "Meet and greet with successful alumni professionals",
                    StartTime = DateTime.UtcNow.AddDays(10),
                    EndTime = DateTime.UtcNow.AddDays(10).AddHours(2),
                    LocationId = "cafeteria-001",
                    Coordinates = new Coordinates { Latitude = 5.3968, Longitude = 100.3080 },
                    ImageUrl = "networking.jpg",
                    IsPublished = true
                }
            };

            var eventsAdded = await _firestoreService.AddEventsAsync(events);

#if ANDROID
            if (eventsAdded)
                Log.Debug(TAG, $"✅ Successfully added {events.Count} events to Firestore");
            else
                Log.Warn(TAG, "⚠️ Failed to add events to Firestore");
#endif
        }

        private async Task SeedNotificationsAsync()
        {
            var notifications = new List<NotificationItem>
            {
                new NotificationItem
                {
                    Id = "notif-001",
                    Title = "Library Maintenance Notice",
                    Body = "3rd floor library will be closed for maintenance on Nov 20th",
                    Type = "announcement",
                    RelatedLocationId = "lib-001",
                    RelatedEventId = "",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new NotificationItem
                {
                    Id = "notif-002",
                    Title = "Cafeteria Menu Update",
                    Body = "Check new halal and vegetarian options available this week",
                    Type = "announcement",
                    RelatedLocationId = "cafeteria-001",
                    RelatedEventId = "",
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                },
                new NotificationItem
                {
                    Id = "notif-003",
                    Title = "Campus Tour Registration Open",
                    Body = "Don't miss the welcome tour! Register now for limited spots",
                    Type = "event_reminder",
                    RelatedLocationId = "aud-001",
                    RelatedEventId = "evt-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new NotificationItem
                {
                    Id = "notif-004",
                    Title = "Wi-Fi Upgrade Complete",
                    Body = "Campus-wide Wi-Fi speed improved by 50%. Reconnect to enjoy!",
                    Type = "update",
                    RelatedLocationId = "",
                    RelatedEventId = "",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new NotificationItem
                {
                    Id = "notif-005",
                    Title = "Sports Complex Extended Hours",
                    Body = "Gym now open until 10 PM on weekdays due to high demand",
                    Type = "announcement",
                    RelatedLocationId = "gym-001",
                    RelatedEventId = "",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            var notificationsAdded = await _firestoreService.AddNotificationsAsync(notifications);

#if ANDROID
            if (notificationsAdded)
                Log.Debug(TAG, $"✅ Successfully added {notifications.Count} notifications to Firestore");
            else
                Log.Warn(TAG, "⚠️ Failed to add notifications to Firestore");
#endif
        }
    }
}
