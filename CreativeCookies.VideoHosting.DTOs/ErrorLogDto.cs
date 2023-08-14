using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs
{
    public class ErrorLogDto
    {
        public string Log { get; set; }

        public ErrorLogDto(string log)
        {
            Log = log;
        }
    }
}
