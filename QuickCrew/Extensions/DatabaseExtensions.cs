using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Seeding;

namespace QuickCrew.Extensions
{
    public static class DatabaseExtensions
    {
        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using QuickCrewContext context = scope.ServiceProvider.GetRequiredService<QuickCrewContext>();


            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            context.Database.Migrate();

            // Seeding
            var initializeSeeder = new DbContextSeeder();
            initializeSeeder.Execute(context, scope.ServiceProvider).GetAwaiter().GetResult();
        }
    }
}
