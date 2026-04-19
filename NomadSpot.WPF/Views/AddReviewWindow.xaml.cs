using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF.Views
{
    public partial class AddReviewWindow : Window
    {
        public AddReviewWindow()
        {
            InitializeComponent();
            Loaded   += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LocationViewModel vm)
                vm.ReviewSaved += OnReviewSaved;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LocationViewModel vm)
                vm.ReviewSaved -= OnReviewSaved;
        }

        private void OnReviewSaved(object sender, System.EventArgs e) => Close();
    }
}
