
using Azure.Storage;
using Azure.Storage.Blobs;
using CreativeCookies.VideoHosting.API.Helpers;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using CreativeCookies.VideoHosting.DAL.Config;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.Azure;
using CreativeCookies.VideoHosting.Domain.BackgroundWorkers.CreativeCookies.VideoHosting.Domain.Services;
using CreativeCookies.VideoHosting.Domain.Repositories;
using CreativeCookies.VideoHosting.Domain.Repositories.OAuth;
using CreativeCookies.VideoHosting.Domain.Stripe;
using CreativeCookies.VideoHosting.Infrastructure;
using CreativeCookies.VideoHosting.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Configuration;
using System.Text;

namespace CreativeCookies.VideoHosting.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                var env = hostingContext.HostingEnvironment;

                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Verbose);

                if (env.IsDevelopment())
                {
                    loggerConfiguration
                        .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.File(new Serilog.Formatting.Display.MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null),
                                      "logs/log-.txt",
                                      rollingInterval: RollingInterval.Day,
                                      retainedFileCountLimit: 7,
                                      restrictedToMinimumLevel: LogEventLevel.Information);
                }
                else
                {
                    var instrumentationKey = hostingContext.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

                    loggerConfiguration
                                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Warning)
                                .WriteTo.ApplicationInsights(new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(instrumentationKey)), TelemetryConverter.Traces);
                }
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Production",
                    builder => builder
                    .WithOrigins("https://myhub.com.pl", "https://streambeacon.azurewebsites.net") 
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());

                options.AddPolicy("Development", builder => builder
                    .WithOrigins("https://localhost:44495")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });
            var connectionString = "";

            if (builder.Environment.IsDevelopment())
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            else if (builder.Environment.IsProduction()) 
            {
                connectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            }

            builder.Services.AddDataAccessLayer(connectionString);

            var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IClientStore, ClientStore>();

            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            builder.Services.AddScoped<IJWTRepository, JWTRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            builder.Services.AddScoped<IConnectAccountsRepository, ConnectAccountsRepository>();
            builder.Services.AddSingleton<ISasTokenRepository, SasTokenRepository>();

            builder.Services.AddScoped<IFilmService, FilmService>();

            builder.Services.AddScoped<IMyHubBlobService, MyHubBlobService>();

            builder.Services.AddTransient<IEmailService>(serviceProvider =>
            {
                int smtpPort;
                var logger = serviceProvider.GetRequiredService<ILogger<EmailService>>();
                var hasPortBeenParsed = int.TryParse(builder.Configuration.GetValue<string>("SMTPPort"), out smtpPort);
                var host = builder.Configuration.GetValue<string>("SMTPHost");
                var user = builder.Configuration.GetValue<string>("SMTPUser");
                var password = builder.Configuration.GetValue<string>("SMTPPassword");
                if (!hasPortBeenParsed || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogError("One of the SMTP configuration is invalid or not set. Please ensure all of the SMTP config key value pairs have been populated and is SMTPPort a valid integer.");
                    throw new ConfigurationErrorsException("One of the SMTP configuration is invalid or not set. Please ensure all of the SMTP config key value pairs have been populated and is SMTPPort a valid integer.");
                }
                var razorViewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
                var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();
                return new EmailService(logger, host, smtpPort, user, password, razorViewEngine, tempDataProvider, serviceProvider);
            });


            var accountName = builder.Configuration.GetValue<string>("Storage:AccountName");
            var accountKey = builder.Configuration.GetValue<string>("Storage:AccountKey");
            var blobServiceUrl = builder.Configuration.GetValue<string>("Storage:BlobServiceUrl");
            builder.Services.AddSingleton(x => new StorageSharedKeyCredential(accountName, accountKey));
            builder.Services.AddSingleton(x => new BlobServiceClient(new Uri(blobServiceUrl), x.GetRequiredService<StorageSharedKeyCredential>()));
            builder.Services.AddSingleton<IBlobServiceClientWrapper>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                return new BlobServiceClientWrapper(blobServiceClient);
            });
            builder.Services.AddSingleton<IStripeService, StripeService>();
            
            builder.Services.AddHostedService<TokenCleanupWorker>();

            var clientId = builder.Configuration.GetValue<string>("ClientId");

            var jwtSecretKey = builder.Configuration.GetValue<string>("JWTSecretKey");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = apiUrl,
                    ValidAudience = clientId.ToString(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
                }; // HACK: hardcoded values above prohibits usage of any other external IdPs 
                options.Events = new JwtBearerEvents();
                options.Events.OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("stac"))
                    {
                        context.Token = context.Request.Cookies["stac"];
                    }

                    return Task.CompletedTask;
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); 
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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
                var adminEmail = builder.Configuration.GetValue<string>("AdminEmail");
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

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/StatusCode");
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseCors("Development");
            }
            else
            {
                app.UseCors("Production");
            }

            app.UseHttpsRedirection();

            app.UseStatusCodePagesWithReExecute("/StatusCode", "?statusCode={0}");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}