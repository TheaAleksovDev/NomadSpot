using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class AddReviewWindow : Window
    {
        private readonly LocationViewModel _viewModel;

        public AddReviewWindow(LocationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}