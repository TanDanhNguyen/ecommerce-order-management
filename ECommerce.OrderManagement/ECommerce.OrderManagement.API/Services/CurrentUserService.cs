using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using System.Security.Claims;

namespace ECommerce.OrderManagement.API.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                    return null;

                var userIdValue =
                    user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

                return Guid.TryParse(userIdValue, out var userId)
                    ? userId
                    : null;
            }
        }
    }
}
