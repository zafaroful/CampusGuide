using System.Collections.Generic;
using System.Threading.Tasks;
using CampusGuide.Models;

namespace CampusGuide.Services
{
    /// <summary>
    /// Service for managing notifications and in-app alerts/reminders.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Displays alert notifications to the user via in-app popups.
        /// </summary>
        void DisplayAlerts(List<NotificationItem> alerts);

        /// <summary>
        /// Displays a single notification as an in-app alert.
        /// </summary>
        Task DisplayNotificationAlertAsync(NotificationItem notification);

        /// <summary>
        /// Schedules a reminder for a notification at a specific time.
        /// </summary>
        Task ScheduleReminderAsync(NotificationItem notification, DateTime reminderTime);

        /// <summary>
        /// Cancels a scheduled reminder for a notification.
        /// </summary>
        Task CancelReminderAsync(string notificationId);

        /// <summary>
        /// Gets notifications that are due for reminder display.
        /// </summary>
        Task<List<NotificationItem>> GetDueRemindersAsync();

        /// <summary>
        /// Checks if a notification should be shown as an alert based on type.
        /// </summary>
        bool IsAlertNotification(NotificationItem notification);
    }
}
