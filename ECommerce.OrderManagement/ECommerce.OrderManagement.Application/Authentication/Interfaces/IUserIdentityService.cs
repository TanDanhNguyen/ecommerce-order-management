using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication.Interfaces
{
    public interface IUserIdentityService
    {
        Task<(bool Succeeded, Guid UserId, IReadOnlyList<string> Errors)>CreateUserAsync(
            string userName,
            string email,
            string password,
            CancellationToken cancellationToken);

        Task<(
            bool Succeeded,
            Guid UserId,
            string Email,
            string Role)> ValidateCredentialsAsync(
                string email,
                string password,
                CancellationToken cancellationToken);
    }
}
