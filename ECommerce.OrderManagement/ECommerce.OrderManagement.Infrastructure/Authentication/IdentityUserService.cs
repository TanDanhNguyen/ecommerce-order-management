using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Infrastructure.Authentication
{
    public sealed class IdentityUserService : IUserIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityUserService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<(bool Succeeded, Guid UserId, IReadOnlyList<string> Errors)> CreateUserAsync(
                string userName,
                string email,
                string password,
                CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (
                    false,
                    user.Id,
                    result.Errors.Select(error => error.Description).ToArray());
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");

            if (!roleResult.Succeeded)
            {
                // Roll back the newly created account if role assignment fails.
                await _userManager.DeleteAsync(user);

                return (
                    false,
                    user.Id,
                    roleResult.Errors.Select(error => error.Description).ToArray());
            }

            return (true, user.Id, Array.Empty<string>());
        }


        public async Task<(
            bool Succeeded,
            Guid UserId,
            string Email,
            string Role)> ValidateCredentialsAsync(
            string email,
            string password,
        CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return (false, Guid.Empty, string.Empty, string.Empty);

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                password);

            if (!passwordValid)
                return (false, Guid.Empty, string.Empty, string.Empty);

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0)
            {
                return (false, Guid.Empty, string.Empty, string.Empty);
            }

            var role = roles[0];

            return (
                true,
                user.Id,
                user.Email ?? string.Empty,
                role);
        }
    }
}
