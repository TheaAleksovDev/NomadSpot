using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class LocationListWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public LocationListWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}