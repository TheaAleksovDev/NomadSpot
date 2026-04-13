using NomadSpot.Blazor.Components;
using NomadSpot.Blazor.Services;
using NomadSpot.Model.Database;
using NomadSpot.ViewModel;


namespace NomadSpot.Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LoadEnvFile();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<DatabaseSettingsService>();

            var pgSettings = new DatabaseSettingsService { DbType = DatabaseType.PostgreSQL };
            new DatabaseInitializer(pgSettings.DbType, pgSettings.ConnectionString).Initialize();

            var sqliteSettings = new DatabaseSettingsService { DbType = DatabaseType.SQLite };
            new DatabaseInitializer(sqliteSettings.DbType, sqliteSettings.ConnectionString).Initialize();

            builder.Services.AddScoped<LocationViewModel>(sp =>
            {
                var settings = sp.GetRequiredService<DatabaseSettingsService>();
                var factory = new RepositoryFactory(settings.DbType, settings.ConnectionString);
                return new LocationViewModel(
                    factory.CreateLocationRepository(),
                    factory.CreateReviewRepository());
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
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
