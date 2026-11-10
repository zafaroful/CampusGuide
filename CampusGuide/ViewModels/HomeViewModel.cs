using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace CampusGuide.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private bool isEmpty = false;

        public HomeViewModel()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            IsEmpty = false;
            await Task.Delay(300);
            IsLoading = false;
            IsEmpty = true;
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
