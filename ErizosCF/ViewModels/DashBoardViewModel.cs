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

        public DashBoardViewModel()
        {
            _cfService = new CFService();
        }

        [RelayCommand]
        private async Task CargarResumenUsuarios()
        {
            try
            {
                var alumnosDB = await UserProfile.ObtenerTodosAsync();

                UsuariosResumen.Clear();

                foreach (var alumno in alumnosDB)
                {
                    var user = await _cfService.GetUserInfoAsync(alumno.Handle);
                    await Task.Delay(500);

                    var problemas = await _cfService.GetUserStatusAsync(alumno.Handle);
                    await Task.Delay(500);

                    if (user == null || problemas == null)
                        continue;

                    alumno.ActualizarDatosCodeforces(user, problemas);
                    UsuariosResumen.Add(alumno);
                    Debug.WriteLine($"Agregado: {alumno.Handle} - {alumno.FullName} - {alumno.CurrentRating}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar usuarios: {ex.Message}");
            }
        }
    }
}