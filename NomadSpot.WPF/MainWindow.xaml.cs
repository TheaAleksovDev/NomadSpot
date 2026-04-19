using NomadSpot.Model.Database;
using NomadSpot.ViewModel;
using System.Windows;

namespace NomadSpot.WPF
{
    public partial class MainWindow : Window
    {
        private LocationViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            SetViewModel(CreateViewModel(DatabaseType.PostgreSQL,
                Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!));
        }

        private static LocationViewModel CreateViewModel(DatabaseType dbType, string connStr)
        {
            var factory = new RepositoryFactory(dbType, connStr);
            return new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository());
        }

        private void SetViewModel(LocationViewModel vm)
        {
            _viewModel = vm;
            DataContext = _viewModel;
            _viewModel.UseMyLocationCommand = new ViewModel.RelayCommand(async () =>
            {
                try
                {
                    var geolocator = new Windows.Devices.Geolocation.Geolocator();
                    var position   = await geolocator.GetGeopositionAsync();
                    _viewModel.UserLatitude  = position.Coordinate.Point.Position.Latitude;
                    _viewModel.UserLongitude = position.Coordinate.Point.Position.Longitude;
                }
                catch
                {
                    _viewModel.StatusMessage = "Could not get location. Make sure location access is enabled in Windows Settings.";
                }
            });

            _viewModel.LocationsFound += (s, e) =>
            {
                var w = new Views.LocationListWindow();
                w.DataContext = _viewModel;
                w.Show();
            };

            _viewModel.OpenAddLocationRequested += (s, e) =>
            {
                var w = new Views.AddLocationWindow();
                w.DataContext = _viewModel;
                w.Show();
            };

            _viewModel.OpenAddReviewRequested += (s, e) =>
            {
                var w = new Views.AddReviewWindow();
                w.DataContext = _viewModel;
                w.Show();
            };

            _viewModel.OpenReviewListRequested += (s, e) =>
            {
                var w = new Views.ReviewListWindow();
                w.DataContext = _viewModel;
                w.Show();
            };

            _viewModel.OpenFilterRequested += filter =>
            {
                var filterWindow = new Views.FilterWindow(filter, new FilterControlFactory());
                filterWindow.Show();
            };

            _viewModel.OpenReviewFilterRequested += filter =>
            {
                var filterWindow = new Views.FilterWindow(filter, new FilterControlFactory());
                filterWindow.Show();
            };
        }

        private void DB_Changed(object sender, RoutedEventArgs e)
        {
            var dbType = PgRadio.IsChecked == true ? DatabaseType.PostgreSQL : DatabaseType.SQLite;
            var connStr = dbType == DatabaseType.PostgreSQL
                ? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!
                : Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")!;

            var factory     = new RepositoryFactory(dbType, connStr);
            var initializer = new DatabaseInitializer(dbType, connStr);
            initializer.Initialize();

            SetViewModel(new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository()));
        }
    }
}
