using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IUsersRepository
    {
        Task<IUsersPaginatedResult> GetUsersList(string search, int pageNumber, int pageSize);
    }
}
