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

        [ObservableProperty]
        private List<string> _encabezadosSemanas = new();

        public bool EncabezadosHabilitados => DatosCargados;

        [ObservableProperty]
        private ObservableCollection<int> _cursosSeleccionados = new();

        public DashBoardViewModel()
        {
            _cfService = new CFService();        
        }

        [RelayCommand]
        private async Task CargarResumenUsuarios()
        {
            try
            {
                CalcularEncabezadosSemanales();



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
                    // filtros antes de realizar operaciones innecesarias como cargar listas de problemas, ver en el apiCF, y así
                    // if(alumno.Curso no pertenece al curso de los activos en el checkbox) continue

                    //
                    var user = await _cfService.GetUserInfoAsync(alumno.Handle);
                    if (user == null) continue;
                    var problemas = await _cfService.GetUserStatusAsync(alumno.Handle, FechaInicio, FechaFin);
                    await alumno.ActualizarDatosCodeforces(user, problemas, alumno.IdEscuela);
                    alumno.ProblemasPorSemana = new ObservableCollection<int>(ProblemStats.ProblemasPorSemana(problemas, FechaInicio, FechaFin));

                    UsuariosResumen.Add(alumno);
                    DatosCargados = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                DatosCargados = false;
            }
        }

        // segmentar los problemas

        private void CalcularEncabezadosSemanales()
        {
            TimeSpan diferencia = FechaFin.Subtract(FechaInicio);
            int diasDiferencia = diferencia.Days;

            diasDiferencia = (int)Math.Ceiling(diasDiferencia / 7.0);

            EncabezadosSemanas = Enumerable.Range(0, diasDiferencia)
                .Select(i => {
                    var inicioSemana = FechaInicio.AddDays(i * 7);
                    var finSemana = inicioSemana.AddDays(6) > FechaFin ? FechaFin : inicioSemana.AddDays(6);
                    return $"{inicioSemana:dd/MM} - {finSemana:dd/MM}";
                    })
                .ToList();
        }
    }
}