using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ErizosCF.ViewModels
{
    public partial class DashBoardViewModel : ObservableObject
    {

        private readonly CFService _cfService;

        [ObservableProperty]
        private ObservableCollection<UserProfile> _usuariosResumen = new();

        [ObservableProperty]
        private DateTime _fechaInicio = DateTime.Now.AddMonths(-1); 

        [ObservableProperty]
        private DateTime _fechaFin = DateTime.Now;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EncabezadosHabilitados))]
        private bool _datosCargados;

        public bool EncabezadosHabilitados => DatosCargados;

        public DashBoardViewModel()
        {
            _cfService = new CFService();
        }

        [RelayCommand]
        private async Task CargarResumenUsuarios()
        {
            try
            {
                DatosCargados = false;
                UsuariosResumen.Clear();

                if (FechaInicio > FechaFin)
                {
                    await Shell.Current.DisplayAlert("Error", "La fecha de inicio no puede ser mayor a la fecha fin", "OK");
                    return;
                }

                var alumnosDB = await UserProfile.ObtenerTodosUsuariosAsync();

                foreach (var alumno in alumnosDB)
                {
                    var user = await _cfService.GetUserInfoAsync(alumno.Handle);
                    if (user == null) continue;
                    var problemas = await _cfService.GetUserStatusAsync(alumno.Handle, FechaInicio, FechaFin);
                    await alumno.ActualizarDatosCodeforces(user, problemas, alumno.IdEscuela);
                    UsuariosResumen.Add(alumno);

                    Debug.WriteLine($"Datos actualizados: {alumno.Handle}");
                    DatosCargados = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                DatosCargados = false;
            }
        }
    }
}