using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DAL.Repositories;
using CreativeCookies.VideoHosting.DAL.Repositories.OAuth;
using Hangfire;
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

            services.AddDefaultIdentity<MyHubUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.ProviderMap[TokenOptions.DefaultAuthenticatorProvider] = new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<MyHubUser>));
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddHangfire(conf => conf.UseSqlServerStorage(connectionString));

            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IFilmsRepository, FilmsRepository>();
            services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IConnectAccountsRepository, ConnectAccountsRepository>();
            services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IClientStore, ClientStore>();
            services.AddScoped<IAboutPageRepository, AboutPageRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();

            return services;
        }

        public static WebApplication MigrateAndPopulateDatabase(this WebApplication app, string adminEmail)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MyHubUser>>();

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
                    adminUser = new MyHubUser { UserName = adminEmail, Email = adminEmail };
                    adminUser.EmailConfirmed = true;
                    var result = userManager.CreateAsync(adminUser, "Pass123$").Result;
                    if (result.Succeeded)
                    {
                        // HACK: Send an email about creation of the user to adminEmail
                        userManager.AddToRoleAsync(adminUser, "admin").Wait();
                    }
                }

                // HACK: Add initial homepage's contents
            }
            return app;
        }
    }
}
