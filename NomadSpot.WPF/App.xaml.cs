using NomadSpot.Model.Database;
using System.Windows;

namespace NomadSpot.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var initializer = new DatabaseInitializer(
                DatabaseType.PostgreSQL,
                "Host=localhost;Database=nomadspot;Username=postgres;Password=tea123");

            initializer.Initialize();
        }
    }
}