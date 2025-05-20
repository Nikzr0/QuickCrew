using System.Data;

using Microsoft.AspNetCore.Identity;

using QuickCrew.Common;

namespace QuickCrew.Data.Seeding
{
    internal class AdminSeeder : ISeeder
    {
        public async Task SeedAsync(QuickCrewContext dbContext, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(RoleConstants.Admin));
        }
    }
}
