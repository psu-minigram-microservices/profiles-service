namespace Minigram.Core.Middleware
{
    using System.Net;
    using System.Text.Json;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Minigram.Core.Exceptions;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _env = env;
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
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.NotFound, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt on {Path}", context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.Unauthorized, ex);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, ex);
            }
        }

        private async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, Exception exception)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            object response;

            if (_env.IsDevelopment())
            {
                response = new
                {
                    statusCode = (int)statusCode,
                    message = exception.Message,
                    exceptionType = exception.GetType().Name,
                    stackTrace = exception.StackTrace,
                };
            }
            else
            {
                var errorMessage = (int)statusCode < 500 ? exception.Message 
                    : "An unexpected error occurred. Please try again later.";

                response = new
                {
                    statusCode = (int)statusCode,
                    message = errorMessage,
                };
            }

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}