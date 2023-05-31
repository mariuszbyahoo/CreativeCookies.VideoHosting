using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IRefreshToken
    {
        Guid Id { get; set; }
        Guid UserId { get; set; }
        string Token { get; set; }
        DateTime CreationDate { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}
