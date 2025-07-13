using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ErizosCF.ViewModels
{
    public partial class DashBoardViewModel : BaseViewModel
    {
        private readonly DbService _dbservice = new DbService();

        private readonly EscuelaService _escuelaService;

        [ObservableProperty]
        private ObservableCollection<Escuela> _escuelas = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarBotonGuardar))]
        [NotifyPropertyChangedFor(nameof(MostrarBotonEliminar))]
        private Escuela _escuelaSeleccionada;

        [ObservableProperty]
        private int _dificultadMinima = 800;

        [ObservableProperty]
        private int _dificultadMaxima = 2500;

        [ObservableProperty]
        private List<int> _dificultadesDisponibles = new();

        [ObservableProperty]
        private ObservableCollection<UserProfile> _usuariosResumen = new();

        partial void OnDificultadMinimaChanged(int value) => GenerarDificultades();
        partial void OnDificultadMaximaChanged(int value) => GenerarDificultades();

        public bool MostrarBotonGuardar => EscuelaSeleccionada != null;
        public bool MostrarBotonEliminar => EscuelaSeleccionada != null;

        public DashBoardViewModel(EscuelaService escuelaService)
        {
            _escuelaService = escuelaService;
            CargarEscuelas();

            _dbservice.OpenConnectionAsync();
            _dbservice.CloseConnection();
        }

        [RelayCommand]
        private async Task CargarResumenUsuarios()
        {
            var alumnosDB = _dbservice.ObtenerAlumnos(); // (handle, nombre, escuela)
            var cf = new CFService();
            UsuariosResumen.Clear();

            foreach (var (handle, nombre, escuela) in alumnosDB)
            {
                var user = await cf.GetUserInfoAsync(handle);
                var problemas = await cf.GetUserStatusAsync(handle);

                if (user == null || problemas == null) continue;

                var problemasOk = problemas
                    .Where(p => p.Verdict == "OK")
                    .DistinctBy(p => p.ProblemName)
                    .ToList();

                var problemasPorDif = new Dictionary<int, int>();
                foreach (var p in problemasOk)
                {
                    if (p.Rating.HasValue)
                    {
                        int r = p.Rating.Value;
                        if (!problemasPorDif.ContainsKey(r)) problemasPorDif[r] = 0;
                        problemasPorDif[r]++;
                    }
                }

                var profile = new UserProfile
                {
                    Handle = handle,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Escuela = escuela,
                    CurrentRating = user.CurrentRating,
                    MaxRating = user.MaxRating,
                    TotalSolved = problemasOk.Count,
                    Problemas = problemasOk,
                    ProblemasPorDificultad = problemasPorDif
                };

                UsuariosResumen.Add(profile);
            }

            GenerarDificultades();
        }

            private void GenerarDificultades()
        {
            if (DificultadMaxima < DificultadMinima) return;
            DificultadesDisponibles = Enumerable
                .Range(DificultadMinima / 100, (DificultadMaxima - DificultadMinima) / 100 + 1)
                .Select(x => x * 100)
                .ToList();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [RelayCommand]
        private void GuardarCambios()
        {
            _escuelaService.SaveEscuelas(Escuelas);
        }

        // FILTROS CODEPANAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [RelayCommand]
        private async Task FiltroEscuela()
        {
            // MANDAMOS LLAMAR AL CFSERVICE CON ALGUNA FUNCION QUE ESTE TENGA Y YA NOMAS CARGAMOS LA LISTA DE ALUMNOS O ALGO EN BASE A ESO... NO SE JEJE AYUDA
        }

        // CRUD DE ESCUELAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CargarEscuelas()
        {
            try
            {
                var escuelasCargadas = _escuelaService.LoadEscuelas();
                Escuelas.Clear();

                foreach (var escuela in escuelasCargadas)
                {
                    Escuelas.Add(escuela);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar escuelas: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddEscuela()
        {
            string nombre = await Shell.Current.DisplayPromptAsync(
                "Nueva Escuela",
                "Ingrese el nombre de la escuela:",
                "Aceptar",
                "Cancelar",
                maxLength: 50);

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var nuevaEscuela = new Escuela
                {
                    NombreEscuela = nombre,
                    Seleccionada = true
                };

                Escuelas.Add(nuevaEscuela);
                _escuelaService.SaveEscuelas(Escuelas);
            }
        }

        [RelayCommand]
        private async Task UpdateEscuela()
        {
            if (EscuelaSeleccionada == null) return;

            string nuevoNombre = await Shell.Current.DisplayPromptAsync(
                "Editar Escuela",
                "Nuevo nombre:",
                "Guardar",
                "Cancelar",
                EscuelaSeleccionada.NombreEscuela,
                maxLength: 50);

            if (!string.IsNullOrWhiteSpace(nuevoNombre))
            {
                EscuelaSeleccionada.NombreEscuela = nuevoNombre;
                _escuelaService.SaveEscuelas(Escuelas);
                EscuelaSeleccionada = null;
            }
        }

        [RelayCommand]
        private async Task DeleteEscuela()
        {
            if (EscuelaSeleccionada == null) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Confirmar",
                $"¿Eliminar la escuela {EscuelaSeleccionada.NombreEscuela}?",
                "Sí",
                "No");

            if (confirmar)
            {
                Escuelas.Remove(EscuelaSeleccionada);
                _escuelaService.SaveEscuelas(Escuelas);
                EscuelaSeleccionada = null;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}