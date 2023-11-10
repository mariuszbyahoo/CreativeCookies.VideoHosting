using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Abstractions;
using CreativeCookies.VideoHosting.API.Templates;
using CreativeCookies.VideoHosting.Contracts.Email;
using CreativeCookies.VideoHosting.DTOs.Email;
using Microsoft.Extensions.Localization;

namespace CreativeCookies.VideoHosting.API.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _smtpPass;
        private readonly ILogger<EmailService> _logger;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public EmailService(
            ILogger<EmailService> logger,
            string smtpHost,
            int smtpPort,
            string senderEmail,
            string smtpPass,
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _senderEmail = senderEmail;
            _smtpPass = smtpPass;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Method utilizating EmailTemplateViewModel in order to send a custom email message to desired recipient.
        /// </summary>
        /// <param name="recipientEmail">Email message reviever</param>
        /// <param name="subject">Email message subject</param>
        /// <param name="introduction">Short polite introduciton paragraph</param>
        /// <param name="websiteUrl">Url which will be pointed as an origin of the email</param>
        /// <param name="websiteName">Website's name which will be shown to the end user to identify the email sender</param>
        /// <returns>boolean value indicating was email sending successful at the end or not.</returns>
        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string introduction, string message, string websiteName)
        {
            var model = new EmailTemplateViewModel(recipientEmail, message, introduction, websiteName);
            var htmlMessage = await RenderViewToStringAsync("EmailTemplate", model);
            return await SendMessageAsync(recipientEmail, subject, htmlMessage);
        }

        public async Task<bool> SendAccountActivationEmailAsync(string recipientEmail, string subject, string introduction, string websiteUrl, string websiteName, string accountActivationLink)
        {
            var model = new AccountActivationEmailTemplateViewModel(recipientEmail, introduction, websiteUrl, websiteName, accountActivationLink);
            var htmlMessage = await RenderViewToStringAsync("AccountActivationEmailTemplate", model);
            return await SendMessageAsync(recipientEmail, subject, htmlMessage);
        }

        public async Task<bool> SendEmailChangeLinkAsync(string recipientEmail, string subject, string introduction, string websiteName, string emailChangeLink)
        {
            var model = new AccountConfirmEmailChangeTemplateViewModel(recipientEmail, "", introduction, websiteName, emailChangeLink);
            var htmlMessage = await RenderViewToStringAsync("AccountConfirmEmailChangeTemplate", model);
            return await SendMessageAsync(recipientEmail, subject, htmlMessage);
        }


        public async Task<bool> SendResetPasswordLinkAsync(string recipientEmail, string subject, string introduction, string websiteName, string resetPasswordLink)
        {
            var model = new AccountResetPasswordEmailTemplateViewModel(recipientEmail, introduction, websiteName, resetPasswordLink);
            var htmlMessage = await RenderViewToStringAsync("AccountResetPasswordEmailTemplate", model);
            return await SendMessageAsync(recipientEmail, subject, htmlMessage);
        }

        public async Task<bool> SendInvoiceAsync(string recipientEmail, string invoiceNo, string websiteName, Attachement attachment)
        {
            var stringLocalizer = _serviceProvider.GetRequiredService<IStringLocalizer<EmailService>>();
            var model = new EmailTemplateViewModel(recipientEmail, $"{stringLocalizer["InvoiceMailTxt1"]} {invoiceNo} {stringLocalizer["InvoiceMailTxt2"]}", string.Empty, websiteName);
            var htmlMessage = await RenderViewToStringAsync("EmailTemplate", model);
            return await SendMessageAsync(recipientEmail, $"{stringLocalizer["Invoice"]} {invoiceNo}", htmlMessage, attachment);
        }

        /// <summary>
        /// Sends an email using secured TLS encryption, if an email server does not supporting TLS encrpyption, it will throw an NotSupportedException
        /// </summary>
        /// <param name="recipientEmail"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns>Boolean value indicating the result of an operation: true (success) or false (something went wrong, most proppably mail wasn't sent)</returns>
        private async Task<bool> SendMessageAsync(string recipientEmail, string subject, string htmlMessage, Attachement? attachment = null)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(MailboxAddress.Parse(_senderEmail));
                emailMessage.To.Add(MailboxAddress.Parse(recipientEmail));
                emailMessage.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlMessage };

                // Check if there's an attachment
                if (attachment != null && attachment.FileData.Length > 0)
                {
                    // Add the attachment to the email
                    builder.Attachments.Add(attachment.FileNameWithExtension, attachment.FileData);
                }

                // Set the email body
                emailMessage.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_senderEmail, _smtpPass);

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
                return true;
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError($"NotSupportedException occurred: {ex.Message}", ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occurred: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Renders Razor View (for example an email template) to HTML string
        /// </summary>
        /// <typeparam name="TModel">View model class which will be used by a Razor View</typeparam>
        /// <param name="viewName">Razor view's name - without .cshtml at the end!</param>
        /// <param name="model">Razor view's model instance</param>
        /// <returns>String containing HTML code for Email message</returns>
        /// <exception cref="ArgumentNullException">Thrown if particular view has not been found.</exception>
        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.GetView("~/", $"/Templates/{viewName}.cshtml", false);

                if (viewResult.View == null)
                {
                    var ex = new ArgumentNullException($"{viewName} does not match any available view");
                    _logger.LogError($"Email service has received wrong {nameof(viewName)}'s value, read exception message to view more", ex.ParamName);
                    throw ex;
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
