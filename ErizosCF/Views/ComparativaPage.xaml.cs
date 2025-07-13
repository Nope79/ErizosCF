using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using ErizosCF.Models;
using ErizosCF.ViewModels;

namespace ErizosCF.Views
{
    public partial class ComparativaPage : ContentPage
    {
        private ObservableCollection<UserProfile> _alumnos;

        public ComparativaPage()
        {
            InitializeComponent();
            BindingContext = new ComparativaViewModel();
        }
    }
}
