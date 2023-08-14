using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class UsersPaginatedResultDto
    {
        public IEnumerable<MyHubUserDto> Users { get; set; }
        public bool HasMore { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public UsersPaginatedResultDto(IEnumerable<MyHubUserDto> users, bool hasMore, int currentPage, int totalPages)
        {
            Users = users;
            HasMore = hasMore;
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }
    }
}
