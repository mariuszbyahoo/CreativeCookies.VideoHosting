using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.ModelContracts
{
    public interface ISasTokenResult
    {
        string SasToken { get; }
    }
}
