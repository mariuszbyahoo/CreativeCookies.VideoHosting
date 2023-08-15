using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.Repositories;
using CreativeCookies.VideoHosting.DAL.Repositories.OAuth;
using Microsoft.AspNetCore.Builder;
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

            services.AddScoped<IFilmsRepository, FilmsRepository>();
            services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IConnectAccountsRepository, ConnectAccountsRepository>();
            services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IClientStore, ClientStore>();

            return services;
        }

        public static WebApplication MigrateAndPopulateDatabase(this WebApplication app, string adminEmail)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Ensure the roles exist
                var roles = new[] { "admin", "subscriber", "nonsubscriber" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }

                // Create an admin user
                var adminUser = userManager.FindByEmailAsync(adminEmail)?.Result;

                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    adminUser.EmailConfirmed = true;
                    var result = userManager.CreateAsync(adminUser, "Pass123$").Result;
                    if (result.Succeeded)
                    {
                        // HACK: Send an email about creation of the user to adminEmail
                        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                    }
                }
            }
            return app;
        }
    }
}
