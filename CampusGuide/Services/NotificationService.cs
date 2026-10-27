using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusGuide.Models;
using Microsoft.Maui.Controls;

namespace CampusGuide.Services
{
    /// <summary>
    /// Service for managing notifications and in-app alerts/reminders.
    /// Handles displaying alerts and managing reminder scheduling.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly Dictionary<string, DateTime> _scheduledReminders = new();
        private const string ALERT_TYPE = "Alert";
        private const string REMINDER_TYPE = "Reminder";
        private const string EVENT_TYPE = "Event";

        /// <summary>
        /// Displays alert notifications to the user via in-app popups.
        /// Sequentially shows each alert to avoid overwhelming the user.
        /// </summary>
        public void DisplayAlerts(List<NotificationItem> alerts)
        {
            if (alerts?.Count == 0)
                return;

            // Display alerts one by one on the main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                foreach (var alert in alerts)
                {
                    await DisplayNotificationAlertAsync(alert).ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// Displays a single notification as an in-app alert.
        /// </summary>
        public async Task DisplayNotificationAlertAsync(NotificationItem notification)
        {
            if (notification == null)
                return;

            try
            {
                var windows = Application.Current?.Windows;
                if (windows == null || windows.Count == 0)
                    return;

                var window = windows.FirstOrDefault();
                var page = window?.Page;

                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        notification.Title ?? "Notification",
                        notification.Body ?? "You have a new notification",
                        "OK"
                    ).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to display notification alert: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedules a reminder for a notification at a specific time.
        /// </summary>
        public async Task ScheduleReminderAsync(NotificationItem notification, DateTime reminderTime)
        {
            if (notification == null)
                return;

            try
            {
                _scheduledReminders[notification.Id] = reminderTime;

                // In a production app, you would use platform-specific APIs for background notifications
                // For now, this stores the reminder time and can be checked periodically

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to schedule reminder: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels a scheduled reminder for a notification.
        /// </summary>
        public async Task CancelReminderAsync(string notificationId)
        {
            try
            {
                if (!string.IsNullOrEmpty(notificationId))
                {
                    _scheduledReminders.Remove(notificationId);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cancel reminder: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets notifications that are due for reminder display.
        /// Checks scheduled reminders against the current time.
        /// </summary>
        public async Task<List<NotificationItem>> GetDueRemindersAsync()
        {
            var dueReminders = new List<NotificationItem>();
            var now = DateTime.UtcNow;
            var dueDictionary = new Dictionary<string, DateTime>();

            try
            {
                // Find reminders that are due (time has passed)
                foreach (var kvp in _scheduledReminders)
                {
                    if (kvp.Value <= now)
                    {
                        dueDictionary[kvp.Key] = kvp.Value;
                    }
                }

                // Remove due reminders from the scheduled list
                foreach (var key in dueDictionary.Keys)
                {
                    _scheduledReminders.Remove(key);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get due reminders: {ex.Message}");
            }

            return dueReminders;
        }

        /// <summary>
        /// Checks if a notification should be shown as an alert based on type.
        /// </summary>
        public bool IsAlertNotification(NotificationItem notification)
        {
            if (notification == null)
                return false;

            // Check if the notification type is "Alert" or "Event"
            var isAlert = notification.Type?.Equals(ALERT_TYPE, StringComparison.OrdinalIgnoreCase) ?? false;
            var isEvent = notification.Type?.Equals(EVENT_TYPE, StringComparison.OrdinalIgnoreCase) ?? false;

            return isAlert || isEvent;
        }
    }
}
