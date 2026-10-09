using Microsoft.AspNetCore.Identity;
using Hotel_Booking.Core.Model.Users;
using Hotel_Booking.Core.Options;
using Hotel_Booking.Core.ProjectRoles;
using Hotel_Booking.Core.Services.Contract;
using System.Data;

namespace Hotel_Booking.Services
{
    public class DbInitialize : IDbInitialize
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SeedAdminOptions _seedAdminOptions;

        public DbInitialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SeedAdminOptions seedAdminOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _seedAdminOptions = seedAdminOptions;
        }


        public async Task CreateInitializationAsync()
        {
            string[] rolesName = [Roles.Admin, Roles.RoleTwo, Roles.RoleThree];

            foreach (var role in rolesName)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!_seedAdminOptions.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_seedAdminOptions.Email) ||
                string.IsNullOrWhiteSpace(_seedAdminOptions.UserName) ||
                string.IsNullOrWhiteSpace(_seedAdminOptions.Password))
            {
                throw new InvalidOperationException("Seed admin is enabled, but the required settings are missing.");
            }

            var existingUser = await _userManager.FindByEmailAsync(_seedAdminOptions.Email);
            if (existingUser is not null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                FirstName = _seedAdminOptions.FirstName,
                LastName = _seedAdminOptions.LastName,
                UserName = _seedAdminOptions.UserName,
                Email = _seedAdminOptions.Email,
                Address = _seedAdminOptions.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, _seedAdminOptions.Password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(Environment.NewLine, result.Errors.Select(error => error.Description));
                throw new InvalidOperationException(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, Roles.Admin);
        }
    }
}
