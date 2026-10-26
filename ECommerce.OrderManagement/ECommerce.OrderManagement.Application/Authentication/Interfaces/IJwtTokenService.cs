using ECommerce.OrderManagement.Application.Authentication.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication.Interfaces
{
    public interface IJwtTokenService
    {
        LoginResponse CreateToken(Guid userId, string email, string role);
    }
}
