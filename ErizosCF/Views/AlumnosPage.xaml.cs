using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using ErizosCF.Models;
using ErizosCF.ViewModels;

namespace ErizosCF.Views
{
    public partial class AlumnosPage : ContentPage
    {
        private ObservableCollection<UserProfile> _alumnos;
        private ObservableCollection<UserProfile> _alumnosFiltrados;

        public AlumnosPage()
        {
            InitializeComponent();


            _alumnosFiltrados = new ObservableCollection<UserProfile>(_alumnos);
            AlumnosCollection.ItemsSource = _alumnosFiltrados;

            BindingContext = new AlumnosViewModel();
        }

        private async void OnAlumnoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            var alumno = e.CurrentSelection.FirstOrDefault() as UserProfile;
            if (alumno == null) return;

            // Navegar a detalle, enviando el handle como parámetro
            await Shell.Current.GoToAsync($"alumnodetalle?handle={alumno.Handle}");

            // Deseleccionar para poder volver a seleccionar
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}