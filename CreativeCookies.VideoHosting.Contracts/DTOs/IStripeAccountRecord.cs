using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs
{
    public interface IStripeAccountRecord
    {
        public Guid Id { get; set; }
        public string StripeConnectedAccountId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
