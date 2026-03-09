namespace Minigram.Auth
{
    using System.Text.Json.Serialization;
    using Microsoft.OpenApi;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Minigram.Core.Context;
    using Minigram.Core.Extensions;
    using Minigram.Core.Conventions;
    using Minigram.Core.Middleware;
    using Minigram.Core.Repositories;
    using Minigram.Auth.Models;
    using Minigram.Auth.Options;
    using Minigram.Auth.Services;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers(options =>
                {
                    options.Conventions.Add(new ApiVersionRouteConvention());
                })
                .AddNewtonsoftJson()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddApiVersioning(new ApiVersion(1, 0));
            builder.Services.AddVersionedApiExplorer();

            builder.Services.Configure<RouteOptions>(options =>
                options.LowercaseUrls = true);

            builder.Services.AddDbContext<BaseDbContext, ApplicationContext>(options => 
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

            builder.Services.AddScoped<IRepository<User>, BaseRepository<User>>();
            builder.Services.AddScoped<IRepository<RefreshSession>, BaseRepository<RefreshSession>>();

            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<UserService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                {
                    Title = "Minigram.Auth API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
            }
            
            app.UseExceptionHandling();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
