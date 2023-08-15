using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CreativeCookies.VideoHosting.DAL.Config
{
    public static class DALExtensions
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, string connectionString)
        {
            // Register the DbContext, other DAL related services

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.ProviderMap[TokenOptions.DefaultAuthenticatorProvider] = new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<IdentityUser>));
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            // Register repository implementations
            services.AddScoped<IFilmsRepository, FilmsRepository>();
            services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            // ... other repository registrations

            return services;
        }
    }
}
