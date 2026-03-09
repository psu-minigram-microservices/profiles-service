namespace Minigram.Auth
{
    using System.Text.Json.Serialization;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Auth.Models;
    using Minigram.Core.Context;
    using Minigram.Core.Repositories;
    using Npgsql;

    public class Program
    {
        public static async Task Main(string[] args)
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

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BaseDbContext>();

                try
                {
                    if (!await context.Database.CanConnectAsync())
                    {

                        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                        var masterConnectionString = connectionString.Replace("Database=minigram-db", "Database=postgres");

                        using var masterConnection = new NpgsqlConnection(masterConnectionString);
                        await masterConnection.OpenAsync();

                        using var cmd = new NpgsqlCommand($"CREATE DATABASE \"minigram-db\"", masterConnection);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Create Database: {ex.Message}");
                    throw;
                }

                try
                {
                    await context.Database.MigrateAsync();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration Error: {ex.Message}");
                    throw;
                }
            }

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
