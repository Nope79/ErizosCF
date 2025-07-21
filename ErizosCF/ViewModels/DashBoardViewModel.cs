using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using Mysqlx.Expr;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace ErizosCF.ViewModels
{
    public partial class DashBoardViewModel : ObservableObject
    {

        private readonly CFService _cfService;
        public DashboardFilterService Filtros { get; }
        public Escuela EscuelaFiltros { get; }

        [ObservableProperty]
        private bool _isFiltrable = false;

        /*
        public bool IsNotFiltrable => !IsFiltrable;
        partial void OnIsFiltrableChanged(bool oldValue, bool newValue)
        {
            OnPropertyChanged(nameof(IsNotFiltrable));
        }*/

        [ObservableProperty]
        private ObservableCollection<UserProfile> _todosUsuariosResumen = new();

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

        [ObservableProperty]
        ObservableCollection<Escuela> _escuelasDisponibles;

        [ObservableProperty]
        private bool modoFiltroSeleccionado = false; // "Estático" o "Dinámico" || true o false

        private bool modoCargar = false;
        public DashBoardViewModel(DashboardFilterService filtros)
        {
            _cfService = new CFService();

            Filtros = filtros;
            Filtros.FiltrosCambiaron += () => AplicarFiltros().SafeFireAndForget();

            CargarEscuelasAsync();
        }

        private async Task CargarEscuelasAsync()
        {
            var query = await Escuela.ObtenerEscuelasAsync();

            var lista = query.Select(e =>
            {
                var escuela = new Escuela
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    EstaSeleccionada = true
                };

                escuela.FiltrosCambiaron += () => AplicarFiltros().SafeFireAndForget();

                return escuela;
            });

            EscuelasDisponibles = new ObservableCollection<Escuela>(lista);
        }

        [RelayCommand]
        private async Task CargarResumenUsuarios()
        {
            try
            {
                Filtros.Reset();
                ResetEscuelas();

                IsFiltrable = false;
                ModoFiltroSeleccionado = false;

                UsuariosResumen.Clear();
                CalcularEncabezadosSemanales();
                DatosCargados = false;

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
                    alumno.ProblemasPorSemana = new ObservableCollection<int>(ProblemStats.ProblemasPorSemana(problemas, FechaInicio, FechaFin));

                    UsuariosResumen.Add(alumno);
                    DatosCargados = true;
                }

                TodosUsuariosResumen = UsuariosResumen;
                IsFiltrable = true;
                ModoFiltroSeleccionado = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                DatosCargados = false;
                IsFiltrable = false;
                ModoFiltroSeleccionado = false;
            }
        }

        private async Task AplicarFiltrosDinamicos()
        {
            if (ModoFiltroSeleccionado) return;
            IsFiltrable = false;
            await Task.Delay(50);

            try
            {
                var filtrados = await Task.Run(() =>
                {
                    var cursosSeleccionados = new List<int>();
                    if (Filtros.Curso1Seleccionado) cursosSeleccionados.Add(1);
                    if (Filtros.Curso2Seleccionado) cursosSeleccionados.Add(2);
                    if (Filtros.Curso3Seleccionado) cursosSeleccionados.Add(3);

                    var sexosSeleccionados = new List<string>();
                    if (Filtros.Hombres) sexosSeleccionados.Add("M");
                    if (Filtros.Mujeres) sexosSeleccionados.Add("F");

                    var rangosSeleccionados = new List<int>();
                    if (Filtros.Newbie) rangosSeleccionados.Add(0);
                    if (Filtros.Pupil) rangosSeleccionados.Add(1);
                    if (Filtros.Specialist) rangosSeleccionados.Add(2);
                    if (Filtros.Expert) rangosSeleccionados.Add(3);
                    if (Filtros.CandidateMaster) rangosSeleccionados.Add(4);
                    if (Filtros.Master) rangosSeleccionados.Add(5);
                    if (Filtros.InternationalMaster) rangosSeleccionados.Add(6);
                    if (Filtros.GrandMaster) rangosSeleccionados.Add(7);
                    if (Filtros.InternationalGrandMaster) rangosSeleccionados.Add(8);
                    if (Filtros.LegendaryGrandMaster) rangosSeleccionados.Add(9);

                    var estadoSeleccionado = new List<string>();
                    if (Filtros.Icpc) estadoSeleccionado.Add("ICPC");
                    if (Filtros.Excelente) estadoSeleccionado.Add("EXCELENTE");
                    if (Filtros.Normal) estadoSeleccionado.Add("NORMAL");
                    if (Filtros.Riesgo) estadoSeleccionado.Add("RIESGO");

                    var escuelasSeleccionadas = EscuelasDisponibles
                        .Where(e => e.EstaSeleccionada)
                        .Select(e => e.Id)
                        .ToList();

                    return TodosUsuariosResumen
                        .AsParallel()
                        .Where(u =>
                            cursosSeleccionados.Contains(u.Curso) &&
                            sexosSeleccionados.Contains(u.Sexo) &&
                            rangosSeleccionados.Contains(UserProfile.ObtenerRangoDesdeRating(u.CurrentRating)) &&
                            estadoSeleccionado.Contains(u.Estado) &&
                            escuelasSeleccionadas.Contains(u.IdEscuela)
                        )
                        .ToList();
                });

                UsuariosResumen = new ObservableCollection<UserProfile>(filtrados);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }

            finally
            {
                IsFiltrable = true;
            }
        }

        [RelayCommand]
        private async Task AplicarFiltrosEstaticos()
        {
            IsFiltrable = false;
            await Task.Delay(50);

            try
            {
                var filtrados = await Task.Run(() =>
                {
                    var cursosSeleccionados = new List<int>();
                    if (Filtros.Curso1Seleccionado) cursosSeleccionados.Add(1);
                    if (Filtros.Curso2Seleccionado) cursosSeleccionados.Add(2);
                    if (Filtros.Curso3Seleccionado) cursosSeleccionados.Add(3);

                    var sexosSeleccionados = new List<string>();
                    if (Filtros.Hombres) sexosSeleccionados.Add("M");
                    if (Filtros.Mujeres) sexosSeleccionados.Add("F");

                    var rangosSeleccionados = new List<int>();
                    if (Filtros.Newbie) rangosSeleccionados.Add(0);
                    if (Filtros.Pupil) rangosSeleccionados.Add(1);
                    if (Filtros.Specialist) rangosSeleccionados.Add(2);
                    if (Filtros.Expert) rangosSeleccionados.Add(3);
                    if (Filtros.CandidateMaster) rangosSeleccionados.Add(4);
                    if (Filtros.Master) rangosSeleccionados.Add(5);
                    if (Filtros.InternationalMaster) rangosSeleccionados.Add(6);
                    if (Filtros.GrandMaster) rangosSeleccionados.Add(7);
                    if (Filtros.InternationalGrandMaster) rangosSeleccionados.Add(8);
                    if (Filtros.LegendaryGrandMaster) rangosSeleccionados.Add(9);

                    var estadoSeleccionado = new List<string>();
                    if (Filtros.Icpc) estadoSeleccionado.Add("ICPC");
                    if (Filtros.Excelente) estadoSeleccionado.Add("EXCELENTE");
                    if (Filtros.Normal) estadoSeleccionado.Add("NORMAL");
                    if (Filtros.Riesgo) estadoSeleccionado.Add("RIESGO");

                    var escuelasSeleccionadas = EscuelasDisponibles
                        .Where(e => e.EstaSeleccionada)
                        .Select(e => e.Id)
                        .ToList();

                    return TodosUsuariosResumen
                        .AsParallel()
                        .Where(u =>
                            cursosSeleccionados.Contains(u.Curso) &&
                            sexosSeleccionados.Contains(u.Sexo) &&
                            rangosSeleccionados.Contains(UserProfile.ObtenerRangoDesdeRating(u.CurrentRating)) &&
                            estadoSeleccionado.Contains(u.Estado) &&
                            escuelasSeleccionadas.Contains(u.IdEscuela)
                        )
                        .ToList();
                });

                UsuariosResumen = new ObservableCollection<UserProfile>(filtrados);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }

            finally
            {
                IsFiltrable = true;
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

        public void ResetEscuelas()
        {
            foreach(var e in EscuelasDisponibles)
            {
                e.EstaSeleccionada = true;
            }
        }
    }
}