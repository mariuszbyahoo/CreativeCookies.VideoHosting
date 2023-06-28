using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs
{
    public interface IUsersPaginatedResult
    {
        public IEnumerable<IMyHubUser> Users{ get; set; }
        public bool HasMore { get; set; }
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
    }
}
