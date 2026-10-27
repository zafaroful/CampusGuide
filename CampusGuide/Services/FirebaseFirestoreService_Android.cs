using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
#if ANDROID
    // Android-specific Firestore implementation placeholder.
    public class FirebaseFirestoreService_Android : IFirestoreService
    {
        public Task<List<LocationModel>> GetAllLocationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LocationModel?> GetLocationByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<LocationModel>> SearchLocationsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public Task<List<EventItem>> GetUpcomingEventsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<NotificationItem>> GetNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }
#endif
}
