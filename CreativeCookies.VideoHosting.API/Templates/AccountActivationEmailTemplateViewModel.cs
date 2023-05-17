namespace CreativeCookies.VideoHosting.API.Templates
{
    public class AccountActivationEmailTemplateViewModel
    {
        public string RecipientName { get; set; }
        public string Introduction { get; set; }
        public string WebsiteUrl { get; set; }
        public string WebsiteName { get; set; }
        public string AccountActivationLink { get; set; }

        public AccountActivationEmailTemplateViewModel(string recipientName, string introduction, string websiteUrl, string websiteName, string accountActivationLink)
        {
            RecipientName = recipientName;
            Introduction = introduction;
            WebsiteUrl = websiteUrl;
            WebsiteName = websiteName;
            AccountActivationLink = accountActivationLink;
        }
    }
}
