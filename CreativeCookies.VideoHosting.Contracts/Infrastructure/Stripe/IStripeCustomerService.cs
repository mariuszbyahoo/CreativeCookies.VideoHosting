using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public interface IStripeCustomerService
    {
        /// <summary>
        /// Creates a Stripe Customer entity assigned to an exact user's Id
        /// </summary>
        /// <param name="userId">Id of an user to create a  Stripe Customer object for.</param>
        /// <returns>True - no exceptions occured, result successfull, False - exception occured, Customer might not been created</returns>
        Task<bool> CreateStripeCustomer(string userId, string userEmail);
    }
}
