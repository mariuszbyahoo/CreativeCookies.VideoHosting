using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs.OAuth
{
    public class MyHubUserDto : IMyHubUser
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public MyHubUserDto(Guid id, string userEmail, string role, bool isActive)
        {
            Id = id;
            UserEmail = userEmail;
            Role = role;
            IsActive = isActive;
        }
    }
}
