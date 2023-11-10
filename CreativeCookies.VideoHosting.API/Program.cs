
using Azure.Storage;
using Azure.Storage.Blobs;
using CreativeCookies.VideoHosting.API.Helpers;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Configuration;
using System.Text;
using CreativeCookies.VideoHosting.DAL.Config;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Domain.BackgroundWorkers.CreativeCookies.VideoHosting.Domain.Services;
using CreativeCookies.VideoHosting.Services.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Azure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Services.IdP;
using CreativeCookies.VideoHosting.Infrastructure;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.Services.Subscriptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility;
using Hangfire;
using CreativeCookies.VideoHosting.API.Attributes;
using Hangfire.Storage;
using Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.About;
using CreativeCookies.VideoHosting.Services.About;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using CreativeCookies.VideoHosting.API.Utils.JsonStringLocalizer;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Localization;
using CreativeCookies.VideoHosting.Contracts.Repositories;

namespace CreativeCookies.VideoHosting.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "pl-PL", "en-US" };
                var localizationOptions = new RequestLocalizationOptions()
                    .SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

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
                    var appInsightsInstrumentationKey = hostingContext.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    if (string.IsNullOrWhiteSpace(appInsightsInstrumentationKey)) throw new InvalidOperationException("AppInsights Instrumentation key has not been found!");
                    loggerConfiguration
                                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Information);
                    loggerConfiguration
                                .WriteTo.ApplicationInsights(new TelemetryClient(new TelemetryConfiguration
                                {
                                    InstrumentationKey = appInsightsInstrumentationKey
                                }), TelemetryConverter.Traces);
                }
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Production",
                    builder => builder
                    .WithOrigins("https://myhub.com.pl", "https://mytube.azurewebsites.net") 
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
            else
            {
                connectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            }

            builder.Services.AddHttpContextAccessor();

            var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");            
            var storageAccountName = builder.Configuration.GetValue<string>("StorageAccountName");
            var storageAccountKey = builder.Configuration.GetValue<string>("StorageAccountKey");
            var blobServiceUrl = builder.Configuration.GetValue<string>("StorageBlobServiceUrl");
            var clientId = builder.Configuration.GetValue<string>("ClientId");
            var jwtSecretKey = builder.Configuration.GetValue<string>("JWTSecretKey");
            var adminEmail = builder.Configuration.GetValue<string>("AdminEmail");
            var appInsightsInstrumentationKey = builder.Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            builder.Services.AddDataAccessLayer(connectionString);

            builder.Services.AddApplicationInsightsTelemetry(appInsightsInstrumentationKey);
            builder.Services.AddHangfireServer();

            builder.Services.AddSingleton<ISasTokenService, SasTokenService>();
            builder.Services.AddSingleton<IJWTGenerator, JwtGenerator>();
            builder.Services.AddSingleton(sp => JobStorage.Current.GetMonitoringApi());
            builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

            builder.Services.AddScoped<IMerchantService, MerchantService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IStripeProductsService, StripeProductsService>();
            builder.Services.AddScoped<IFilmService, FilmService>();
            builder.Services.AddScoped<IErrorLogsService, ErrorLogsService>();
            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IConnectAccountsService, ConnectAccountsService>();
            builder.Services.AddScoped<IAuthorizationCodeService, AuthorizationCodeService>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
            builder.Services.AddScoped<IOAuthClientService, OAuthClientService>();
            builder.Services.AddScoped<IMyHubSignInManager, MyHubSignInManager>();
            builder.Services.AddScoped<IMyHubUserManager, MyHubUserManager>();
            builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();
            builder.Services.AddScoped<IAboutPageService, AboutPageService>();

            builder.Services.AddScoped<IMyHubBlobService, MyHubBlobService>();
            builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();

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
            var keyVaultUrl = builder.Configuration.GetValue<string>("CentralKeyVaultUrl");

            builder.Services.AddInfrastructureConfiguration(keyVaultUrl, storageAccountName, storageAccountKey, blobServiceUrl);

            builder.Services.AddSingleton<IBlobServiceClientWrapper>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                return new BlobServiceClientWrapper(blobServiceClient);
            });
            builder.Services.AddSingleton<IStripeOnboardingService, StripeOnboardingService>();
            
            builder.Services.AddHostedService<TokenCleanupWorker>();
            builder.Services.AddHostedService<StripeMessageReceiver>();
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
            builder.Services.AddRazorPages()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddRazorRuntimeCompilation(); 
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.MigrateAndPopulateDatabase(adminEmail);
            var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
            if (localizationOptions != null)
            {
                app.UseRequestLocalization(localizationOptions);
            }
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
            }); 
            app.UseHangfireServer();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/StatusCode");
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (contextFeature != null)
                        {
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(contextFeature.Error);
                            await context.Response.WriteAsync(json);
                        }
                    });
                });
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