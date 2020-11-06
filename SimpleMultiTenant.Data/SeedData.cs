using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMultiTenant.Data
{
    public static class SeedData
    {
        public static async Task Seed(ApplicationDbContext dataContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string[] roles)
        {
            if (dataContext.Users.Any() == false)
            {
                List<IdentityUser> users = await CreateUsers(dataContext, userManager);
                await CreateRoles(roleManager, roles);
                await AddUsersToRoles(dataContext, userManager, users, roles);
            }
        }

        private static async Task AddUsersToRoles(ApplicationDbContext dataContext, UserManager<IdentityUser> userManager, List<IdentityUser> users, string[] roles)
        {
            foreach (var user in users)
            {
                foreach (var role in roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await dataContext.SaveChangesAsync();
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager, string[] roles)
        {
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task<List<IdentityUser>> CreateUsers(ApplicationDbContext dataContext, UserManager<IdentityUser> userManager)
        {
            var testEmail = "test@test.com";
            var defaultPassword = "123!Aa";

            var user = new IdentityUser
            {
                UserName = testEmail,
                Email = testEmail
            };

            await userManager.CreateAsync(user, defaultPassword);
            await dataContext.SaveChangesAsync();

            return new List<IdentityUser>() { user };
        }
    }
}
