using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Infrastructure.Identity
{
    public static class IdentityRoleSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roles = ["Admin", "Customer"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole<Guid>(role));

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            result.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"Failed to create role '{role}': {errors}");
                    }
                }
            }
        }
    }
}
