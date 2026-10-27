using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace CampusGuide.ViewModels
{
    public partial class SplashViewModel : BaseViewModel
    {
        [RelayCommand]
        public async Task Initialize()
        {
            IsBusy = true;
            await Task.Delay(800);
            IsBusy = false;
        }
    }
}
