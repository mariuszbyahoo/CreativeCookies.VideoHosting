using CreativeCookies.VideoHosting.DTOs.Regulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IRegulationsRepository
    {
        Task<WebsiteRegulationsDTO?> GetRegulations();
        Task<WebsiteRegulationsDTO> UpdateRegulaions(WebsiteRegulationsDTO dto);
        Task<WebsitePrivacyPolicyDTO?> GetPrivacyPolicy();
        Task<WebsitePrivacyPolicyDTO> UpdatePrivacyPolicy(WebsitePrivacyPolicyDTO dto);
    }
}
