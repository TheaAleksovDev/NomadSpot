using NomadSpot.Model.Database;

namespace NomadSpot.Blazor.Services
{
    public class DatabaseSettingsService
    {
        public DatabaseType DbType { get; set; } = DatabaseType.PostgreSQL;

        public string ConnectionString => DbType == DatabaseType.PostgreSQL
            ? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")!
            : Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")!;
    }
}
