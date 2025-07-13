using ErizosCF.ViewModels;
using Microsoft.Maui.Controls;

namespace ErizosCF.Views
{
    public partial class ConfiguracionPage : ContentPage
    {
        public ConfiguracionPage()
        {
            InitializeComponent();

            // Cargar configuraci�n guardada (ejemplo)
            SwitchTema.IsToggled = false;
            EntryUltimosContests.Text = "5";
            BindingContext = new ConfiguracionViewModel();
        }

        private void OnTemaToggled(object sender, ToggledEventArgs e)
        {
            // Cambiar tema, si tienes c�digo para eso
        }

        private void OnGuardarConfiguracion(object sender, System.EventArgs e)
        {
            // Guardar opciones de la app, por ejemplo en Preferences
            DisplayAlert("Configuraci�n", "Configuraci�n guardada.", "OK");
        }
    }
}
