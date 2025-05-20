namespace QuickCrew.Data.Seeding
{
    public interface ISeeder
    {
        Task SeedAsync(QuickCrewContext dbContext, IServiceProvider serviceProvider);
    }
}
