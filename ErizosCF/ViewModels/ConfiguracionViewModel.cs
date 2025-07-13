using CommunityToolkit.Mvvm.ComponentModel;

namespace ErizosCF.ViewModels
{
    public partial class ConfiguracionViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool temaOscuro;

        [ObservableProperty]
        int ultimosContests;

        [ObservableProperty]
        int n;

        public ConfiguracionViewModel()
        {
            Title = "Configuración";
            TemaOscuro = false;
            UltimosContests = 5;
            n = 4;
        }
    }
}
