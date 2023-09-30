using CreativeCookies.StripeEvents.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.Contracts
{
    public interface IStripeEventsDistributor
    {
        Task RedirectEvent(StripeEventDTO stripeEvent);
    }
}
