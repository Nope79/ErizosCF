using CommunityToolkit.Mvvm.ComponentModel;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Threading.Tasks;

namespace ErizosCF.ViewModels
{
    public partial class AlumnoDetalleViewModel : BaseViewModel
    {
        private readonly CFService _cfService;

        [ObservableProperty]
        UserProfile alumno;

        public AlumnoDetalleViewModel()
        {
            Title = "Detalle Alumno";
            _cfService = new CFService();
        }

        public async Task LoadAlumnoDetalleAsync(string handle)
        {
            IsBusy = true;

            // Aquí llama a tu servicio para obtener datos reales, ejemplo fijo:
           /* Alumno = new UserProfile
            {
                Handle = handle,
                Nombre = "Juan Pérez",
                CurrentRating = 1800,
                MaxRating = 1900,
                TotalSolved = 450,
                LastContestDate = System.DateTime.Now.AddDays(-10),
                AvgPositionLastNContests = 120
            };*/

            IsBusy = false;
        }
    }
}
