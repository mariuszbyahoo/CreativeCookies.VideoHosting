using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.OAuth;
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

            services.AddScoped<IFilmsRepository, FilmsRepository>();
            services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IConnectAccountsRepository, ConnectAccountsRepository>();
            services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IClientStore, ClientStore>();

            return services;
        }
    }
}
