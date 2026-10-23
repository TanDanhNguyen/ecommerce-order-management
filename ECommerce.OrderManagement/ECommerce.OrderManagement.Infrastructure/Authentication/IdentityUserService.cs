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

        public async Task<(bool Succeeded, Guid UserId, IReadOnlyList<string> Errors)>
            CreateUserAsync(
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

            return (
                result.Succeeded,
                user.Id,
                result.Errors.Select(error => error.Description).ToArray());
        }

        public async Task<(bool Succeeded, Guid UserId, string Email)>ValidateCredentialsAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return (false, Guid.Empty, string.Empty);

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                password);

            if (!passwordValid)
                return (false, Guid.Empty, string.Empty);

            return (true, user.Id, user.Email ?? string.Empty);
        }
    }
}
