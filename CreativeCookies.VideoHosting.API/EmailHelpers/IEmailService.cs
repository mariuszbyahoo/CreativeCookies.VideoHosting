using CreativeCookies.VideoHosting.API.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.API.EmailHelpers
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string introduction, string message, string websiteName);
        Task<bool> SendAccountActivationEmailAsync(string recipientEmail, string subject, string introduction, string websiteUrl, string websiteName, string accountActivationLink);
        Task<bool> SendEmailChangeLinkAsync(string recipientEmail, string subject, string introduction, string websiteName, string emailChangeLink);
    }
}
