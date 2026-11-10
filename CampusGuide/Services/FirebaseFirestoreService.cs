using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

#if ANDROID
using Android.Util;
#endif

namespace CampusGuide.Services
{
    /// <summary>
    /// Firestore implementation using the Firestore REST API (no native Firebase bindings).
    /// Maps Firestore collections (locations, events, notifications) to CampusGuide models.
    /// </summary>
    public class FirebaseFirestoreService : IFirestoreService
    {
        const string TAG = "FirebaseFirestoreService";
        const string ProjectId = "campusguide-d1939";
        const string ApiKey = "AIzaSyAI2w4xEt1N8veN6W-dnlbkj9LtFpAJon8";
        const string LocationsCollection = "locations";
        const string EventsCollection = "events";
        const string NotificationsCollection = "notifications";

        const string DocumentsRoot = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

        const string AuthSignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";

        static readonly HttpClient Http = CreateHttpClient();
        static readonly SemaphoreSlim AuthLock = new(1, 1);
        static string? idToken;
        static DateTime idTokenExpiresUtc;

        static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Android-Package", "com.companyname.campusguide");
            client.DefaultRequestHeaders.Add("X-Android-Cert", "14FC0872F47A1AF958140D5019CBAA6C1512AEE0");
            return client;
        }

        static async Task<string> GetIdTokenAsync()
        {
            if (idToken != null && DateTime.UtcNow < idTokenExpiresUtc)
                return idToken;

            await AuthLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (idToken != null && DateTime.UtcNow < idTokenExpiresUtc)
                    return idToken;

                var body = new JsonObject { ["returnSecureToken"] = true };
                using var request = new HttpRequestMessage(HttpMethod.Post, AuthSignUpUrl + ApiKey)
                {
                    Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
                };

                using var response = await Http.SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Firebase anonymous sign-in failed: {(int)response.StatusCode} {content}");

                var json = JsonNode.Parse(content)!;
                idToken = json["idToken"]!.GetValue<string>();
                var expiresIn = int.Parse(json["expiresIn"]!.GetValue<string>());
                idTokenExpiresUtc = DateTime.UtcNow.AddSeconds(expiresIn - 60);
                return idToken;
            }
            finally
            {
                AuthLock.Release();
            }
        }

        static async Task AddAuthorizationAsync(HttpRequestMessage request)
        {
            var token = await GetIdTokenAsync().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        static void LogError(string message)
        {
#if ANDROID
            Log.Error(TAG, message);
#endif
        }

        static void LogDebug(string message)
        {
#if ANDROID
            Log.Debug(TAG, message);
#endif
        }

        static async Task<JsonNode?> SendAsync(HttpMethod method, string path, JsonObject? body = null)
        {
            using var request = new HttpRequestMessage(method, $"{DocumentsRoot}/{path}");
            if (body != null)
            {
                request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            }
            await AddAuthorizationAsync(request).ConfigureAwait(false);

            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Firestore {method} {path} failed: {(int)response.StatusCode} {content}");

            return string.IsNullOrWhiteSpace(content) ? null : JsonNode.Parse(content);
        }

        static async Task<List<JsonNode>> QueryCollectionAsync(string collection, JsonObject? where = null)
        {
            var results = new List<JsonNode>();
            var query = new JsonObject
            {
                ["parent"] = $"projects/{ProjectId}/databases/(default)/documents",
                ["structuredQuery"] = new JsonObject
                {
                    ["from"] = new JsonArray(new JsonObject { ["collectionId"] = collection }),
                }
            };
            if (where != null)
                query["structuredQuery"]!["where"] = where;

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{DocumentsRoot}:runQuery")
            {
                Content = new StringContent(query.ToJsonString(), Encoding.UTF8, "application/json")
            };
            await AddAuthorizationAsync(request).ConfigureAwait(false);

            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Firestore query on '{collection}' failed: {(int)response.StatusCode} {content}");

            if (JsonNode.Parse(content) is JsonArray rows)
            {
                foreach (var row in rows)
                {
                    if (row?["document"] is JsonNode document)
                        results.Add(document);
                }
            }
            return results;
        }

        static string DocumentIdOf(JsonNode document)
        {
            var name = document["name"]?.GetValue<string>() ?? string.Empty;
            return name.Substring(name.LastIndexOf('/') + 1);
        }

        static JsonObject PublishedFilter() => new()
        {
            ["fieldFilter"] = new JsonObject
            {
                ["field"] = new JsonObject { ["fieldPath"] = "isPublished" },
                ["op"] = "EQUAL",
                ["value"] = new JsonObject { ["booleanValue"] = true }
            }
        };

        static string? GetString(JsonNode? fields, string key)
        {
            var value = fields?[key];
            return value?["stringValue"]?.GetValue<string>();
        }

        static bool GetBool(JsonNode? fields, string key)
        {
            return fields?[key]?["booleanValue"]?.GetValue<bool>() ?? false;
        }

        static DateTime? GetTimestamp(JsonNode? fields, string key)
        {
            var text = fields?[key]?["timestampValue"]?.GetValue<string>();
            if (text == null)
                return null;
            return DateTime.Parse(text, null, System.Globalization.DateTimeStyles.RoundtripKind).ToLocalTime();
        }

        static Coordinates? GetCoordinates(JsonNode? fields)
        {
            var map = fields?["coordinates"]?["mapValue"]?["fields"];
            if (map == null)
                return null;
            return new Coordinates
            {
                Latitude = GetDouble(map, "latitude"),
                Longitude = GetDouble(map, "longitude"),
            };
        }

        static double GetDouble(JsonNode? fields, string key)
        {
            var value = fields?[key];
            if (value?["doubleValue"] is JsonNode d)
                return d.GetValue<double>();
            if (value?["integerValue"] is JsonNode i)
                return double.Parse(i.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);
            return 0.0;
        }

        static List<string> GetStringArray(JsonNode? fields, string key)
        {
            var result = new List<string>();
            if (fields?[key]?["arrayValue"]?["values"] is JsonArray values)
            {
                foreach (var v in values)
                {
                    var s = v?["stringValue"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(s))
                        result.Add(s);
                }
            }
            return result;
        }

        static JsonObject StringValue(string value) => new() { ["stringValue"] = value ?? string.Empty };

        static JsonObject BoolValue(bool value) => new() { ["booleanValue"] = value };

        static JsonObject DoubleValue(double value) => new() { ["doubleValue"] = value };

        static JsonObject TimestampValue(DateTime value) => new()
        {
            ["timestampValue"] = value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture)
        };

        static JsonObject ArrayValue(IEnumerable<string> values)
        {
            var array = new JsonArray();
            foreach (var v in values)
                array.Add(StringValue(v));
            return new JsonObject { ["arrayValue"] = new JsonObject { ["values"] = array } };
        }

        static JsonObject MapValue(JsonObject fields) => new() { ["mapValue"] = new JsonObject { ["fields"] = fields } };

        static JsonObject CoordinatesValue(Coordinates? coordinates) => MapValue(new JsonObject
        {
            ["latitude"] = DoubleValue(coordinates?.Latitude ?? 0.0),
            ["longitude"] = DoubleValue(coordinates?.Longitude ?? 0.0),
        });

        static JsonObject LocationFields(LocationModel location) => new()
        {
            ["id"] = StringValue(location.Id),
            ["name"] = StringValue(location.Name),
            ["description"] = StringValue(location.Description),
            ["type"] = StringValue(location.Type),
            ["building"] = StringValue(location.Building),
            ["floor"] = StringValue(location.Floor),
            ["qrCodeId"] = StringValue(location.QrCodeId),
            ["coordinates"] = CoordinatesValue(location.Coordinates),
            ["images"] = ArrayValue(location.Images ?? new List<string>()),
            ["tags"] = ArrayValue(location.Tags ?? new List<string>()),
        };

        static JsonObject EventFields(EventItem eventItem) => new()
        {
            ["id"] = StringValue(eventItem.Id),
            ["title"] = StringValue(eventItem.Title),
            ["description"] = StringValue(eventItem.Description),
            ["locationId"] = StringValue(eventItem.LocationId),
            ["imageUrl"] = StringValue(eventItem.ImageUrl),
            ["isPublished"] = BoolValue(eventItem.IsPublished),
            ["startTime"] = TimestampValue(eventItem.StartTime),
            ["endTime"] = TimestampValue(eventItem.EndTime),
            ["coordinates"] = CoordinatesValue(eventItem.Coordinates),
        };

        static JsonObject NotificationFields(NotificationItem notification) => new()
        {
            ["id"] = StringValue(notification.Id),
            ["title"] = StringValue(notification.Title),
            ["body"] = StringValue(notification.Body),
            ["type"] = StringValue(notification.Type),
            ["relatedEventId"] = StringValue(notification.RelatedEventId),
            ["relatedLocationId"] = StringValue(notification.RelatedLocationId),
            ["createdAt"] = TimestampValue(notification.CreatedAt),
        };

        static JsonObject Document(JsonObject fields) => new() { ["fields"] = fields };

        static async Task<bool> SetDocumentAsync(string collection, string id, JsonObject fields)
        {
            var path = $"{collection}/{Uri.EscapeDataString(id)}";
            await SendAsync(HttpMethod.Patch, path, Document(fields)).ConfigureAwait(false);
            return true;
        }

        static async Task<bool> CommitWritesAsync(IEnumerable<JsonObject> writes)
        {
            var writeArray = new JsonArray();
            foreach (var w in writes)
                writeArray.Add(w);

            if (writeArray.Count == 0)
                return true;

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{DocumentsRoot}:commit")
            {
                Content = new StringContent(new JsonObject { ["writes"] = writeArray }.ToJsonString(), Encoding.UTF8, "application/json")
            };
            await AddAuthorizationAsync(request).ConfigureAwait(false);

            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            return true;
        }

        static JsonObject UpdateWrite(string collection, string id, JsonObject fields) => new()
        {
            ["update"] = new JsonObject
            {
                ["name"] = $"projects/{ProjectId}/databases/(default)/documents/{collection}/{id}",
                ["fields"] = fields,
            }
        };

        static JsonObject DeleteWrite(string collection, string id) => new()
        {
            ["delete"] = $"projects/{ProjectId}/databases/(default)/documents/{collection}/{id}"
        };

        static LocationModel? MapLocation(JsonNode document)
        {
            var fields = document["fields"];
            return new LocationModel
            {
                Id = GetString(fields, "id") ?? DocumentIdOf(document),
                Name = GetString(fields, "name") ?? string.Empty,
                Description = GetString(fields, "description") ?? string.Empty,
                Type = GetString(fields, "type") ?? string.Empty,
                Floor = GetString(fields, "floor") ?? string.Empty,
                Building = GetString(fields, "building") ?? string.Empty,
                QrCodeId = GetString(fields, "qrCodeId") ?? string.Empty,
                Coordinates = GetCoordinates(fields),
                Images = GetStringArray(fields, "images"),
                Tags = GetStringArray(fields, "tags"),
            };
        }

        static EventItem MapEvent(JsonNode document)
        {
            var fields = document["fields"];
            return new EventItem
            {
                Id = GetString(fields, "id") ?? DocumentIdOf(document),
                Title = GetString(fields, "title") ?? string.Empty,
                Description = GetString(fields, "description") ?? string.Empty,
                LocationId = GetString(fields, "locationId") ?? string.Empty,
                ImageUrl = GetString(fields, "imageUrl") ?? string.Empty,
                IsPublished = GetBool(fields, "isPublished"),
                StartTime = GetTimestamp(fields, "startTime") ?? default,
                EndTime = GetTimestamp(fields, "endTime") ?? default,
                Coordinates = GetCoordinates(fields),
            };
        }

        static NotificationItem MapNotification(JsonNode document)
        {
            var fields = document["fields"];
            return new NotificationItem
            {
                Id = GetString(fields, "id") ?? DocumentIdOf(document),
                Title = GetString(fields, "title") ?? string.Empty,
                Body = GetString(fields, "body") ?? string.Empty,
                Type = GetString(fields, "type") ?? string.Empty,
                RelatedEventId = GetString(fields, "relatedEventId") ?? string.Empty,
                RelatedLocationId = GetString(fields, "relatedLocationId") ?? string.Empty,
                CreatedAt = GetTimestamp(fields, "createdAt") ?? default,
            };
        }

        public async Task<List<LocationModel>> GetAllLocationsAsync()
        {
            var documents = await QueryCollectionAsync(LocationsCollection).ConfigureAwait(false);
            return documents.Select(MapLocation).OfType<LocationModel>().ToList();
        }

        public async Task<LocationModel?> GetLocationByIdAsync(string id)
        {
            try
            {
                var document = await SendAsync(HttpMethod.Get, $"{LocationsCollection}/{Uri.EscapeDataString(id)}").ConfigureAwait(false);
                return document == null ? null : MapLocation(document);
            }
            catch (Exception ex)
            {
                LogError($"GetLocationByIdAsync({id}) exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<LocationModel>> SearchLocationsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return await GetAllLocationsAsync().ConfigureAwait(false);

                var allLocations = await GetAllLocationsAsync().ConfigureAwait(false);
                var searchLower = query.ToLowerInvariant();

                return allLocations
                    .Where(loc =>
                        loc.Name.ToLowerInvariant().Contains(searchLower) ||
                        loc.Description.ToLowerInvariant().Contains(searchLower) ||
                        (loc.Tags?.Any(tag => tag.ToLowerInvariant().Contains(searchLower)) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError($"SearchLocationsAsync({query}) exception: {ex.Message}");
                return new List<LocationModel>();
            }
        }

        public async Task<List<EventItem>> GetUpcomingEventsAsync()
        {
            try
            {
                var documents = await QueryCollectionAsync(EventsCollection, PublishedFilter()).ConfigureAwait(false);
                return documents.Select(MapEvent).ToList();
            }
            catch (Exception ex)
            {
                LogError($"GetUpcomingEventsAsync exception: {ex.Message}");
                return new List<EventItem>();
            }
        }

        public async Task<List<NotificationItem>> GetNotificationsAsync()
        {
            try
            {
                var documents = await QueryCollectionAsync(NotificationsCollection).ConfigureAwait(false);
                return documents.Select(MapNotification).ToList();
            }
            catch (Exception ex)
            {
                LogError($"GetNotificationsAsync exception: {ex.Message}");
                return new List<NotificationItem>();
            }
        }

        public async Task<bool> AddLocationAsync(LocationModel location)
        {
            try
            {
                if (string.IsNullOrEmpty(location?.Id))
                {
                    LogError("Cannot add location with null or empty ID");
                    return false;
                }

                await SetDocumentAsync(LocationsCollection, location.Id, LocationFields(location)).ConfigureAwait(false);
                LogDebug($"Location '{location.Name}' (ID: {location.Id}) added successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"AddLocationAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddLocationsAsync(List<LocationModel> locations)
        {
            try
            {
                if (locations == null || locations.Count == 0)
                    return true;

                var writes = locations
                    .Where(l => !string.IsNullOrEmpty(l?.Id))
                    .Select(l => UpdateWrite(LocationsCollection, l.Id, LocationFields(l)));

                var result = await CommitWritesAsync(writes).ConfigureAwait(false);
                LogDebug($"Batch added {locations.Count} locations successfully");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"AddLocationsAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddEventsAsync(List<EventItem> events)
        {
            try
            {
                if (events == null || events.Count == 0)
                    return true;

                var writes = events
                    .Where(e => !string.IsNullOrEmpty(e?.Id))
                    .Select(e => UpdateWrite(EventsCollection, e.Id, EventFields(e)));

                var result = await CommitWritesAsync(writes).ConfigureAwait(false);
                LogDebug($"Batch added {events.Count} events successfully");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"AddEventsAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddNotificationsAsync(List<NotificationItem> notifications)
        {
            try
            {
                if (notifications == null || notifications.Count == 0)
                    return true;

                var writes = notifications
                    .Where(n => !string.IsNullOrEmpty(n?.Id))
                    .Select(n => UpdateWrite(NotificationsCollection, n.Id, NotificationFields(n)));

                var result = await CommitWritesAsync(writes).ConfigureAwait(false);
                LogDebug($"Batch added {notifications.Count} notifications successfully");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"AddNotificationsAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteLocationAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    LogError("Cannot delete location with null or empty ID");
                    return false;
                }

                await SendAsync(HttpMethod.Delete, $"{LocationsCollection}/{Uri.EscapeDataString(id)}").ConfigureAwait(false);
                LogDebug($"Location '{id}' deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"DeleteLocationAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllLocationsAsync()
        {
            try
            {
                LogDebug("Deleting all locations from Firestore...");

                var locations = await GetAllLocationsAsync().ConfigureAwait(false);
                if (locations.Count == 0)
                {
                    LogDebug("No locations to delete");
                    return true;
                }

                var writes = locations.Select(l => DeleteWrite(LocationsCollection, l.Id));
                var result = await CommitWritesAsync(writes).ConfigureAwait(false);
                LogDebug($"Deleted {locations.Count} locations successfully");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"DeleteAllLocationsAsync exception: {ex.Message}");
                return false;
            }
        }
    }
}
