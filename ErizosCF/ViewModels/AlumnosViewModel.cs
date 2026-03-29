using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ErizosCF.ViewModels
{
    public partial class AlumnosViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<UserProfile> alumnos = new();

        public AlumnosViewModel()
        {
            CargarAlumnosAsync();
        }

        private async void CargarAlumnosAsync()
        {
            var lista = await UserProfile.ObtenerTodosUsuariosAsync();
            Alumnos = new ObservableCollection<UserProfile>(lista);
        }
    }
}
