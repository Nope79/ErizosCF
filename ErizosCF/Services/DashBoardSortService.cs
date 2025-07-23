using CommunityToolkit.Mvvm.ComponentModel;

public partial class DashboardSortService : ObservableObject
{
    public DashboardSortService() { }

    [ObservableProperty]
    private bool sortByUsuario;

    [ObservableProperty]
    private bool sortByNombre;

    [ObservableProperty]
    private bool sortByCurso;

    [ObservableProperty]
    private bool sortByEscuela;

    [ObservableProperty]
    private bool sortByRating;

    [ObservableProperty]
    private bool sortByTeam;

    [ObservableProperty]
    private bool sortByIndividual;

    [ObservableProperty]
    private bool sortByUnrated;

    [ObservableProperty]
    private bool sortAsc;

    [ObservableProperty]
    private bool sortDesc;


    public event Action OrdenamientoCambiado;

    // Detectar cambios
    partial void OnSortByUsuarioChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByNombreChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByCursoChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByEscuelaChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByRatingChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByTeamChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByIndividualChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortByUnratedChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortAscChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }
    partial void OnSortDescChanged(bool oldValue, bool newValue)
    {
        OrdenamientoCambiado?.Invoke();
    }

    public void Reset()
    {
        SortByUsuario = true;
        SortByNombre = false;
        SortByCurso = false;
        SortByEscuela = false;
        SortByRating = false;
        SortByTeam = false;
        SortByIndividual = false;
        SortByUnrated = false;
        SortAsc = true;
        SortDesc = false;
    }
}
