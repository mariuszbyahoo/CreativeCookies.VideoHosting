using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services.IdP
{
    public class MyHubUserStore : IMyHubUserStore
    {
        private readonly IUserStore<MyHubUser> _userStore;
        private readonly UserManager<MyHubUser> _userManager;

        public MyHubUserStore(IUserStore<MyHubUser> userStore, UserManager<MyHubUser> userManager)
        {
            _userStore = userStore;
            _userManager = userManager;
        }

        public async Task SetUserNameAsync(MyHubUserDto user, string? userName, CancellationToken cancellationToken)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            await _userStore.SetUserNameAsync(dao, userName, cancellationToken);
        }
    }
}
