using CreativeCookies.VideoHosting.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.App.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ClientError> ClientErrors { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
    }
}
