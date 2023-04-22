using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Models
{
    public class SasTokenResult : ISasTokenResult
    {
        public string SasToken { get; }

        public SasTokenResult(string sasToken)
        {
            SasToken = sasToken;
        }
    }
}
