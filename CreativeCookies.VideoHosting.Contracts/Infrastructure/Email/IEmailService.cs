using CreativeCookies.VideoHosting.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string introduction, string message, string websiteName);
        Task<bool> SendAccountActivationEmailAsync(string recipientEmail, string subject, string introduction, string websiteUrl, string websiteName, string accountActivationLink);
        Task<bool> SendEmailChangeLinkAsync(string recipientEmail, string subject, string introduction, string websiteName, string emailChangeLink);
        Task<bool> SendResetPasswordLinkAsync(string recipientEmail, string subject, string introduction, string websiteName, string resetPasswordLink);
        Task<bool> SendInvoiceAsync(string recipientEmail, string invoiceNo, string websiteName, Attachement attachment);
    }
}
