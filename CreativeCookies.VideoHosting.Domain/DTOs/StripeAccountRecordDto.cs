using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class StripeAccountRecordDto
    {
        public Guid Id { get; set; }
        public string StripeConnectedAccountId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
