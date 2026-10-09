using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Hotel_Booking.Repositories.Data
{
    public class TestDbContextFactory : IDesignTimeDbContextFactory<DbContextClass>
    {
        public DbContextClass CreateDbContext(string[] args)
        {
            // Build configuration from the API project's appsettings
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Courses.Api");
            if (!File.Exists(Path.Combine(basePath, "appsettings.Development.json")))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DbContextClass>();
            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'Default' not found. Ensure appsettings.Development.json exists in the Courses.Api project with a valid connection string.");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new DbContextClass(optionsBuilder.Options);
        }
    }
}
