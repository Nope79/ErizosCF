using ErizosCF.ViewModels;
namespace ErizosCF.Views;

public partial class DashBoardPage : ContentPage
{
    public DashBoardPage()
    {
        InitializeComponent();
        BindingContext = new DashBoardViewModel();
    }
}