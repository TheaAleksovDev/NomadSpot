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

            var dbType = DatabaseType.PostgreSQL;
            var connStr = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!;

            var initializer = new DatabaseInitializer(dbType, connStr);
            initializer.Initialize();

            var factory = new RepositoryFactory(dbType, connStr);

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

        private async void UseMyLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var geolocator = new Windows.Devices.Geolocation.Geolocator();
                var position = await geolocator.GetGeopositionAsync();
                LatitudeBox.Text = position.Coordinate.Point.Position.Latitude.ToString("F6");
                LongitudeBox.Text = position.Coordinate.Point.Position.Longitude.ToString("F6");
            }
            catch
            {
                MessageBox.Show("Could not get location. Make sure location access is enabled in Windows Settings.");
            }
        }

        private void DB_Changed(object sender, RoutedEventArgs e)
        {
            var dbType = PgRadio.IsChecked == true ? DatabaseType.PostgreSQL : DatabaseType.SQLite;
            var connStr = dbType == DatabaseType.PostgreSQL
                ? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!
                : Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")!;

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