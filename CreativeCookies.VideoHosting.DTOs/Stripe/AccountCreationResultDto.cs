using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Stripe
{
    public class AccountCreationResultDto
    {
        public string AccountOnboardingUrl { get; set; }
        public string AccountId { get; set; }

        public AccountCreationResultDto(string accountOnboardingUrl, string accountId)
        {
            AccountOnboardingUrl = accountOnboardingUrl;
            AccountId = accountId;
        }
    }
}
