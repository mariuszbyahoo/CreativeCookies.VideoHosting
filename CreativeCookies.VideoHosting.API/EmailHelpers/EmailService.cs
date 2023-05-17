using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace CreativeCookies.VideoHosting.API.EmailHelpers
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
        /// Sends an email using secured TLS encryption, if an email server does not supporting TLS encrpyption, it will throw an NotSupportedException
        /// </summary>
        /// <param name="recipientEmail"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <returns>Boolean value indicating the result of an operation: true (success) or false (something went wrong, most proppably mail wasn't sent)</returns>
        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlMessage)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_senderEmail, _senderEmail));
                emailMessage.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = htmlMessage
                };

                using (var client = new SmtpClient())
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
                _logger.LogError($"NotSupportedException occured, inspect is an email server : {_smtpHost} supporting TLS encryption at all as this is the most proppable cause of an exception, msg: {ex.Message}, innerException: {ex.InnerException}, source: {ex.Source}", ex);
                return false;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Unexpected exception occured when trying to send an email, msg: {ex.Message}, innerException: {ex.InnerException}, source: {ex.Source}", ex);
                return false;
            }
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
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
