using NomadSpot.Model.Database;
using NomadSpot.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace NomadSpot.WPF
{
    public partial class MainWindow : Window
    {
        private LocationViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var factory = new RepositoryFactory(
                DatabaseType.PostgreSQL,
                "Host=localhost;Database=nomadspot;Username=postgres;Password=tea123");

            _viewModel = new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository());

            DataContext = _viewModel;
        }

        private void FindLocations_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(LatitudeBox.Text, out double lat) ||
                !double.TryParse(LongitudeBox.Text, out double lon))
            {
                MessageBox.Show("Please enter valid coordinates.");
                return;
            }

            _viewModel.UserLatitude = lat;
            _viewModel.UserLongitude = lon;

            if (int.TryParse(ResultCountBox.Text, out int count))
                _viewModel.ResultCount = count;

            bool indoorOnly = ((ComboBoxItem)LocationTypeBox.SelectedItem).Content.ToString() == "Indoor";
            _viewModel.IndoorFilter.ClearFilters();
            _viewModel.OutdoorFilter.ClearFilters();
            _viewModel.FindClosestLocations(indoorOnly);

            var window = new Views.LocationListWindow(_viewModel);
            window.Show();
        }

        private void FilterLocations_Click(object sender, RoutedEventArgs e)
        {
            bool indoorOnly = ((ComboBoxItem)LocationTypeBox.SelectedItem).Content.ToString() == "Indoor";

            IFilterViewModel filter = indoorOnly ? _viewModel.IndoorFilter : (IFilterViewModel)_viewModel.OutdoorFilter;

            Action onSearch = () =>
            {
                _viewModel.UserLatitude = double.TryParse(LatitudeBox.Text, out double lat) ? lat : 0;
                _viewModel.UserLongitude = double.TryParse(LongitudeBox.Text, out double lon) ? lon : 0;
                _viewModel.FindClosestLocations(indoorOnly);

                var listWindow = new Views.LocationListWindow(_viewModel);
                listWindow.Show();
            };
            var filterWindow = new Views.FilterWindow(filter, onSearch);

            filterWindow.Show();
        }

        private void AddLocation_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.AddLocationWindow(_viewModel);
            window.Show();
        }

        private void DB_Changed(object sender, RoutedEventArgs e)
        {
            var dbType = PgRadio.IsChecked == true ? DatabaseType.PostgreSQL : DatabaseType.SQLite;
            var connStr = dbType == DatabaseType.PostgreSQL
                ? "Host=localhost;Database=nomadspot;Username=postgres;Password=tea123"
                : "Data Source=nomadspot.db";

            var factory = new RepositoryFactory(dbType, connStr);
            var initializer = new DatabaseInitializer(dbType, connStr);
            initializer.Initialize();

            _viewModel = new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository());

            DataContext = _viewModel;
        }
    }
}