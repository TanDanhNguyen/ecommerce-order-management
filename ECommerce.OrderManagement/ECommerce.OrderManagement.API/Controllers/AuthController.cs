using ECommerce.OrderManagement.Application.Authentication;
using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly RegisterService _registerService;
        private readonly LoginService _loginService;

        public AuthController(RegisterService registerService, LoginService loginService)
        {
            _registerService = registerService;
            _loginService = loginService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RegisterResponse>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _registerService.RegisterAsync(
                request,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("login")]
        [ProducesResponseType(
        typeof(LoginResponse),
        StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _loginService.LoginAsync(
                request,
                cancellationToken);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email");

            return Ok(new
            {
                UserId = userId,
                Email = email
            });
        }
    }
}
