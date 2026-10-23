using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication.DTOs
{
    public sealed record LoginRequest(
        string Email,
        string Password);
}
