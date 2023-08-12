using CreativeCookies.VideoHosting.Contracts.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Wrappers
{
    public class AccountCreationResult : IAccountCreationResult
    {
        public string AccountOnboardingUrl { get; set; }
        public string AccountId { get; set; }
    }
}
