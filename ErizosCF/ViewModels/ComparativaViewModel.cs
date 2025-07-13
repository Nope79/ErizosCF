using CommunityToolkit.Mvvm.ComponentModel;
using ErizosCF.Models;
using System.Collections.ObjectModel;

namespace ErizosCF.ViewModels
{
    public partial class ComparativaViewModel : BaseViewModel
    {
        public ObservableCollection<UserProfile> Alumnos { get; } = new();
        [ObservableProperty]
        string ooo;

        public ComparativaViewModel()
        {
            Title = "Comparativa";

        }
    }
}
