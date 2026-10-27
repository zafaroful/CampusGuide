using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
    public interface IFirestoreService
    {
        Task<List<LocationModel>> GetAllLocationsAsync();
        Task<LocationModel?> GetLocationByIdAsync(string id);
        Task<List<LocationModel>> SearchLocationsAsync(string query);
        Task<List<EventItem>> GetUpcomingEventsAsync();
        Task<List<NotificationItem>> GetNotificationsAsync();
    }
}
