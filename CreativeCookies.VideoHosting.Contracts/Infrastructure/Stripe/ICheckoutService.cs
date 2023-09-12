using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public interface ICheckoutService
    {
        public Task<string> CreateNewSession(string priceId);
    }
}
