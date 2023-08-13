using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs
{
    public interface IAccountCreationResult
    {
        public string AccountOnboardingUrl { get; set; }
        public string AccountId { get; set; }
    }
}
