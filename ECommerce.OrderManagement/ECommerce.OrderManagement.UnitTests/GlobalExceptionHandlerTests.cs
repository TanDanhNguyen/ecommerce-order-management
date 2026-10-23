using ECommerce.OrderManagement.API.Middleware;
using ECommerce.OrderManagement.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests
{
    using ECommerce.OrderManagement.Application.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using System.Text.Json;
    using Xunit;

    public class GlobalExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_WhenOrderConcurrencyExceptionOccurs_Returns409ProblemDetails()
        {
            // Arrange
            var logger = Mock.Of<ILogger<GlobalExceptionHandler>>();
            var handler = new GlobalExceptionHandler(logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var exception = new OrderConcurrencyException();

            // Act
            var handled = await handler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);
            Assert.Equal(
                StatusCodes.Status409Conflict,
                httpContext.Response.StatusCode);

            Assert.StartsWith(
                "application/json",
                httpContext.Response.ContentType);

            httpContext.Response.Body.Position = 0;

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
                httpContext.Response.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(problemDetails);
            Assert.Equal(409, problemDetails.Status);
            Assert.Equal("Conflict", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
            Assert.Equal(string.Empty, problemDetails.Instance);
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_Returns500WithoutExposingInternalDetails()
        {
            // Arrange
            var logger = NullLogger<GlobalExceptionHandler>.Instance;
            var handler = new GlobalExceptionHandler(logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var exception = new Exception(
                "Sensitive database connection details");

            // Act
            var handled = await handler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
                httpContext.Response.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(problemDetails);
            Assert.Equal(500, problemDetails.Status);
            Assert.Equal("Internal Server Error", problemDetails.Title);
            Assert.Equal(
                "An unexpected error occurred.",
                problemDetails.Detail);

            Assert.DoesNotContain(
                "Sensitive database connection details",
                problemDetails.Detail);
        }


        [Fact]
        public async Task TryHandleAsync_WhenKeyNotFoundExceptionOccurs_Returns404ProblemDetails()
        {
            // Arrange
            var logger = NullLogger<GlobalExceptionHandler>.Instance;
            var handler = new GlobalExceptionHandler(logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/orders/123";
            httpContext.Response.Body = new MemoryStream();

            var exception = new KeyNotFoundException("Order not found.");

            // Act
            var handled = await handler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);
            Assert.Equal(
                StatusCodes.Status404NotFound,
                httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
                httpContext.Response.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Not Found", problemDetails.Title);
            Assert.Equal("Order not found.", problemDetails.Detail);
            Assert.Equal("/api/orders/123", problemDetails.Instance);
        }


        [Fact]
        public async Task TryHandleAsync_WhenArgumentExceptionOccurs_Returns400ProblemDetails()
        {
            // Arrange
            var logger = NullLogger<GlobalExceptionHandler>.Instance;
            var handler = new GlobalExceptionHandler(logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/products";

            httpContext.Response.Body = new MemoryStream();

            var exception = new ArgumentException("Product name is required.");

            // Act
            var handled = await handler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(
                StatusCodes.Status400BadRequest,
                httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
                httpContext.Response.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.Equal("Bad Request", problemDetails.Title);
            Assert.Equal("Product name is required.", problemDetails.Detail);
            Assert.Equal("/api/products", problemDetails.Instance);
        }


        [Fact]
        public async Task TryHandleAsync_WhenUnauthorizedAccessExceptionOccurs_Returns401ProblemDetails()
        {
            // Arrange
            var logger = NullLogger<GlobalExceptionHandler>.Instance;
            var handler = new GlobalExceptionHandler(logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/orders";

            httpContext.Response.Body = new MemoryStream();

            var exception = new UnauthorizedAccessException(
                "An authenticated user is required.");

            // Act
            var handled = await handler.TryHandleAsync(
                httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(
                StatusCodes.Status401Unauthorized,
                httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
                httpContext.Response.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(problemDetails);
            Assert.Equal(401, problemDetails.Status);
            Assert.Equal("Unauthorized", problemDetails.Title);
            Assert.Equal(
                "An authenticated user is required.",
                problemDetails.Detail);
            Assert.Equal("/api/orders", problemDetails.Instance);
        }

    }
}

