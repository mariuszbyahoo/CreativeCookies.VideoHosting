using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class UsersPaginatedResult : IUsersPaginatedResult
    {
        public IEnumerable<IMyHubUser> Users { get; set; }
        public bool HasMore { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public UsersPaginatedResult(IEnumerable<IMyHubUser> users, bool hasMore, int currentPage, int totalPages)
        {
            Users = users;
            HasMore = hasMore;
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }
    }
}
