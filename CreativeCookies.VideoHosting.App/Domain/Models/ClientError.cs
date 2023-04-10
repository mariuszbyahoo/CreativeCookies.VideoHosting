using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Models
{
    public class ClientError
    {
        public Guid Id { get; set; }
        public string ErrorLog { get; set; }
        public ClientError() { }


    }
}
