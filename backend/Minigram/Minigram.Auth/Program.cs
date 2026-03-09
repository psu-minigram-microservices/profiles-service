namespace Minigram.Auth
{
    using System.Text.Json.Serialization;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Auth.Models;
    using Minigram.Core.Context;
    using Minigram.Core.Middleware;
    using Minigram.Core.Repositories;

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

            builder.Services.AddDbContext<BaseDbContext, ApplicationContext>(options => 
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IRepository<User>, BaseRepository<User>>();
            builder.Services.AddScoped<IRepository<RefreshSession>, BaseRepository<RefreshSession>>();

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
