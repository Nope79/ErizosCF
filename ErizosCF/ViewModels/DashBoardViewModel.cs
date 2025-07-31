using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ErizosCF.Models;
using ErizosCF.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace ErizosCF.ViewModels
{
    public partial class DashBoardViewModel : ObservableObject
    {

        private readonly CFService _cfService;
        public DashboardFilterService Filtros { get; }
        public DashboardSortService Ordenador { get; }
        public Escuela EscuelaFiltros { get; }

        [ObservableProperty]
        private bool _isFiltrable;

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
        private bool modoFiltroSeleccionado; // "Estático" o "Dinámico" || true o false

        public DashBoardViewModel(DashboardFilterService filtros)
        {
            IsFiltrable = false;

            _cfService = new CFService();

            Filtros = filtros;
            Filtros.FiltrosCambiaron += () => AplicarFiltrosDinamicos().SafeFireAndForget();

            CargarEscuelasAsync().SafeFireAndForget();
        }

        public enum SortField
        {
            Usuario,
            Nombre,
            Curso,
            Escuela,
            Rating,
            Team,
            Individual,
            Unrated
        }

        public enum SortDirection
        {
            Asc,
            Desc
        }

        [ObservableProperty]
        private SortField selectedSortField = SortField.Usuario;

        [ObservableProperty]
        private SortDirection selectedSortDirection = SortDirection.Asc;

        public Array SortFields => Enum.GetValues(typeof(SortField));
        public Array SortDirections => Enum.GetValues(typeof(SortDirection));


        partial void OnSelectedSortFieldChanged(SortField value)
        {
            OrdenarUsuarios();
        }

        partial void OnSelectedSortDirectionChanged(SortDirection value)
        {
            OrdenarUsuarios();
        }

        private void OrdenarUsuarios()
        {
            try
            {
                if (UsuariosResumen == null || UsuariosResumen.Count == 0)
                    return;

                var usuarios = UsuariosResumen.AsEnumerable();

                usuarios = (SelectedSortField, SelectedSortDirection) switch
                {
                    (SortField.Usuario, SortDirection.Asc) => usuarios.OrderBy(u => u.Handle),
                    (SortField.Usuario, SortDirection.Desc) => usuarios.OrderByDescending(u => u.Handle),

                    (SortField.Nombre, SortDirection.Asc) => usuarios.OrderBy(u => u.FullName),
                    (SortField.Nombre, SortDirection.Desc) => usuarios.OrderByDescending(u => u.FullName),

                    (SortField.Curso, SortDirection.Asc) => usuarios.OrderBy(u => u.Curso),
                    (SortField.Curso, SortDirection.Desc) => usuarios.OrderByDescending(u => u.Curso),

                    (SortField.Escuela, SortDirection.Asc) => usuarios.OrderBy(u => u.NombreEscuela),
                    (SortField.Escuela, SortDirection.Desc) => usuarios.OrderByDescending(u => u.NombreEscuela),

                    (SortField.Rating, SortDirection.Asc) => usuarios.OrderBy(u => u.CurrentRating),
                    (SortField.Rating, SortDirection.Desc) => usuarios.OrderByDescending(u => u.CurrentRating),

                    (SortField.Individual, SortDirection.Asc) => usuarios.OrderBy(u => u.TotalSolved - u.Team),
                    (SortField.Individual, SortDirection.Desc) => usuarios.OrderByDescending(u => u.TotalSolved - u.Team),

                    (SortField.Team, SortDirection.Asc) => usuarios.OrderBy(u => u.Team),
                    (SortField.Team, SortDirection.Desc) => usuarios.OrderByDescending(u => u.Team),

                    (SortField.Unrated, SortDirection.Asc) =>
                        usuarios.OrderBy(u => u.ProblemasPorDificultad.ContainsKey(-1) ? u.ProblemasPorDificultad[-1] : 0),

                    (SortField.Unrated, SortDirection.Desc) =>
                        usuarios.OrderByDescending(u => u.ProblemasPorDificultad.ContainsKey(-1) ? u.ProblemasPorDificultad[-1] : 0),

                    _ => usuarios
                };

                UsuariosResumen = new ObservableCollection<UserProfile>(usuarios);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }
        }

        private async Task CargarEscuelasAsync()
        {
            try
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

                    escuela.FiltrosCambiaron += () => AplicarFiltrosDinamicos().SafeFireAndForget();

                    return escuela;
                });

                EscuelasDisponibles = new ObservableCollection<Escuela>(lista);
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }
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
                    var user = await _cfService.GetUserInfoAsync(alumno.Handle); // obtener usuario de CF

                    if (user == null) continue;

                    alumno.Problemas = await _cfService.GetUserStatusAsync(alumno.Handle, FechaInicio, FechaFin); // obtener problemas del usuario de CF
                    await alumno.ActualizarDatosCodeforces(user, alumno.Problemas, alumno.IdEscuela); // se actualizan datos y ProblemasPorDificultad del usuario (Diccionario)
                    alumno.ProblemasPorSemana = new ObservableCollection<int>(ProblemStats.ProblemasSemanales(alumno.Problemas, FechaInicio, FechaFin)); // se actualiza el conteo de problemas semanales (ObservableCollection)

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
            try
            {
                if (ModoFiltroSeleccionado) return;
                IsFiltrable = false;
                await Task.Delay(50);
                await UsuariosFiltro();  // actualizar UsuariosResumen

                CalcularEncabezadosSemanales();
                IsFiltrable = true;
            }
            catch(Exception e)
            {
                IsFiltrable = true;
                Debug.WriteLine($"Error: {e}");
            }
        }

        [RelayCommand]
        private async Task AplicarFiltrosEstaticos()
        {
            try
            {
                if (FechaFin >= FechaInicio)
                {
                    IsFiltrable = false;
                    await Task.Delay(50);
                    await UsuariosFiltro(); // actualizar UsuariosResumen

                    CalcularEncabezadosSemanales();

                    IsFiltrable = true;
                    ModoFiltroSeleccionado = true;
                }

                else
                {
                    EncabezadosSemanas.Clear();
                    await Shell.Current.DisplayAlert("Alerta", "La fecha de inicio no puede ser mayor a la fecha fin.", "OK");
                    IsFiltrable = false;
                    ModoFiltroSeleccionado = true;
                }
                
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error {e}");
            }
        }

        

        private async Task UsuariosFiltro()
        {
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

                foreach (var u in filtrados)
                {
                    var problemas = u.Problemas
                        .Where(p => p.SolvedDate >= FechaInicio && p.SolvedDate <= FechaFin)
                        .ToList();

                    await u.ActualizarDatosCodeforces(u, problemas, u.IdEscuela);
                    u.ProblemasPorSemana = new ObservableCollection<int>(ProblemStats.ProblemasSemanales(problemas, FechaInicio, FechaFin));
                }

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
            try
            {
                TimeSpan diferencia = FechaFin.Subtract(FechaInicio);
                int diasDiferencia = diferencia.Days + 1;

                diasDiferencia = (int)Math.Ceiling(diasDiferencia / 7.0);

                EncabezadosSemanas = Enumerable.Range(0, diasDiferencia)
                    .Select(i => {
                        var inicioSemana = FechaInicio.AddDays(i * 7);
                        var finSemana = inicioSemana.AddDays(6) > FechaFin ? FechaFin : inicioSemana.AddDays(6);
                        return $"{inicioSemana:dd/MM} - {finSemana:dd/MM}";
                    })
                    .ToList();
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }
        }

        public void ResetEscuelas()
        {
            try
            {
                foreach (var e in EscuelasDisponibles)
                {
                    e.EstaSeleccionada = true;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }
        }

        // ganchos y eventos

        private async Task VerificarYCalcularAsync()
        {
            if (FechaFin >= FechaInicio)
            {
                if (ModoFiltroSeleccionado) return;
                IsFiltrable = false;
                await Task.Delay(50);
                await UsuariosFiltro();
                CalcularEncabezadosSemanales();
                IsFiltrable = true;
            }

            else
            {
                EncabezadosSemanas.Clear();
                await Shell.Current.DisplayAlert("Alerta", "La fecha de inicio no puede ser mayor a la fecha fin.", "OK");
                IsFiltrable = false;
            }
        }

        async partial void OnFechaInicioChanged(DateTime value)
        {
            await VerificarYCalcularAsync();
        }

        async partial void OnFechaFinChanged(DateTime value)
        {
            await VerificarYCalcularAsync();
        }

        // csv y excel

        [RelayCommand]
        private async Task ExportarCsvAsync()
        {
            if (UsuariosResumen == null || UsuariosResumen.Count == 0)
            {
                await Shell.Current.DisplayAlert("Sin datos", "No hay usuarios para exportar.", "OK");
                return;
            }

            var sb = new StringBuilder();

            sb.Append("Usuario,Nombre,Curso,Escuela,Rating,Solved,Team,Individual,Unrated");
        
            var dificultades = new List<int> { 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 2100, 2200, 2300, 2400, 2500 };
            foreach (var dif in dificultades)
                sb.Append($",{dif}");

            foreach (var semana in EncabezadosSemanas)
                sb.Append($",\"{semana}\"");

            sb.AppendLine();

            foreach (var user in UsuariosResumen)
            {
                sb.Append($"{user.Handle},{user.FullName},{user.Curso},{user.NombreEscuela},{user.CurrentRating},{user.TotalSolved},{user.Team},{user.Individual}");

                sb.Append($",{user.ProblemasPorDificultad.GetValueOrDefault(-1)}");

                foreach (var dif in dificultades)
                {
                    sb.Append($",{user.ProblemasPorDificultad.GetValueOrDefault(dif)}");
                }

                foreach (var count in user.ProblemasPorSemana)
                {
                    sb.Append($",{count}");
                }

                sb.AppendLine();
            }

            // luego de crear el stringbuilder, lo guardamos haciendo uso del so de la compu (olv que void, es la primera vez que uso el so de la compu xdd)

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var stream = new MemoryStream(bytes);

            var result = await FileSaver.Default.SaveAsync("AvanceErizos.csv", stream, default);

            if (result.IsSuccessful)
                await Shell.Current.DisplayAlert("Guardado Correctamente", $"CSV guardado en:\n{result.FilePath}", "OK");
            else
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el archivo.", "OK");
        }

        [RelayCommand]
        private async Task ExportarExcel()
        {
            try
            {
                var workbook = new XLWorkbook();
                
                // primer hoja
                var hojaResumen = workbook.Worksheets.Add("Problemas Por Rating");

                hojaResumen.Cell(1, 1).Value = "Usuario";
                hojaResumen.Cell(1, 2).Value = "Nombre";
                hojaResumen.Cell(1, 3).Value = "Curso";
                hojaResumen.Cell(1, 4).Value = "Escuela";
                hojaResumen.Cell(1, 5).Value = "Rating";
                hojaResumen.Cell(1, 6).Value = "Solved";
                hojaResumen.Cell(1, 7).Value = "Team";
                hojaResumen.Cell(1, 8).Value = "Individual";
                hojaResumen.Cell(1, 9).Value = "Unrated";

                int col = 10;
                var dificultades = new List<int> { 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 2100, 2200, 2300, 2400, 2500 };
                foreach (var d in dificultades)
                    hojaResumen.Cell(1, col++).Value = d.ToString();

                int fila = 2;
                foreach (var user in UsuariosResumen)
                {
                    col = 1;
                    hojaResumen.Cell(fila, col++).Value = user.Handle;
                    hojaResumen.Cell(fila, col++).Value = user.FullName;
                    hojaResumen.Cell(fila, col++).Value = user.Curso;
                    hojaResumen.Cell(fila, col++).Value = user.NombreEscuela;
                    hojaResumen.Cell(fila, col++).Value = user.CurrentRating;
                    hojaResumen.Cell(fila, col++).Value = user.TotalSolved;
                    hojaResumen.Cell(fila, col++).Value = user.Team;
                    hojaResumen.Cell(fila, col++).Value = user.Individual;
                    hojaResumen.Cell(fila, col++).Value = user.ProblemasPorDificultad.ContainsKey(-1) ? user.ProblemasPorDificultad[-1] : 0;

                    foreach (var d in dificultades)
                    {
                        hojaResumen.Cell(fila, col++).Value = user.ProblemasPorDificultad.ContainsKey(d) ? user.ProblemasPorDificultad[d] : 0;
                    }

                    fila++;
                }

                hojaResumen.Columns().AdjustToContents();

                // segunda hoja
                var hojaSemanas = workbook.Worksheets.Add("Problemas Por Semanas");

                hojaSemanas.Cell(1, 1).Value = "Usuario";
                hojaSemanas.Cell(1, 2).Value = "Nombre";
                hojaSemanas.Cell(1, 3).Value = "Curso";
                hojaSemanas.Cell(1, 4).Value = "Escuela";
                hojaSemanas.Cell(1, 5).Value = "Rating";
                hojaSemanas.Cell(1, 6).Value = "Solved";
                hojaSemanas.Cell(1, 7).Value = "Team";
                hojaSemanas.Cell(1, 8).Value = "Individual";
                hojaSemanas.Cell(1, 9).Value = "Unrated";

                for (int i = 0; i < EncabezadosSemanas.Count; i++)
                {
                    hojaSemanas.Cell(1, i + 10).Value = EncabezadosSemanas[i];
                }
                
                fila = 2;
                foreach (var user in UsuariosResumen)
                {
                    col = 1;
                    hojaSemanas.Cell(fila, col++).Value = user.Handle;
                    hojaSemanas.Cell(fila, col++).Value = user.FullName;
                    hojaSemanas.Cell(fila, col++).Value = user.Curso;
                    hojaSemanas.Cell(fila, col++).Value = user.NombreEscuela;
                    hojaSemanas.Cell(fila, col++).Value = user.CurrentRating;
                    hojaSemanas.Cell(fila, col++).Value = user.TotalSolved;
                    hojaSemanas.Cell(fila, col++).Value = user.Team;
                    hojaSemanas.Cell(fila, col++).Value = user.Individual;
                    hojaSemanas.Cell(fila, col++).Value = user.ProblemasPorDificultad.ContainsKey(-1) ? user.ProblemasPorDificultad[-1] : 0;

                    for (int i = 0; i < user.ProblemasPorSemana.Count; i++)
                    {
                        hojaSemanas.Cell(fila, i + col).Value = user.ProblemasPorSemana[i];
                    }

                    fila++;
                }

                hojaSemanas.Columns().AdjustToContents();

                // guardar en memoria
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                // guardar archivo
                var result = await FileSaver.Default.SaveAsync("AvanceErizos.xlsx", stream, default);

                if (result.IsSuccessful)
                    await Shell.Current.DisplayAlert("Guardado Correctamente", $"Archivo Excel guardado en:\n{result.FilePath}", "OK");
                else
                    await Shell.Current.DisplayAlert("Error", "No se pudo guardar el archivo.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Error al exportar: {ex.Message}", "OK");
            }
        }
    }
}