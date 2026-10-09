using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Hotel_Booking.Repositories.Data
{
    public class DbContextClass : IdentityDbContext
    {
        public DbContextClass(DbContextOptions<DbContextClass> options)
            :base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder); // For Identity
        }
    }
}
