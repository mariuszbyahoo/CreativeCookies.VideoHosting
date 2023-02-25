using CreativeCookies.VideoHosting.EfCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. ConfigureServices(IServiceCollection services) method's contents
builder.Services.AddDbContext<VideoHostingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevelopmentConnection"));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

CreateDbIfNotExists(app);
// Here type the Configure(IApplicationBuilder app, IWebHostEnvironment env) contents

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

static void CreateDbIfNotExists(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<VideoHostingDbContext>();
            DbInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occured while seeding the database");
        }
    }
}