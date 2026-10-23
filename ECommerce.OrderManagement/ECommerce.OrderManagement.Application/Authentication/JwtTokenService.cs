using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Authentication
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoginResponse CreateToken(Guid userId, string email)
        {
            var jwtSection = _configuration.GetSection("Jwt");

            var issuer = jwtSection["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is not configured.");

            var audience = jwtSection["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is not configured.");

            var signingKey = jwtSection["SigningKey"]
                ?? throw new InvalidOperationException(
                    "JWT SigningKey is not configured.");

            if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            {
                throw new InvalidOperationException(
                    "JWT SigningKey must be at least 32 bytes.");
            }

            var accessTokenMinutes = int.TryParse(
                jwtSection["AccessTokenMinutes"],
                out var minutes)
                    ? minutes
                    : 30;

            if (accessTokenMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "JWT AccessTokenMinutes must be greater than zero.");
            }

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(accessTokenMinutes);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new LoginResponse(
                accessToken,
                expiresAt,
                userId,
                email);
        }
    }
}
