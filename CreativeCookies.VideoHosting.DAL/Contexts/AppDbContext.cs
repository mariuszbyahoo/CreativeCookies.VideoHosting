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
    public class AppDbContext : IdentityDbContext<MyHubUser>
    {
        public DbSet<ClientException> ClientErrors { get; set; }
        public DbSet<OAuthClient> OAuthClients { get; set; }
        public DbSet<AllowedScope> AllowedScopes { get; set; }
        public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }
        public DbSet<RefreshTokenDAO> RefreshTokens { get; set; }
        public DbSet<VideoMetadata> VideosMetadata { get; set; }
        public DbSet<StripeConfig> StripeConfig { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Price> StripePrices { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MyHubUser>().Property(s => s.StripeCustomerId).IsRequired(false);
            builder.Entity<VideoMetadata>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(e => e.Id).HasDefaultValueSql("newsequentialid()");
            });

            builder.Entity<OAuthClient>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(e => e.Id).HasDefaultValueSql("newsequentialid()");
            });

            builder.Entity<AuthorizationCode>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(c => c.Id).HasDefaultValueSql("newsequentialid()");
            });

            builder.Entity<RefreshTokenDAO>(o =>
            {
                o.HasKey(o => o.Id);
                o.Property(c => c.Id).HasDefaultValueSql("newsequentialid()");
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
