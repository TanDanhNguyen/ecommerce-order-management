using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly RegisterService _registerService;

        public AuthController(RegisterService registerService)
        {
            _registerService = registerService;
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
    }
}
