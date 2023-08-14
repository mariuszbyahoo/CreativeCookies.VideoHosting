using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Azure
{
    public class SasTokenResultDto
    {
        public string SasToken { get; }

        public SasTokenResultDto(string sasToken)
        {
            SasToken = sasToken;
        }
    }
}
