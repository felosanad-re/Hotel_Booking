using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Hotel_Booking.API.ErrorHandler;
using Hotel_Booking.API.Extensions;
using Hotel_Booking.Core.Model.Users;
using Hotel_Booking.Repositories.Data;
using Serilog;

namespace Hotel_Booking.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                #region Use Serlog
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateLogger();

                builder.Host.UseSerilog();
                #endregion

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();
                builder.Services.AddSwaggerGen();

                // Add Identity
                builder.Services.AddIdentity<ApplicationUser, IdentityUser>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                    .AddEntityFrameworkStores<DbContextClass>().AddDefaultTokenProviders();

                // Add DbContext
                builder.Services.AddDbContext<DbContextClass>(options =>
                {
                    options.UseSqlServer(BuildConnectionString(builder.Configuration));
                });

                // Add Application Services (JWT auth configured here overrides Identity cookies)
                builder.Services.AddApplicationServices(builder.Configuration);

                var allowedOrigin = builder.Configuration.GetSection("AllowCORS").Get<string[]>();
                var allowedOringin = builder.Configuration.GetSection("AllowCORS").Get<string[]>();
                builder.Services.AddCors(action =>
                {
                    action.AddPolicy("Angular", options =>
                    {
                        options.WithOrigins(allowedOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
                });

                var app = builder.Build();

                await app.InitializeDatabaseAsync();

                // Custom middleware
                app.UseMiddleware<ExceptionMiddleware>();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseCors("Angular");
                app.UseHttpsRedirection();

                app.UseStaticFiles();
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();
            }
            catch (Exception ex)
            {
                WriteStartupFailure(ex);
                throw;
            }
        }

        #region Helper Methods
        private static string BuildConnectionString(IConfiguration configuration)
        {
            // Try get ConnectionString
            var configuredConnectionString = configuration.GetConnectionString("Defult");
            if (string.IsNullOrWhiteSpace(configuredConnectionString))
                throw new InvalidOperationException("Connection string 'Default' is missing.");

            // Try Get Password
            var connectionStringsBuilder = new SqlConnectionStringBuilder(configuredConnectionString);
            var passwordFromSecret = configuration["ConnectionStrings:DefaultPassword"];

            if (string.IsNullOrWhiteSpace(connectionStringsBuilder.Password))
            {
                // Try Get Password For Def
                var passwordFromFallbackProvider = TryGetPasswordFromFallbackConnectionString(configuration);
                if(!string.IsNullOrWhiteSpace(passwordFromFallbackProvider))
                    connectionStringsBuilder.Password = passwordFromFallbackProvider;
            }

            if(string.IsNullOrWhiteSpace(connectionStringsBuilder.Password))
            {
                var passwordFromProductionFile = TryGetPasswordFromJsonFile(configuration, "appsettings.Production.Json");
                if (!string.IsNullOrWhiteSpace(passwordFromProductionFile))
                    connectionStringsBuilder.Password = passwordFromProductionFile;
            }

            if (!string.IsNullOrWhiteSpace(passwordFromSecret))
                connectionStringsBuilder.Password = passwordFromSecret;

            if (!connectionStringsBuilder.IntegratedSecurity && string.IsNullOrWhiteSpace(connectionStringsBuilder.Password))
                throw new InvalidOperationException("Database password is missing. Set 'ConnectionStrings__DefaultPassword' or provide a full 'ConnectionStrings__Default' value outside git.");

            return connectionStringsBuilder.ConnectionString;
        }

        private static string? TryGetPasswordFromFallbackConnectionString(IConfiguration configuration)
        {
            if (configuration is not IConfigurationRoot configurationRoot)
                return null;

            foreach (var provider in configurationRoot.Providers)
            {
                if(!provider.TryGet("ConnectionStrings:Password", out var candidateConnectionString) || string.IsNullOrWhiteSpace(candidateConnectionString))
                    continue;

                try
                {
                    var candidatebuilder = new SqlConnectionStringBuilder(candidateConnectionString);
                    if (!string.IsNullOrWhiteSpace(candidatebuilder.Password))
                        return candidatebuilder.Password;
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        private static string? TryGetPasswordFromJsonFile(IConfiguration configuration, string fileName)
        {
            var contentRoot = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            if (string.IsNullOrWhiteSpace(contentRoot))
            {
                contentRoot = AppContext.BaseDirectory;
            }

            var fullPath = Path.Combine(contentRoot, fileName);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            try
            {
                var directConfiguration = new ConfigurationBuilder()
                    .SetBasePath(contentRoot)
                    .AddJsonFile(fileName, optional: false, reloadOnChange: false)
                    .Build();

                var candidateConnectionString = directConfiguration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(candidateConnectionString))
                {
                    return null;
                }

                var candidateBuilder = new SqlConnectionStringBuilder(candidateConnectionString);
                return string.IsNullOrWhiteSpace(candidateBuilder.Password) ? null : candidateBuilder.Password;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Write Error in log file
        private static void WriteStartupFailure(Exception ex)
        {
            var message = $"""
            [{DateTime.UtcNow:O}] Application startup failed.
            {FlattenException(ex)}

            """;

            foreach (var logPath in GetStartupLogPaths())
            {
                try
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(logPath, message);
                }
                catch
                {
                    // Try the next writable location.
                }
            }

            try
            {
                Console.Error.WriteLine(message);
            }
            catch
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static IEnumerable<string> GetStartupLogPaths()
        {
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "startup-error.log"),
                Path.Combine(AppContext.BaseDirectory, "logs", "startup-error.log"),
                Path.Combine(Path.GetTempPath(), "Courses.Api", "startup-error.log")
            };

            return paths.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string FlattenException(Exception ex)
        {
            var lines = new List<string>();
            var current = ex;
            var level = 0;

            while (current != null)
            {
                lines.Add($"Level {level}: {current.GetType().FullName}");
                lines.Add($"Message: {current.Message}");
                lines.Add(current.StackTrace ?? "No stack trace available.");
                lines.Add(string.Empty);
                current = current.InnerException!;
                level++;
            }

            return string.Join(Environment.NewLine, lines);
        }
        #endregion
    }
}
