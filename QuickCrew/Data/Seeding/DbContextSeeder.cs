namespace QuickCrew.Data.Seeding
{
    public class DbContextSeeder
    {
        public async Task Execute(QuickCrewContext dbContext, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            ArgumentNullException.ThrowIfNull(serviceProvider);

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(DbContextSeeder));

            var seeders = new List<ISeeder>
            {
                new AdminSeeder(),
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext, serviceProvider);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Seeder {Name} done.", seeder.GetType().Name);
            }
        }
    }
}
