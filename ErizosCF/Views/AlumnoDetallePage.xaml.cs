using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace ErizosCF.Views
{
    [QueryProperty(nameof(Handle), "handle")]
    public partial class AlumnoDetallePage : ContentPage
    {
        private string _handle;
        public string Handle
        {
            get => _handle;
            set
            {
                _handle = value;
                CargarDetalleAlumno(value);
            }
        }

        public AlumnoDetallePage()
        {
            InitializeComponent();
        }

        private async void CargarDetalleAlumno(string handle)
        {
            await Task.Delay(500);

            LblNombre.Text = "Juan Pérez";
            LblHandle.Text = $"Handle: {handle}";
            LblRatingActual.Text = "Rating actual: 1800";
            LblRatingMax.Text = "Rating máximo: 1900";
            LblProblemasTotales.Text = "Problemas resueltos: 450";
            LblUltimoContest.Text = "Última participación: 2025-06-28";
            LblPosicionPromedio.Text = "Posición promedio últimos 5 contests: 120";
        }
    }
}
