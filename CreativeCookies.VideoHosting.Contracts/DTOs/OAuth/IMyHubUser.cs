using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IMyHubUser
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
