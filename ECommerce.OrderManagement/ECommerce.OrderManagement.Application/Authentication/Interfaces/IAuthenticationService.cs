using ECommerce.OrderManagement.Application.Authentication.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication.Interfaces
{
    public interface IAuthenticationService
    {
        Task<RegisterResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken);
    }
}
