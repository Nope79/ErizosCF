using ErizosCF.ViewModels;
namespace ErizosCF.Views;

public partial class DashBoardPage : ContentPage
{
    public DashBoardPage(DashBoardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    private void OnCheckboxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is DashBoardViewModel vm)
        {
            vm.GuardarCambiosCommand.Execute(null);
        }
    }
}