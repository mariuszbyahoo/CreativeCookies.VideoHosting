namespace CreativeCookies.VideoHosting.API.Templates
{
    public class AccountConfirmEmailChangeTemplateViewModel: EmailTemplateViewModel
    {
        public string EmailChangeLink { get; set; }

        public AccountConfirmEmailChangeTemplateViewModel(string recipientName, string message, string introduction, string websiteName, string emailChangeLink) : 
            base(recipientName, message, introduction, websiteName)
        {
            EmailChangeLink = emailChangeLink;
        }
    }
}
