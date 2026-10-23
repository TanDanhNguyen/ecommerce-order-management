using ECommerce.OrderManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.OrderManagement.API.Middleware
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                KeyNotFoundException =>
                    StatusCodes.Status404NotFound,

                OrderConcurrencyException =>
                    StatusCodes.Status409Conflict,

                DuplicateProductSkuException =>
                    StatusCodes.Status409Conflict,

                InsufficientStockException =>
                    StatusCodes.Status409Conflict,

                InvalidOperationException =>
                    StatusCodes.Status409Conflict,

                UserAlreadyExistsException =>
                    StatusCodes.Status409Conflict,

                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,
            _ =>
                    StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred.");
            }

            var title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                _ => "Internal Server Error"
            };

            var detail = statusCode ==
                StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}
