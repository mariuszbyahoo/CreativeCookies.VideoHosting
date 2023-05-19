namespace CreativeCookies.VideoHosting.API.Templates
{
    public class AccountResetPasswordEmailTemplateViewModel : EmailTemplateViewModel
    {
        public string ResetPasswordLink { get; set; }
        public AccountResetPasswordEmailTemplateViewModel(string recipientName, string introduction, string websiteName, string resetPasswordLink) 
            : base(recipientName, string.Empty, introduction, websiteName)
        {
            ResetPasswordLink = resetPasswordLink;
        }
    }
}
