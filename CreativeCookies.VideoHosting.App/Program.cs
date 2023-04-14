using Azure.Storage.Blobs;
using Azure.Storage;
using CreativeCookies.VideoHosting.App.Data;
using CreativeCookies.VideoHosting.App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.FileProviders;

namespace CreativeCookies.VideoHosting.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var accountName = builder.Configuration.GetValue<string>("Storage:AccountName");
            var accountKey = builder.Configuration.GetValue<string>("Storage:AccountKey");
            var blobServiceUrl = builder.Configuration.GetValue<string>("Storage:BlobServiceUrl");


            builder.Services.AddSingleton(x => new StorageSharedKeyCredential(accountName, accountKey));
            builder.Services.AddSingleton(x => new BlobServiceClient(new Uri(blobServiceUrl), x.GetRequiredService<StorageSharedKeyCredential>()));

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseSpa(spa =>
                {
                    spa.UseProxyToSpaDevelopmentServer("https://localhost:7237");
                });
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                // Configure the app to use the production build of your React app
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build")),
                    RequestPath = new PathString("")
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}