using Microsoft.EntityFrameworkCore;
using Hotel_Booking.Core.Services.Contract;
using Hotel_Booking.Repositories.Data;
using Hotel_Booking.Repositories.Data.DataSeeding;

namespace Hotel_Booking.API.Extensions
{
    public static class InitializeDataBase
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            // Add Scope
            using var scope = app.Services.CreateScope();
            // Add Services
            var services = scope.ServiceProvider;
            // Create object From DbContext Implicitly
            var _context = services.GetRequiredService<DbContextClass>();
            // Create object From DbInitialize Implicitly
            var _dbInitialization = services.GetRequiredService<IDbInitialize>();
            var logger = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await _context.Database.MigrateAsync();
                await _dbInitialization.CreateInitializationAsync();
                await TestDbContextSeeder.SeederAsync(_context);
            }
            catch (Exception ex)
            {
                var _logger = logger.CreateLogger<Program>();
                _logger.LogError(ex, "Error in database");
            }
        }
    }
}
