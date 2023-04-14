namespace CreativeCookies.VideoHosting.DAL.DTOs
{
    public class ClientException
    {
        public Guid Id { get; set; }
        public string ErrorLog { get; set; }
        public ClientException() { }
    }
}