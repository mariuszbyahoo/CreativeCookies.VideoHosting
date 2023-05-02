
using Azure.Storage;
using Azure.Storage.Blobs;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.Azure;
using CreativeCookies.VideoHosting.Domain.BackgroundWorkers.CreativeCookies.VideoHosting.Domain.Services;
using CreativeCookies.VideoHosting.Domain.Repositories;
using CreativeCookies.VideoHosting.Domain.Repositories.OAuth;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

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
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information);

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
                    var instrumentationKey = hostingContext.Configuration["ApplicationInsights:InstrumentationKey"];

                    loggerConfiguration
                                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Warning)
                                .WriteTo.ApplicationInsights(new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(instrumentationKey)), TelemetryConverter.Traces);
                }
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
            // Add services to the container.
            var connectionString = "";

            if (builder.Environment.IsDevelopment())
            {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            else if (builder.Environment.IsProduction()) 
            {
                connectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            }


            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, // Number of times to retry before giving up
                        maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
                        errorNumbersToAdd: null); // You can add custom error numbers to be considered transient errors
                });
            });

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddRazorPages(); // Add Razor Pages support

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IClientStore, ClientStore>();
            builder.Services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();
            builder.Services.AddScoped<IJWTRepository, JWTRepository>();

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
            builder.Services.AddSingleton<IFilmsRepository, FilmsRepository>();
            builder.Services.AddSingleton<ISasTokenRepository, SasTokenRepository>();
            builder.Services.AddScoped<IErrorLogsRepository, ErrorLogsRepository>();
            builder.Services.AddScoped<IAuthorizationCodeRepository, AuthorizationCodeRepository>();

            builder.Services.AddHostedService<TokenCleanupWorker>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var context = builder.Services.BuildServiceProvider().GetService<AppDbContext>();
            context.Database.Migrate();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); 
            app.UseCors("AllowAllOriginsPolicy");

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages(); 

            app.Run();
        }
    }
}