using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CampusGuide.Models;
using CampusGuide.Services;

namespace CampusGuide.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IFirestoreService _firestoreService;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private bool isEmpty = false;

        public ObservableCollection<NotificationItem> Announcements { get; } = new();

        public HomeViewModel()
        {
            _firestoreService = ServiceHelper.GetService<IFirestoreService>() ?? throw new InvalidOperationException("IFirestoreService not registered");
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            IsEmpty = false;
            try
            {
                var items = await _firestoreService.GetNotificationsAsync();
                Announcements.Clear();
                if (items != null)
                {
                    foreach (var item in items.OrderByDescending(n => n.CreatedAt).Take(3))
                    {
                        Announcements.Add(item);
                    }
                }
                IsEmpty = Announcements.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeViewModel: Failed to load updates: {ex.Message}");
                IsEmpty = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoToScanner()
        {
            await Shell.Current.GoToAsync("//scanner");
        }

        [RelayCommand]
        public async Task GoToMap()
        {
            try
            {
                await Shell.Current.GoToAsync("//map");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to Map failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task GoToSearch()
        {
            try
            {
                await Shell.Current.GoToAsync("//search");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to Search failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task GoToFavorites()
        {
            try
            {
                await Shell.Current.GoToAsync("//favorites");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to Favorites failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task GoToEvents()
        {
            try
            {
                await Shell.Current.GoToAsync("//events");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to Events failed: {ex.Message}");
            }
        }
    }
}
