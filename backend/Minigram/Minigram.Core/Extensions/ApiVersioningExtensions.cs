namespace Minigram.Core.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;

    public static class ApiVersioningExtensions
    {
        public static IServiceCollection AddApiVersioning(this IServiceCollection services, ApiVersion version)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = version;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            return services;
        }

        public static IServiceCollection AddVersionedApiExplorer(this IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}