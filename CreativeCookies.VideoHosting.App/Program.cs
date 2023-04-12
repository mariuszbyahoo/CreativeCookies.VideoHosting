using Azure.Storage.Blobs;
using Azure.Storage;
using CreativeCookies.VideoHosting.App.Data;
using CreativeCookies.VideoHosting.App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;


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
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
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