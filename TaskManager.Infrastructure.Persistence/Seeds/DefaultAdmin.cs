using Microsoft.AspNetCore.Identity;

namespace TaskManager.Infrastructure.Persistence.Seeds
{
    public static class DefaultAdmin
    {
        public static async Task SeedAsync(UserManager<Domain.Entities.User> userManager)
        {
            var defaultUser = new Domain.Entities.User
            {
                UserName = "admin",
                Email = "admin@test.email",
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Admin.1@");
                    await userManager.AddToRoleAsync(defaultUser, "Admin");
                    await userManager.AddToRoleAsync(defaultUser, "User");
                }

            }
        }
    }
}
