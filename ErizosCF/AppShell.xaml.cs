namespace ErizosCF
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("alumnodetalle", typeof(Views.AlumnoDetallePage));
        }
    }
}
