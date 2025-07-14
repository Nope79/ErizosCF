using ErizosCF.ViewModels;
namespace ErizosCF.Views;

public partial class DashBoardPage : ContentPage
{
    private double _lastContentScrollX;
    private double _lastHeaderScrollX;
    private bool _isSyncing;

    public DashBoardPage()
    {
        InitializeComponent();
    }

    private void OnContentHorizontalScroll(object sender, ScrolledEventArgs e)
    {
        if (!((DashBoardViewModel)BindingContext).EncabezadosHabilitados) return;

        if (_isSyncing) return;
        _isSyncing = true;
        _lastContentScrollX = e.ScrollX;
        HeaderScrollView.ScrollToAsync(e.ScrollX, 0, false);
        _isSyncing = false;
    }

    private void OnHeaderScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (!((DashBoardViewModel)BindingContext).EncabezadosHabilitados) return;

        if (_isSyncing) return;
        _isSyncing = true;
        _lastHeaderScrollX = e.ScrollX;
        ContentScrollView.ScrollToAsync(e.ScrollX, 0, false);
        _isSyncing = false;
    }
}