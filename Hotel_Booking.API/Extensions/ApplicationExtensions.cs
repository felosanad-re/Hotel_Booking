using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Hotel_Booking.Core.GenericRepo;
using Hotel_Booking.Core.Model;
using Hotel_Booking.Core.Options;
using Hotel_Booking.Core.Services.Contract;
using Hotel_Booking.Core.Services.Contract.AttachmentServices;
using Hotel_Booking.Core.UnitOfWork;
using Hotel_Booking.Repositories.GenericRepos;
using Hotel_Booking.Repositories.UnitOfWorks;
using Hotel_Booking.Services;
using Hotel_Booking.Services.AttachmentServices;
using Hotel_Booking.Services.CreateTokenServices;
using System.Text;

namespace Hotel_Booking.API.Extensions
{
    public static class ApplicationExtensions
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<ICreateToken, CreateTokenService>();
            services.AddScoped<IDbInitialize, DbInitialize>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepo<>));

            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            // JWT Options
            services.Configure<JwtOptions>(jwtSection);

            // File Settings Options (used by AttachmentService)
            services.Configure<FileSettingsOptions>(configuration.GetSection(FileSettingsOptions.SectionName));

            var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtOptions.ClockSkewMinutes),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                };
            });
            return services;
        }
    }
}
