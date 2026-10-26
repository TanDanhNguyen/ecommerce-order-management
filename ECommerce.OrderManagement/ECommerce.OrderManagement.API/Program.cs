using ECommerce.OrderManagement.API.Middleware;
using ECommerce.OrderManagement.API.Services;
using ECommerce.OrderManagement.Application;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure;
using ECommerce.OrderManagement.Infrastructure.Identity;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Debugging;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "[{CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog();


    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(
                    context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed.",
                    Detail = "One or more validation errors occurred.",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails);
            };
        });

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ECommerceDbContext>(
        name: "database",
        tags: new[] { "ready" });

builder.Services.AddRateLimiter(options =>
{
options.RejectionStatusCode =
    StatusCodes.Status429TooManyRequests;

options.OnRejected = (context, cancellationToken) =>
{
if (context.Lease.TryGetMetadata(
    MetadataName.RetryAfter,
    out var retryAfter))
{
var seconds = Math.Max(
    1,
    (int)Math.Ceiling(retryAfter.TotalSeconds));

context.HttpContext.Response.Headers.RetryAfter =
    seconds.ToString(
        CultureInfo.InvariantCulture);
}

return ValueTask.CompletedTask;
};

options.AddPolicy("CreateOrderPolicy", httpContext =>
{
var userId = httpContext.User.FindFirst(
    ClaimTypes.NameIdentifier)?.Value;

if (string.IsNullOrWhiteSpace(userId))
{
return RateLimitPartition.GetFixedWindowLimiter(
    "unauthenticated",
    _ => new FixedWindowRateLimiterOptions
{
PermitLimit = 5,
Window = TimeSpan.FromMinutes(1),
QueueLimit = 0,
AutoReplenishment = true
});
}

return RateLimitPartition.GetFixedWindowLimiter(
    userId,
    _ => new FixedWindowRateLimiterOptions
{
PermitLimit = 5,
Window = TimeSpan.FromMinutes(1),
QueueLimit = 0,
AutoReplenishment = true
});
});
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
Name = "Authorization",
Type = SecuritySchemeType.Http,
Scheme = "bearer",
BearerFormat = "JWT",
In = ParameterLocation.Header,
Description = "Enter your JWT access token."
});

options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

var jwtSection = builder.Configuration.GetSection("Jwt");

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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
options.TokenValidationParameters =
    new TokenValidationParameters
{
ValidateIssuer = true,
ValidIssuer = issuer,

ValidateAudience = true,
ValidAudience = audience,

ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey)),

ValidateLifetime = true,
ClockSkew = TimeSpan.FromSeconds(30)
};
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IOrderTransaction, OrderTransaction>();

var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await IdentityRoleSeeder.SeedAsync(roleManager);
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, exception) =>
        {
            if (exception != null || httpContext.Response.StatusCode >= 500)
                return Serilog.Events.LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 400)
                return Serilog.Events.LogEventLevel.Warning;

            return Serilog.Events.LogEventLevel.Information;
        };
    });

    app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapHealthChecks(
    "/health/ready",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
Predicate = check => check.Tags.Contains("ready")
});

app.Run();
}
catch (Exception ex)
{
Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
Log.CloseAndFlush();
}

public partial class Program { }