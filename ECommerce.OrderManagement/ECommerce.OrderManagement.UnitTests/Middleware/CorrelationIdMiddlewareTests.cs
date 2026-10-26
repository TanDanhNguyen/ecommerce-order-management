using ECommerce.OrderManagement.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests.Middleware
{
    public class CorrelationIdMiddlewareTests
    {
        private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdProvided_ReturnsSameIdInResponse()
        {
            // Arrange
            const string correlationId = "test-correlation-123";
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Correlation-ID"] = correlationId;

            var middleware = new CorrelationIdMiddleware(
                _ => Task.CompletedTask,
                _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(
                correlationId,
                context.Response.Headers["X-Correlation-ID"].ToString());

            Assert.Equal(correlationId, context.TraceIdentifier);
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdMissing_GeneratesId()
        {
            // Arrange
            var context = new DefaultHttpContext();

            var middleware = new CorrelationIdMiddleware(
                _ => Task.CompletedTask,
                _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId =
                context.Response.Headers["X-Correlation-ID"].ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.Equal(32, correlationId.Length);
            Assert.Equal(correlationId, context.TraceIdentifier);
            Assert.True(Guid.TryParseExact(correlationId, "N", out _));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid\nid")]
        public async Task InvokeAsync_WhenCorrelationIdInvalid_GeneratesNewId(
            string invalidCorrelationId)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Correlation-ID"] = invalidCorrelationId;

            var middleware = new CorrelationIdMiddleware(
                _ => Task.CompletedTask,
                _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId =
                context.Response.Headers["X-Correlation-ID"].ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.NotEqual(invalidCorrelationId, correlationId);
            Assert.True(Guid.TryParseExact(correlationId, "N", out _));
        }


        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdExceeds128Characters_GeneratesNewId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var invalidCorrelationId = new string('a', 129);

            context.Request.Headers["X-Correlation-ID"] = invalidCorrelationId;

            var middleware = new CorrelationIdMiddleware(
                _ => Task.CompletedTask,
                _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId =
                context.Response.Headers["X-Correlation-ID"].ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.NotEqual(invalidCorrelationId, correlationId);
            Assert.True(Guid.TryParseExact(correlationId, "N", out _));
            Assert.Equal(correlationId, context.TraceIdentifier);
        }

    }
}
