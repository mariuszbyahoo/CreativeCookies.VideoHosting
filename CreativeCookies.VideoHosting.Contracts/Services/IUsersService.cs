using CreativeCookies.VideoHosting.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IUsersService
    {
        Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role);

        Task<bool> AssignStripeCustomerId(string userId, string stripeCustomerId);

        Task<bool> IsUserSubscriber(string userId);
    }
}
