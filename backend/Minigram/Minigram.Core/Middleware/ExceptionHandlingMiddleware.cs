namespace Minigram.Core.Middleware
{
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Minigram.Core.Dto;
    using Minigram.Core.Utils.Exceptions;

    public class ExceptionHandlingMiddleware
    {
        private const string DefaultErrorMessage = "An unexpected error occurred. Please try again later.";

        private static readonly Dictionary<HttpStatusCode, string> ErrorMessages = new ()
        {
            [HttpStatusCode.NotFound] = "The requested resource was not found.",
            [HttpStatusCode.Unauthorized] = "Authentication is required to access this resource.",
            [HttpStatusCode.BadRequest] = "The request contains invalid data.",
            [HttpStatusCode.Conflict] = "A conflict occurred while processing your request. The resource may have been modified.",
            [HttpStatusCode.Forbidden] = "Do not have permission to access this resource.",
        };

        private static readonly JsonSerializerOptions JsonOptions = new ()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

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
                await WriteErrorResponse(context, HttpStatusCode.NotFound, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteErrorResponse(context, HttpStatusCode.Unauthorized, ex);
            }
            catch (ArgumentException ex)
            {
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error on {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.Conflict, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, ex);
            }
        }

        private async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, Exception exception)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            ErrorResponse dto = new ()
            {
                StatusCode = (int)statusCode,
                Message = GetErrorMessage(statusCode, exception),
            };

            if (_env.IsDevelopment())
            {
                dto.ExceptionType = exception.GetType().Name;
                dto.StackTrace = exception.StackTrace;
            }

            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await context.Response.WriteAsync(json);
        }

        private string GetErrorMessage(HttpStatusCode statusCode, Exception exception)
        {
            if (_env.IsDevelopment())
            {
                return exception.Message;
            }
            
            return ErrorMessages.GetValueOrDefault(statusCode, DefaultErrorMessage);
        }
    }
}