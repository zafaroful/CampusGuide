using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CampusGuide.Models;
using CampusGuide.Services;

namespace CampusGuide.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly IFirestoreService _firestoreService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<NotificationItem> notifications = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasNotifications = false;

        public NotificationsViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>() ?? throw new InvalidOperationException("IFirestoreService not registered");
            _notificationService = ServiceHelper.GetService<INotificationService>() ?? throw new InvalidOperationException("INotificationService not registered");

            // Load notifications on initialization (with small delay to ensure Firebase is ready)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500); // Give Firebase time to initialize
                await LoadNotificationsCommand.ExecuteAsync(null);
            });
        }

        /// <summary>
        /// Loads all notifications from Firebase Firestore and displays any alerts.
        /// </summary>
        [RelayCommand]
        public async Task LoadNotifications()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine("NotificationsViewModel: Loading notifications from Firestore...");
                var notificationsList = await _firestoreService.GetNotificationsAsync().ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                        {
                            notifications.Clear();

                            // If no notifications from Firestore, load test data
                            if (notificationsList == null || notificationsList.Count == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("NotificationsViewModel: No notifications from Firestore, loading test data...");
                                LoadTestNotifications();
                                return;
                            }

                            // Sort by date descending (newest first)
                            var sortedNotifications = notificationsList
                                .OrderByDescending(n => n.CreatedAt)
                                .ToList();

                            foreach (var notification in sortedNotifications)
                            {
                                notifications.Add(notification);
                            }

                            HasNotifications = notifications.Count > 0;

                            System.Diagnostics.Debug.WriteLine($"NotificationsViewModel: Loaded {notifications.Count} notifications");

                    // Check for alerts and display them
                    var alerts = sortedNotifications.Where(n => n.Type?.Equals("Alert", StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                    if (alerts.Count > 0)
                    {
                        _notificationService.DisplayAlerts(alerts);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotificationsViewModel: Error loading notifications: {ex.Message}");
                ErrorMessage = $"Failed to load notifications: {ex.Message}";
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HasNotifications = false;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadTestNotifications()
        {
            var testNotifications = new List<NotificationItem>
            {
                new NotificationItem
                {
                    Id = "1",
                    Title = "Library Extended Hours",
                    Body = "The library will remain open until 10 PM for finals preparation.",
                    Type = "Alert",
                    CreatedAt = DateTime.Now.AddHours(-2)
                },
                new NotificationItem
                {
                    Id = "2",
                    Title = "Campus Tour Available",
                    Body = "Join our guided campus tour at 2 PM in the main courtyard.",
                    Type = "Notification",
                    CreatedAt = DateTime.Now.AddHours(-4)
                },
                new NotificationItem
                {
                    Id = "3",
                    Title = "Cafeteria Closure",
                    Body = "The main cafeteria will be closed for renovations next week.",
                    Type = "Alert",
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

            foreach (var notification in testNotifications)
            {
                Notifications.Add(notification);
            }

            HasNotifications = Notifications.Count > 0;
            System.Diagnostics.Debug.WriteLine($"NotificationsViewModel: Loaded {Notifications.Count} test notifications");
        }

         /// <summary>
         /// Refreshes notifications from Firebase.
         /// </summary>
         [RelayCommand]
         public async Task RefreshNotifications()
         {
            await LoadNotificationsCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Gets notifications filtered by type.
        /// </summary>
        public ObservableCollection<NotificationItem> GetNotificationsByType(string notificationType)
        {
            var filtered = new ObservableCollection<NotificationItem>(
                notifications.Where(n => n.Type?.Equals(notificationType, StringComparison.OrdinalIgnoreCase) ?? false)
            );
            return filtered;
        }

        /// <summary>
        /// Gets notifications for a specific event.
        /// </summary>
        public ObservableCollection<NotificationItem> GetNotificationsForEvent(string eventId)
        {
            var filtered = new ObservableCollection<NotificationItem>(
                notifications.Where(n => n.RelatedEventId == eventId)
            );
            return filtered;
        }

        /// <summary>
        /// Gets notifications for a specific location.
        /// </summary>
        public ObservableCollection<NotificationItem> GetNotificationsForLocation(string locationId)
        {
            var filtered = new ObservableCollection<NotificationItem>(
                notifications.Where(n => n.RelatedLocationId == locationId)
            );
            return filtered;
        }
    }
}
