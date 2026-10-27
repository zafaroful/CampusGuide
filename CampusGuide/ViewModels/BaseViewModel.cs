using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusGuide.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
