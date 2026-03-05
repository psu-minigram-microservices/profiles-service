namespace Minigram.Auth
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json.Serialization;
    using Minigram.Auth.Services;
    using Minigram.Core.ApplicationContext;
    using Minigram.Core.Models;
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

            builder.Services.AddDbContext<ApplicationContext>(options => 
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddSingleton<CurrentUserService>();

            builder.Services.AddScoped<IRepository<User>, BaseRepository<User>>();
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
