using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication.DTOs
{
    public sealed record RegisterRequest(
        string UserName,
        string Email,
        string Password);
}
