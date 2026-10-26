using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public static class TestJwtTokenFactory
    {
        public static string CreateToken(
            IConfiguration configuration,
            string role)
        {
            var jwt = configuration.GetSection("Jwt");

            var issuer = jwt["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is missing.");

            var audience = jwt["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is missing.");

            var signingKey = jwt["SigningKey"]
                ?? throw new InvalidOperationException(
                    "JWT SigningKey is missing.");

            var claims = new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                Guid.NewGuid().ToString()),
            new Claim(
                ClaimTypes.Role,
                role),
            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
