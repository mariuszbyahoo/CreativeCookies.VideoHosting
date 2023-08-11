using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Wrappers
{
    public interface IStripeResult<T>
    {
        bool Success { get; set; }
        T Data { get; set; }
        string ErrorMessage { get; set; }
    }

}
