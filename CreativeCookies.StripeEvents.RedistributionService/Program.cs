

using CreativeCookies.StripeEvents.Contracts;
using CreativeCookies.StripeEvents.DTOs;
using CreativeCookies.StripeEvents.Services;
using CreativeCookies.StripeEvents.Services.HostedServices;

namespace CreativeCookies.StripeEvents.RedistributionService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IDeployedInstancesService, DeployedInstancesService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IStripeEventsDistributor, StripeEventsDistributor>();
            builder.Services.AddHostedService<StripeMessageReceiver>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}