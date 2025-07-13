using CommunityToolkit.Mvvm.ComponentModel;

namespace ErizosCF.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
    }
}
