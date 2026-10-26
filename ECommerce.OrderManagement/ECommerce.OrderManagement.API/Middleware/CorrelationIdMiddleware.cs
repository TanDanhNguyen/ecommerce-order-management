namespace ECommerce.OrderManagement.API.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName]
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId)
                || correlationId.Length > 128
                || correlationId.Any(char.IsControl))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            context.TraceIdentifier = correlationId;

            context.Response.Headers[HeaderName] = correlationId;

            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
            {
                _logger.LogInformation(
                    "Handling HTTP {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);
            }
        }
    }
}
