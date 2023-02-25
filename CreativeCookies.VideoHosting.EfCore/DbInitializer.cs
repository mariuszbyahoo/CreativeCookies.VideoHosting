using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore
{
    public static class DbInitializer
    {
        public static void Initialize(VideoHostingDbContext context)
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }
}
