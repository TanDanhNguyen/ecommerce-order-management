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

                ProductConcurrencyException =>
                    StatusCodes.Status409Conflict,

                UnauthorizedAccessException =>
                    StatusCodes.Status401Unauthorized,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                _ => StatusCodes.Status500InternalServerError
            };

            var traceId = httpContext.TraceIdentifier;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    traceId);
            }
            else
            {
                _logger.LogWarning(
                    "Request failed with status {StatusCode} for {Method} {Path}. TraceId: {TraceId}",
                    statusCode,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    traceId);
            }

            var title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                _ => "Internal Server Error"
            };

            var detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

    }
}
