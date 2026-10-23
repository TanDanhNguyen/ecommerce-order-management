using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Exceptions;

namespace ECommerce.OrderManagement.Infrastructure.Authentication
{
    public sealed class RegisterService
    {
        private readonly IUserIdentityService _userIdentityService;

        public RegisterService(IUserIdentityService userIdentityService)
        {
            _userIdentityService = userIdentityService;
        }

        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var userName = request.UserName?.Trim();
            var email = request.Email?.Trim();
            var password = request.Password;

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.");

            var result = await _userIdentityService.CreateUserAsync(
                userName,
                email,
                password,
                cancellationToken);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors);

                var isDuplicate = result.Errors.Any(error =>
                    error.Contains("already taken", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("DuplicateUserName", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("DuplicateEmail", StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                    throw new UserAlreadyExistsException(errors);

                throw new ArgumentException(errors);
            }

            return new RegisterResponse(
                result.UserId,
                userName,
                email);
        }
    }
}