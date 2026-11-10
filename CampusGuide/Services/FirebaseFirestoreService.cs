using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

#if ANDROID
using Firebase.Firestore;
using Android.Util;
using Android.Gms.Tasks;
#endif

namespace CampusGuide.Services
{
#if ANDROID
    /// <summary>
    /// Android-specific Firestore implementation using Xamarin.Firebase.Firestore bindings.
    /// Maps Firestore collections (locations, events, notifications) to CampusGuide models.
    /// Uses lazy initialization to ensure Firebase is initialized before accessing Firestore.Instance
    /// </summary>
    public class FirebaseFirestoreService : IFirestoreService
    {
        const string TAG = "FirebaseFirestoreService";
        const string LocationsCollection = "locations";
        const string EventsCollection = "events";
        const string NotificationsCollection = "notifications";

        private FirebaseFirestore? _db;
        private readonly object _dbLock = new object();

        /// <summary>
        /// Gets the Firestore instance with lazy initialization.
        /// This ensures Firebase is fully initialized before we try to access Firestore.Instance
        /// </summary>
        private FirebaseFirestore GetFirestoreInstance()
        {
            if (_db == null)
            {
                lock (_dbLock)
                {
                    if (_db == null)
                    {
                        try
                        {
                            _db = FirebaseFirestore.Instance;
                            Log.Debug(TAG, "Firestore instance obtained successfully");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(TAG, $"Failed to get Firestore instance: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            return _db;
        }

        public FirebaseFirestoreService()
        {
            // Firestore initialization is deferred until first use (lazy initialization)
            // This allows Firebase.FirebaseApp.InitializeApp() in MainActivity to complete first
            Log.Debug(TAG, "FirebaseFirestoreService created (lazy initialization)");
        }

        public async Task<List<LocationModel>> GetAllLocationsAsync()
        {
            try
            {
                var locations = new List<LocationModel>();
                var tcs = new TaskCompletionSource<List<LocationModel>>();

                GetFirestoreInstance().Collection(LocationsCollection)
                    .Get()
                    .AddOnSuccessListener(new SuccessListener(snapshot =>
                    {
                        if (snapshot is QuerySnapshot qs)
                        {
                            foreach (var doc in qs.Documents)
                            {
                                var location = MapLocationDocument(doc);
                                if (location != null)
                                    locations.Add(location);
                            }
                        }
                        tcs.SetResult(locations);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"GetAllLocationsAsync failed: {ex.Message}");
                        tcs.SetResult(locations);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetAllLocationsAsync exception: {ex.Message}");
                return new List<LocationModel>();
            }
        }

        public async Task<LocationModel?> GetLocationByIdAsync(string id)
        {
            try
            {
                LocationModel? location = null;
                var tcs = new TaskCompletionSource<LocationModel?>();

                GetFirestoreInstance().Collection(LocationsCollection)
                    .Document(id)
                    .Get()
                    .AddOnSuccessListener(new SuccessListener(snapshot =>
                    {
                        if (snapshot is DocumentSnapshot ds && ds.Exists())
                        {
                            location = MapLocationDocument(ds);
                        }
                        tcs.SetResult(location);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"GetLocationByIdAsync({id}) failed: {ex.Message}");
                        tcs.SetResult(null);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetLocationByIdAsync({id}) exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<LocationModel>> SearchLocationsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return await GetAllLocationsAsync().ConfigureAwait(false);

                // Client-side search: fetch all and filter by name/tags
                var allLocations = await GetAllLocationsAsync().ConfigureAwait(false);
                var searchLower = query.ToLowerInvariant();

                var results = allLocations
                    .Where(loc =>
                        loc.Name.ToLowerInvariant().Contains(searchLower) ||
                        loc.Description.ToLowerInvariant().Contains(searchLower) ||
                        (loc.Tags?.Any(tag => tag.ToLowerInvariant().Contains(searchLower)) ?? false))
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"SearchLocationsAsync({query}) exception: {ex.Message}");
                return new List<LocationModel>();
            }
        }

        public async Task<List<EventItem>> GetUpcomingEventsAsync()
        {
            try
            {
                var events = new List<EventItem>();
                var tcs = new TaskCompletionSource<List<EventItem>>();

                GetFirestoreInstance().Collection(EventsCollection)
                    .WhereEqualTo("isPublished", true)
                    .Get()
                    .AddOnSuccessListener(new SuccessListener(snapshot =>
                    {
                        if (snapshot is QuerySnapshot qs)
                        {
                            foreach (var doc in qs.Documents)
                            {
                                var eventItem = MapEventDocument(doc);
                                if (eventItem != null)
                                    events.Add(eventItem);
                            }
                        }
                        tcs.SetResult(events);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"GetUpcomingEventsAsync failed: {ex.Message}");
                        tcs.SetResult(events);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetUpcomingEventsAsync exception: {ex.Message}");
                return new List<EventItem>();
            }
        }

        public async Task<List<NotificationItem>> GetNotificationsAsync()
        {
            try
            {
                var notifications = new List<NotificationItem>();
                var tcs = new TaskCompletionSource<List<NotificationItem>>();

                GetFirestoreInstance().Collection(NotificationsCollection)
                    .Get()
                    .AddOnSuccessListener(new SuccessListener(snapshot =>
                    {
                        if (snapshot is QuerySnapshot qs)
                        {
                            foreach (var doc in qs.Documents)
                            {
                                var notification = MapNotificationDocument(doc);
                                if (notification != null)
                                    notifications.Add(notification);
                            }
                        }
                        tcs.SetResult(notifications);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"GetNotificationsAsync failed: {ex.Message}");
                        tcs.SetResult(notifications);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetNotificationsAsync exception: {ex.Message}");
                return new List<NotificationItem>();
            }
        }

        // Mapping helpers
        private LocationModel? MapLocationDocument(DocumentSnapshot doc)
        {
            try
            {
                if (!doc.Exists())
                    return null;

                var id = doc.GetString("id") ?? doc.Id;
                Log.Debug(TAG, $"MapLocationDocument: doc.Id='{doc.Id}', field 'id'='{doc.GetString("id")}', resolved id='{id}'");

                var location = new LocationModel
                {
                    Id = id,
                    Name = doc.GetString("name") ?? string.Empty,
                    Description = doc.GetString("description") ?? string.Empty,
                    Type = doc.GetString("type") ?? string.Empty,
                    Floor = doc.GetString("floor") ?? string.Empty,
                    Building = doc.GetString("building") ?? string.Empty,
                    QrCodeId = doc.GetString("qrCodeId") ?? string.Empty,
                };

                // Map coordinates
                if (doc.Get("coordinates") is IDictionary<string, object> coordMap)
                {
                    location.Coordinates = new Coordinates
                    {
                        Latitude = Convert.ToDouble(coordMap.ContainsKey("latitude") ? coordMap["latitude"] : 0.0),
                        Longitude = Convert.ToDouble(coordMap.ContainsKey("longitude") ? coordMap["longitude"] : 0.0),
                    };
                }

                // Map images array
                if (doc.Get("images") is Java.Util.ArrayList imagesList)
                {
                    for (int i = 0; i < imagesList.Size(); i++)
                    {
                        var obj = imagesList.Get(i);
                        var img = obj != null ? obj.ToString() : null;
                        if (!string.IsNullOrEmpty(img))
                            location.Images.Add(img);
                    }
                }

                // Map tags array
                if (doc.Get("tags") is Java.Util.ArrayList tagsList)
                {
                    for (int i = 0; i < tagsList.Size(); i++)
                    {
                        var obj = tagsList.Get(i);
                        var tag = obj != null ? obj.ToString() : null;
                        if (!string.IsNullOrEmpty(tag))
                            location.Tags.Add(tag);
                    }
                }

                return location;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"MapLocationDocument failed: {ex.Message}");
                return null;
            }
        }

        private EventItem? MapEventDocument(DocumentSnapshot doc)
        {
            try
            {
                if (!doc.Exists())
                    return null;

                var eventItem = new EventItem
                {
                    Id = doc.GetString("id") ?? doc.Id,
                    Title = doc.GetString("title") ?? string.Empty,
                    Description = doc.GetString("description") ?? string.Empty,
                    LocationId = doc.GetString("locationId") ?? string.Empty,
                    ImageUrl = doc.GetString("imageUrl") ?? string.Empty,
                    IsPublished = doc.GetBoolean("isPublished") != null ? doc.GetBoolean("isPublished").BooleanValue() : false,
                };

                // Map timestamps
                // Timestamps from Firestore are returned as Java.Lang.Object; use GetTimestamp and check
                var startObj = doc.GetTimestamp("startTime");
                if (startObj != null)
                {
                    try
                    {
                        var prop = startObj.GetType().GetProperty("Seconds")?.GetValue(startObj);
                        long seconds = prop != null ? Convert.ToInt64(prop) : 0L;
                        eventItem.StartTime = UnixTimeStampToDateTime(seconds);
                    }
                    catch { }
                }

                var endObj = doc.GetTimestamp("endTime");
                if (endObj != null)
                {
                    try
                    {
                        var prop = endObj.GetType().GetProperty("Seconds")?.GetValue(endObj);
                        long seconds = prop != null ? Convert.ToInt64(prop) : 0L;
                        eventItem.EndTime = UnixTimeStampToDateTime(seconds);
                    }
                    catch { }
                }

                // Map coordinates
                if (doc.Get("coordinates") is IDictionary<string, object> coordMap)
                {
                    eventItem.Coordinates = new Coordinates
                    {
                        Latitude = Convert.ToDouble(coordMap.ContainsKey("latitude") ? coordMap["latitude"] : 0.0),
                        Longitude = Convert.ToDouble(coordMap.ContainsKey("longitude") ? coordMap["longitude"] : 0.0),
                    };
                }

                return eventItem;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"MapEventDocument failed: {ex.Message}");
                return null;
            }
        }

        private NotificationItem? MapNotificationDocument(DocumentSnapshot doc)
        {
            try
            {
                if (!doc.Exists())
                    return null;

                var notification = new NotificationItem
                {
                    Id = doc.GetString("id") ?? doc.Id,
                    Title = doc.GetString("title") ?? string.Empty,
                    Body = doc.GetString("body") ?? string.Empty,
                    Type = doc.GetString("type") ?? string.Empty,
                    RelatedEventId = doc.GetString("relatedEventId") ?? string.Empty,
                    RelatedLocationId = doc.GetString("relatedLocationId") ?? string.Empty,
                };

                // Map timestamp (interop-safe)
                var createdObj = doc.GetTimestamp("createdAt");
                if (createdObj != null)
                {
                    try
                    {
                        var prop = createdObj.GetType().GetProperty("Seconds")?.GetValue(createdObj);
                        long seconds = prop != null ? Convert.ToInt64(prop) : 0L;
                        notification.CreatedAt = UnixTimeStampToDateTime(seconds);
                    }
                    catch { }
                }

                return notification;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"MapNotificationDocument failed: {ex.Message}");
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long seconds)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(seconds).ToLocalTime();
            return dateTime;
        }

        /// <summary>
        /// Adds a single location to Firestore.
        /// </summary>
        public async Task<bool> AddLocationAsync(LocationModel location)
        {
            try
            {
                if (string.IsNullOrEmpty(location?.Id))
                {
                    Log.Error(TAG, "Cannot add location with null or empty ID");
                    return false;
                }

                var tcs = new TaskCompletionSource<bool>();

                var locationData = new Java.Util.HashMap();
                locationData.Put("id", location.Id);
                locationData.Put("name", location.Name ?? "");
                locationData.Put("description", location.Description ?? "");
                locationData.Put("type", location.Type ?? "");
                locationData.Put("building", location.Building ?? "");
                locationData.Put("floor", location.Floor ?? "");
                locationData.Put("qrCodeId", location.QrCodeId ?? "");

                // Add coordinates
                var coordinates = new Java.Util.HashMap();
                coordinates.Put("latitude", location.Coordinates?.Latitude ?? 0.0);
                coordinates.Put("longitude", location.Coordinates?.Longitude ?? 0.0);
                locationData.Put("coordinates", coordinates);

                // Add images
                var images = new Java.Util.ArrayList();
                if (location.Images != null)
                {
                    foreach (var img in location.Images)
                        images.Add(img);
                }
                locationData.Put("images", images);

                // Add tags
                var tags = new Java.Util.ArrayList();
                if (location.Tags != null)
                {
                    foreach (var tag in location.Tags)
                        tags.Add(tag);
                }
                locationData.Put("tags", tags);

                GetFirestoreInstance()
                    .Collection(LocationsCollection)
                    .Document(location.Id)
                    .Set(locationData)
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"Location '{location.Name}' (ID: {location.Id}) added successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"AddLocationAsync failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"AddLocationAsync exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds multiple locations to Firestore in batch.
        /// </summary>
        public async Task<bool> AddLocationsAsync(List<LocationModel> locations)
        {
            try
            {
                if (locations == null || locations.Count == 0)
                {
                    Log.Warn(TAG, "No locations to add");
                    return true;
                }

                var tcs = new TaskCompletionSource<bool>();
                var batch = GetFirestoreInstance().Batch();
                var count = 0;

                foreach (var location in locations)
                {
                    if (string.IsNullOrEmpty(location?.Id))
                        continue;

                    var locationData = new Java.Util.HashMap();
                    locationData.Put("id", location.Id);
                    locationData.Put("name", location.Name ?? "");
                    locationData.Put("description", location.Description ?? "");
                    locationData.Put("type", location.Type ?? "");
                    locationData.Put("building", location.Building ?? "");
                    locationData.Put("floor", location.Floor ?? "");
                    locationData.Put("qrCodeId", location.QrCodeId ?? "");

                    // Add coordinates
                    var coordinates = new Java.Util.HashMap();
                    coordinates.Put("latitude", location.Coordinates?.Latitude ?? 0.0);
                    coordinates.Put("longitude", location.Coordinates?.Longitude ?? 0.0);
                    locationData.Put("coordinates", coordinates);

                    // Add images
                    var images = new Java.Util.ArrayList();
                    if (location.Images != null)
                    {
                        foreach (var img in location.Images)
                            images.Add(img);
                    }
                    locationData.Put("images", images);

                    // Add tags
                    var tags = new Java.Util.ArrayList();
                    if (location.Tags != null)
                    {
                        foreach (var tag in location.Tags)
                            tags.Add(tag);
                    }
                    locationData.Put("tags", tags);

                    batch.Set(GetFirestoreInstance().Collection(LocationsCollection).Document(location.Id), locationData);
                    count++;
                }

                batch.Commit()
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"Batch added {count} locations successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"AddLocationsAsync batch failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"AddLocationsAsync exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds multiple events to Firestore in batch.
        /// </summary>
        public async Task<bool> AddEventsAsync(List<EventItem> events)
        {
            try
            {
                if (events == null || events.Count == 0)
                {
                    Log.Warn(TAG, "No events to add");
                    return true;
                }

                var tcs = new TaskCompletionSource<bool>();
                var batch = GetFirestoreInstance().Batch();
                var count = 0;

                foreach (var eventItem in events)
                {
                    if (string.IsNullOrEmpty(eventItem?.Id))
                        continue;

                    var eventData = new Java.Util.HashMap();
                    eventData.Put("id", eventItem.Id);
                    eventData.Put("title", eventItem.Title ?? "");
                    eventData.Put("description", eventItem.Description ?? "");
                    eventData.Put("locationId", eventItem.LocationId ?? "");
                    eventData.Put("imageUrl", eventItem.ImageUrl ?? "");
                    eventData.Put("isPublished", Java.Lang.Boolean.ValueOf(eventItem.IsPublished));
                    eventData.Put("startTime", ToJavaDate(eventItem.StartTime));
                    eventData.Put("endTime", ToJavaDate(eventItem.EndTime));

                    var coordinates = new Java.Util.HashMap();
                    coordinates.Put("latitude", eventItem.Coordinates?.Latitude ?? 0.0);
                    coordinates.Put("longitude", eventItem.Coordinates?.Longitude ?? 0.0);
                    eventData.Put("coordinates", coordinates);

                    batch.Set(GetFirestoreInstance().Collection(EventsCollection).Document(eventItem.Id), eventData);
                    count++;
                }

                batch.Commit()
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"Batch added {count} events successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"AddEventsAsync batch failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"AddEventsAsync exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds multiple notifications to Firestore in batch.
        /// </summary>
        public async Task<bool> AddNotificationsAsync(List<NotificationItem> notifications)
        {
            try
            {
                if (notifications == null || notifications.Count == 0)
                {
                    Log.Warn(TAG, "No notifications to add");
                    return true;
                }

                var tcs = new TaskCompletionSource<bool>();
                var batch = GetFirestoreInstance().Batch();
                var count = 0;

                foreach (var notification in notifications)
                {
                    if (string.IsNullOrEmpty(notification?.Id))
                        continue;

                    var notificationData = new Java.Util.HashMap();
                    notificationData.Put("id", notification.Id);
                    notificationData.Put("title", notification.Title ?? "");
                    notificationData.Put("body", notification.Body ?? "");
                    notificationData.Put("type", notification.Type ?? "");
                    notificationData.Put("relatedEventId", notification.RelatedEventId ?? "");
                    notificationData.Put("relatedLocationId", notification.RelatedLocationId ?? "");
                    notificationData.Put("createdAt", ToJavaDate(notification.CreatedAt));

                    batch.Set(GetFirestoreInstance().Collection(NotificationsCollection).Document(notification.Id), notificationData);
                    count++;
                }

                batch.Commit()
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"Batch added {count} notifications successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"AddNotificationsAsync batch failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"AddNotificationsAsync exception: {ex.Message}");
                return false;
            }
        }

        private static Java.Util.Date ToJavaDate(DateTime value)
        {
            return new Java.Util.Date(new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeMilliseconds());
        }

        /// <summary>
        /// Deletes a single location from Firestore.
        /// </summary>
        public async Task<bool> DeleteLocationAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    Log.Error(TAG, "Cannot delete location with null or empty ID");
                    return false;
                }

                var tcs = new TaskCompletionSource<bool>();

                GetFirestoreInstance()
                    .Collection(LocationsCollection)
                    .Document(id)
                    .Delete()
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"Location '{id}' deleted successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"DeleteLocationAsync failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"DeleteLocationAsync exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes all locations from Firestore.
        /// Used for clearing old test data before re-seeding.
        /// </summary>
        public async Task<bool> DeleteAllLocationsAsync()
        {
            try
            {
                Log.Debug(TAG, "Deleting all locations from Firestore...");

                var locations = await GetAllLocationsAsync().ConfigureAwait(false);
                if (locations.Count == 0)
                {
                    Log.Debug(TAG, "No locations to delete");
                    return true;
                }

                var tcs = new TaskCompletionSource<bool>();
                var batch = GetFirestoreInstance().Batch();
                var count = 0;

                foreach (var location in locations)
                {
                    batch.Delete(GetFirestoreInstance().Collection(LocationsCollection).Document(location.Id));
                    count++;
                }

                batch.Commit()
                    .AddOnSuccessListener(new SuccessListener(_ =>
                    {
                        Log.Debug(TAG, $"✅ Deleted {count} locations successfully");
                        tcs.SetResult(true);
                    }))
                    .AddOnFailureListener(new FailureListener(ex =>
                    {
                        Log.Error(TAG, $"DeleteAllLocationsAsync batch failed: {ex.Message}");
                        tcs.SetResult(false);
                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"DeleteAllLocationsAsync exception: {ex.Message}");
                return false;
            }
        }
    }

    #region Firestore Callback Listeners

    /// <summary>
    /// Helper class wrapping IOnSuccessListener for TaskCompletionSource integration.
    /// </summary>
    internal class SuccessListener : Java.Lang.Object, IOnSuccessListener
    {
        private readonly Action<Java.Lang.Object?> _onSuccessAction;

        public SuccessListener(Action<Java.Lang.Object?> onSuccessAction)
        {
            _onSuccessAction = onSuccessAction;
        }

        public void OnSuccess(Java.Lang.Object? result)
        {
            _onSuccessAction?.Invoke(result);
        }
    }

    /// <summary>
    /// Helper class wrapping IOnFailureListener for TaskCompletionSource integration.
    /// </summary>
    internal class FailureListener : Java.Lang.Object, IOnFailureListener
    {
        private readonly Action<Java.Lang.Exception> _onFailureAction;

        public FailureListener(Action<Java.Lang.Exception> onFailureAction)
        {
            _onFailureAction = onFailureAction;
        }

        public void OnFailure(Java.Lang.Exception ex)
        {
            _onFailureAction?.Invoke(ex);
        }
    }

    #endregion
#else
    /// <summary>
    /// Stub Firestore implementation for non-Android platforms.
    /// Real implementation only available on Android with native Firebase bindings.
    /// </summary>
    public class FirebaseFirestoreService : IFirestoreService
    {
        public Task<List<LocationModel>> GetAllLocationsAsync() => Task.FromResult(new List<LocationModel>());
        public Task<LocationModel?> GetLocationByIdAsync(string id) => Task.FromResult<LocationModel?>(null);
        public Task<List<LocationModel>> SearchLocationsAsync(string query) => Task.FromResult(new List<LocationModel>());
        public Task<List<EventItem>> GetUpcomingEventsAsync() => Task.FromResult(new List<EventItem>());
        public Task<List<NotificationItem>> GetNotificationsAsync() => Task.FromResult(new List<NotificationItem>());
        public Task<bool> AddLocationAsync(LocationModel location) => Task.FromResult(false);
        public Task<bool> AddLocationsAsync(List<LocationModel> locations) => Task.FromResult(false);
        public Task<bool> AddEventsAsync(List<EventItem> events) => Task.FromResult(false);
        public Task<bool> AddNotificationsAsync(List<NotificationItem> notifications) => Task.FromResult(false);
        public Task<bool> DeleteLocationAsync(string id) => Task.FromResult(false);
        public Task<bool> DeleteAllLocationsAsync() => Task.FromResult(false);
    }
#endif
}
