using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace CampusGuide.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [RelayCommand]
        public async Task GoToScanner()
        {
            await Shell.Current.GoToAsync("//scanner");
        }

        [RelayCommand]
        public async Task GoToFavorites()
        {
            try
            {
                await Shell.Current.GoToAsync("//search");
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
                await Shell.Current.GoToAsync("//search");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to Events failed: {ex.Message}");
            }
        }
    }
}
