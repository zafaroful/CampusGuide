using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
    public interface IFirestoreService
    {
        // Read operations
        Task<List<LocationModel>> GetAllLocationsAsync();
        Task<LocationModel?> GetLocationByIdAsync(string id);
        Task<List<LocationModel>> SearchLocationsAsync(string query);
        Task<List<EventItem>> GetUpcomingEventsAsync();
        Task<List<NotificationItem>> GetNotificationsAsync();

        // Write operations
        Task<bool> AddLocationAsync(LocationModel location);
        Task<bool> AddLocationsAsync(List<LocationModel> locations);
        Task<bool> AddEventsAsync(List<EventItem> events);
        Task<bool> AddNotificationsAsync(List<NotificationItem> notifications);

        // Delete operations
        Task<bool> DeleteAllLocationsAsync();
        Task<bool> DeleteLocationAsync(string id);
    }
}
