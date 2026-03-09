namespace Minigram.Profile
{
    using System.Text.Json.Serialization;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Context;
    using Minigram.Core.Middleware;
    using Minigram.Core.Repositories;
    using Minigram.Profile.Models;
    using Minigram.Profile.Services;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddNewtonsoftJson()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

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

            app.UseExceptionHandling();
            app.UseRequestLogging();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
