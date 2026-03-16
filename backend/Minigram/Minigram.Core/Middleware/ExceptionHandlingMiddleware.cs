namespace Minigram.Core.Middleware
{
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Minigram.Core.Dto;
    using Minigram.Core.Exceptions;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ExceptionHandlingMiddleware(RequestDelegate next,IWebHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
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

            var dto = _env.IsDevelopment() ?
                BuildDevelopmentResponse(statusCode, exception) :
                BuildProductionResponse(statusCode, exception);

            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await context.Response.WriteAsync(json);
        }

        private static ErrorResponse BuildDevelopmentResponse(HttpStatusCode statusCode, Exception exception)
        {
            return new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = exception.Message,
                ExceptionType = exception.GetType().Name,
                StackTrace = exception.StackTrace,
            };
        }

        private static ErrorResponse BuildProductionResponse(HttpStatusCode statusCode, Exception exception)
        {
            var message = statusCode switch
            {
                HttpStatusCode.NotFound => "The requested resource was not found.",
                HttpStatusCode.Unauthorized => "Authentication is required to access this resource.",
                HttpStatusCode.BadRequest => "The request contains invalid data.",
                HttpStatusCode.Forbidden => "Do not have permission to access this resource.",
                _ => "An unexpected error occurred. Please try again later."
            };

            return new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
            };
        }
    }
}