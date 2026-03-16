namespace Minigram.Profile
{
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Context;
    using Minigram.Core.Extensions;
    using Minigram.Core.Conventions;
    using Minigram.Core.Repositories;
    using Minigram.Profile.Models;
    using Minigram.Profile.Services;

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
            builder.Services.AddSwaggerGen();

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

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
