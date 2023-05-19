namespace CreativeCookies.VideoHosting.API.Templates
{
    public class AccountActivationEmailTemplateViewModel : EmailTemplateViewModel
    {
        public string WebsiteUrl { get; set; }
        public string AccountActivationLink { get; set; }

        public AccountActivationEmailTemplateViewModel(string recipientName, string introduction, string websiteUrl, string websiteName, string accountActivationLink)
            : base(recipientName, string.Empty, introduction, websiteName)
        {
            WebsiteUrl = websiteUrl;
            AccountActivationLink = accountActivationLink;
        }
    }
}
