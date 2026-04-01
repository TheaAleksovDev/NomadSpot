using NomadSpot.Blazor.Components;
using NomadSpot.Model.Database;
using NomadSpot.ViewModel;


namespace NomadSpot.Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var factory = new RepositoryFactory(
                DatabaseType.PostgreSQL,
                "Host=localhost;Database=nomadspot;Username=postgres;Password=tea123");

            builder.Services.AddScoped<LocationViewModel>(_ => new LocationViewModel(
                factory.CreateLocationRepository(),
                factory.CreateReviewRepository()));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
    }
}
