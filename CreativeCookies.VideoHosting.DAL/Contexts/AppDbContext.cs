using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Contexts
{
    public class AppDbContext : IdentityDbContext
    {
        public DbSet<ClientException> ClientErrors { get; set; }
        public DbSet<OAuthClient> OAuthClients { get; set; }
        public DbSet<AllowedScope> AllowedScopes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OAuthClient>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(e => e.Id).HasDefaultValueSql("newsequentialid()");
            });

            builder.Entity<AllowedScope>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(e => e.Id).HasDefaultValueSql("newsequentialid()");

                o.HasOne(a => a.OAuthClient)
                    .WithMany(a => a.AllowedScopes)
                    .HasForeignKey(a => a.OAuthClientId);
            });
        }
    }
}
