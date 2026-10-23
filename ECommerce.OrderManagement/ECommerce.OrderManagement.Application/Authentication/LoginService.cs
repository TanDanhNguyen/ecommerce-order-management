using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication
{
    public sealed class LoginService
    {
        private readonly IUserIdentityService _userIdentityService;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginService(
            IUserIdentityService userIdentityService,
            IJwtTokenService jwtTokenService)
        {
            _userIdentityService = userIdentityService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var result = await _userIdentityService.ValidateCredentialsAsync(
                request.Email.Trim(),
                request.Password,
                cancellationToken);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            return _jwtTokenService.CreateToken(
                result.UserId,
                result.Email);
        }
    }
}
