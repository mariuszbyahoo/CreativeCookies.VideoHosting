using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    public class StripeConfig
    {
        public Guid Id { get; set; }
        public string StripeConnectedAccountId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
