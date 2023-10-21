using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.About;
using CreativeCookies.VideoHosting.DTOs.About;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services.About
{
    public class AboutPageService : IAboutPageService
    {
        private readonly IAboutPageRepository _repo;

        public AboutPageService(IAboutPageRepository repo)
        {
            _repo = repo;
        }

        public async Task<AboutPageDTO> GetAboutPageContents()
        {
            var res = await _repo.GetAboutPageInnerHTML();
            return res;
        }

        public async Task<bool> UpsertAboutPageContents(string innerHtml)
        {
            var res = await _repo.UpsertAboutPageInnerHTML(innerHtml);
            return res;
        }
    }
}
