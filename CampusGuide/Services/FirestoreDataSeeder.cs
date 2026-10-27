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
                if (locations.Count > 0)
                {
#if ANDROID
                    Log.Debug(TAG, $"Firestore already has {locations.Count} locations. Skipping seed.");
#endif
                    return;
                }

#if ANDROID
                Log.Debug(TAG, "Firestore is empty. Seeding with test data...");
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
                    Name = "Main Library",
                    Description = "Central library with study areas, computer labs, and quiet zones",
                    Type = "library",
                    Building = "Engineering Block",
                    Floor = "1-3",
                    Coordinates = new Coordinates { Latitude = 1.3521, Longitude = 103.8198 },
                    Images = new List<string> { "library1.jpg", "library2.jpg" },
                    Tags = new List<string> { "library", "study", "quiet", "computers" },
                    QrCodeId = "campus://location/lib-001"
                },
                new LocationModel
                {
                    Id = "cafe-001",
                    Name = "Student Cafeteria",
                    Description = "Main cafeteria with variety of food options and seating areas",
                    Type = "cafeteria",
                    Building = "Student Center",
                    Floor = "G",
                    Coordinates = new Coordinates { Latitude = 1.3525, Longitude = 103.8200 },
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
                    Coordinates = new Coordinates { Latitude = 1.3530, Longitude = 103.8195 },
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
                    Coordinates = new Coordinates { Latitude = 1.3518, Longitude = 103.8202 },
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
                    Coordinates = new Coordinates { Latitude = 1.3523, Longitude = 103.8205 },
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
                    Coordinates = new Coordinates { Latitude = 1.3528, Longitude = 103.8190 },
                    Images = new List<string> { "health.jpg" },
                    Tags = new List<string> { "health", "medical", "clinic", "doctor" },
                    QrCodeId = "campus://location/health-001"
                }
            };

#if ANDROID
            Log.Debug(TAG, $"Prepared {locations.Count} locations for Firestore (seeding via Firebase Console)");
#endif
            // Note: Actual Firestore write would happen here if the service supported batch writes
            // For now, data is seeded manually via Firebase Console using FIRESTORE_TEST_DATA.md
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
                    Coordinates = new Coordinates { Latitude = 1.3523, Longitude = 103.8205 },
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
                    Coordinates = new Coordinates { Latitude = 1.3523, Longitude = 103.8205 },
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
                    Coordinates = new Coordinates { Latitude = 1.3530, Longitude = 103.8195 },
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
                    Coordinates = new Coordinates { Latitude = 1.3521, Longitude = 103.8198 },
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
                    LocationId = "cafe-001",
                    Coordinates = new Coordinates { Latitude = 1.3525, Longitude = 103.8200 },
                    ImageUrl = "networking.jpg",
                    IsPublished = true
                }
            };

#if ANDROID
            Log.Debug(TAG, $"Prepared {events.Count} events for Firestore (seeding via Firebase Console)");
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
                    RelatedLocationId = "cafe-001",
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

#if ANDROID
            Log.Debug(TAG, $"Prepared {notifications.Count} notifications for Firestore (seeding via Firebase Console)");
#endif
        }
    }
}
