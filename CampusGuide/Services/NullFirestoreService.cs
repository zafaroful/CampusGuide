using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
    /// <summary>
    /// A null implementation of IFirestoreService that returns empty data.
    /// Used for testing without Firebase configuration (no google-services.json).
    /// This is a temporary testing service and should be replaced with FirebaseFirestoreService
    /// for production use.
    /// </summary>
    public class NullFirestoreService : IFirestoreService
    {
        public Task<List<LocationModel>> GetAllLocationsAsync()
        {
            return Task.FromResult<List<LocationModel>>(new List<LocationModel>());
        }

        public Task<LocationModel?> GetLocationByIdAsync(string id)
        {
            return Task.FromResult<LocationModel?>(null);
        }

        public Task<List<LocationModel>> SearchLocationsAsync(string query)
        {
            return Task.FromResult<List<LocationModel>>(new List<LocationModel>());
        }

        public Task<List<EventItem>> GetUpcomingEventsAsync()
        {
            return Task.FromResult<List<EventItem>>(new List<EventItem>());
        }

        public Task<List<NotificationItem>> GetNotificationsAsync()
        {
            return Task.FromResult<List<NotificationItem>>(new List<NotificationItem>());
        }

        public Task<bool> AddLocationAsync(LocationModel location)
        {
            return Task.FromResult(false);
        }

        public Task<bool> AddLocationsAsync(List<LocationModel> locations)
        {
            return Task.FromResult(false);
        }

        public Task<bool> AddEventsAsync(List<EventItem> events)
        {
            return Task.FromResult(false);
        }

        public Task<bool> AddNotificationsAsync(List<NotificationItem> notifications)
        {
            return Task.FromResult(false);
        }

        public Task<bool> DeleteAllLocationsAsync()
        {
            return Task.FromResult(false);
        }

        public Task<bool> DeleteLocationAsync(string id)
        {
            return Task.FromResult(false);
        }
    }
}
