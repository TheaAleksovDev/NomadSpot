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
            _viewModel = App.ViewModel;
            DataContext = _viewModel;
        }

        private void DB_Changed(object sender, RoutedEventArgs e)
        {
            var dbType = PgRadio.IsChecked == true ? DatabaseType.PostgreSQL : DatabaseType.SQLite;
            var connStr = dbType == DatabaseType.PostgreSQL
                ? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!
                : Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")!;

            new DatabaseInitializer(dbType, connStr).Initialize();

            _viewModel = App.CreateViewModel(dbType, connStr);
            DataContext = _viewModel;
        }
    }
}
