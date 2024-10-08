﻿namespace CreativeCookies.VideoHosting.API.Templates
{
    public class EmailTemplateViewModel
    {
        public string RecipientName { get; set; }

        public string Message { get; set; }

        public string Introduction { get; set; }

        public string WebsiteName { get; set; }

        public EmailTemplateViewModel(string recipientName, string message, string introduction, string websiteName)
        {
            RecipientName = recipientName;
            Message = message;
            Introduction = introduction;
            WebsiteName = websiteName;
        }
    }
}
