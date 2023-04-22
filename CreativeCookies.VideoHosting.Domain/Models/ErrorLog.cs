using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Models
{
    public class ErrorLog : IErrorLog
    {
        public string Log { get; set; }
    }
}
