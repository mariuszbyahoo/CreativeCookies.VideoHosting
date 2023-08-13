using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class AccountCreationResult : IAccountCreationResult
    {
        public string AccountOnboardingUrl { get; set; }
        public string AccountId { get; set; }
    }
}
