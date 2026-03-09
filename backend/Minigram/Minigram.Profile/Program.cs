namespace Minigram.Profile
{
    using System.Text;
    using System.Text.Json.Serialization;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.OpenApi;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Minigram.Core.Context;
    using Minigram.Core.Extensions;
    using Minigram.Core.Conventions;
    using Minigram.Core.Middleware;
    using Minigram.Core.Repositories;
    using Minigram.Profile.Models;
    using Minigram.Profile.Options;
    using Minigram.Profile.Services;


    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            JwtOptions? jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
            ArgumentNullException.ThrowIfNull(jwtOptions);

            Validator.ValidateObject(jwtOptions, new ValidationContext(jwtOptions), true);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
                    };
                });

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

            builder.Services.AddApiVersioning(new ApiVersion(1, 0));
            builder.Services.AddVersionedApiExplorer();

            builder.Services.Configure<RouteOptions>(options =>
                options.LowercaseUrls = true);

            builder.Services.AddDbContext<BaseDbContext, ApplicationContext>(options => 
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddSingleton<CurrentUserService>();

            builder.Services.AddScoped<IRepository<Profile>, BaseRepository<Profile>>();
            builder.Services.AddScoped<IRepository<Relation>, BaseRepository<Relation>>();

            builder.Services.AddScoped<ProfileService>();
            builder.Services.AddScoped<RelationService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                {
                    Title = "Minigram.Profile API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                });

                options.AddSecurityRequirement(document =>
                    new() { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
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

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseExceptionHandling();

            app.MapControllers().RequireAuthorization();
            app.Run();
        }
    }
}
