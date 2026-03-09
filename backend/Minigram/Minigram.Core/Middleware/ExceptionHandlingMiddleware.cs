namespace Minigram.Core.Middleware
{
    using System.Net;
    using System.Text.Json;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Minigram.Core.Exceptions;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entity not found: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt on {Path}", context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode,
                message
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}