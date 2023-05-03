using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs.OAuth
{
    public class MyHubUser : IMyHubUser
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }

        public MyHubUser(Guid id, string userEmail)
        {
            Id = id;
            UserEmail = userEmail;
        }
    }
}
