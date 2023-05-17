using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.API.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);

        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
