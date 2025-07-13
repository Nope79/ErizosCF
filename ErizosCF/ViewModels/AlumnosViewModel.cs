using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ErizosCF.ViewModels
{
    public partial class AlumnosViewModel : BaseViewModel
    {
        private readonly CFService _cfService;

        [ObservableProperty]
        ObservableCollection<UserProfile> alumnos;

        [ObservableProperty]
        string filtro;

        public AlumnosViewModel()
        {
            Title = "Alumnos";
            _cfService = new CFService();
            //Alumnos = new ObservableCollection<UserProfile>();
            _ = LoadAlumnosAsync();
        }

        public async Task LoadAlumnosAsync()
        {
            IsBusy = true;


            IsBusy = false;
        }

        //[ICommand]
        void Buscar()
        {
            //if (string.IsNullOrWhiteSpace(Filtro))
            {
                // Recargar todos o no filtrar
                _ = LoadAlumnosAsync();
                return;
            }

            //var filtroLower = Filtro.ToLower();

            //var filtrados = Alumnos.Where(a =>
                //a.Nombre.ToLower().Contains(filtroLower) ||
                //a.Handle.ToLower().Contains(filtroLower)).ToList();

           // Alumnos.Clear();
            //foreach (var f in filtrados)
               // Alumnos.Add(f);
        }
    }
}
