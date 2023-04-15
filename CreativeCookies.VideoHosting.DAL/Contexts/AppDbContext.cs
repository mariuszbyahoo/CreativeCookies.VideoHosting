using CreativeCookies.VideoHosting.DAL.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<ClientException> ClientErrors { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
