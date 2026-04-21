using NomadSpot.Model.Database;
using NomadSpot.ViewModel;
using System.IO;
using System.Windows;

namespace NomadSpot.WPF
{
    public partial class App : Application
    {
        public static LocationViewModel ViewModel { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoadEnvFile();

            new DatabaseInitializer(DatabaseType.PostgreSQL,
                Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!).Initialize();

            new DatabaseInitializer(DatabaseType.SQLite,
                Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")!).Initialize();

            ViewModel = CreateViewModel(DatabaseType.PostgreSQL,
                Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!);
        }

        public static LocationViewModel CreateViewModel(DatabaseType dbType, string connStr)
        {
            var factory = new RepositoryFactory(dbType, connStr);
            var vm = new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository());
            vm.WindowService = new WindowService(vm, new FilterControlFactory());
            vm.GeolocationService = new WpfGeolocationService();
            return vm;
        }

        private static void LoadEnvFile()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var envFile = Path.Combine(dir.FullName, ".env");
                if (File.Exists(envFile))
                {
                    foreach (var line in File.ReadAllLines(envFile))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
                        var idx = trimmed.IndexOf('=');
                        if (idx < 0) continue;
                        Environment.SetEnvironmentVariable(trimmed[..idx].Trim(), trimmed[(idx + 1)..].Trim());
                    }
                    break;
                }
                dir = dir.Parent;
            }
        }
    }
}
