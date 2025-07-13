using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using ErizosCF.ViewModels;
namespace ErizosCF.Models
{
    public partial class Escuela : ObservableObject
    {
        [ObservableProperty]
        private string _nombreEscuela = string.Empty;

        [ObservableProperty]
        private bool _seleccionada = true;

        partial void OnSeleccionadaChanged(bool value)
        {
            if (Application.Current?.MainPage?.BindingContext is DashBoardViewModel vm)
            {
                vm.GuardarCambiosCommand?.Execute(null);
            }
        }

    }
}