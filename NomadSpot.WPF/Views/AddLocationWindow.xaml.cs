using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class AddLocationWindow : Window
    {
        public AddLocationWindow()
        {
            InitializeComponent();
            Loaded   += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LocationViewModel vm)
                vm.LocationSaved += OnLocationSaved;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LocationViewModel vm)
                vm.LocationSaved -= OnLocationSaved;
        }

        private void OnLocationSaved(object sender, System.EventArgs e) => Close();
    }
}
