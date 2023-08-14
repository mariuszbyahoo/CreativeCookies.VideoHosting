using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class RefreshTokenDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }

        public RefreshTokenDto(Guid id, Guid userId, string token, DateTime creationDate, DateTime expirationDate)
        {
            Id = id;
            UserId = userId;
            Token = token;
            CreationDate = creationDate;
            ExpirationDate = expirationDate;
        }
    }
}
