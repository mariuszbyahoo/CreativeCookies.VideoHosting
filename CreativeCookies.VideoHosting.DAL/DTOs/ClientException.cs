using CreativeCookies.VideoHosting.Contracts.ModelContracts;

namespace CreativeCookies.VideoHosting.DAL.DTOs
{
    public class ClientException : IErrorLog
    {
        public Guid Id { get; set; }
        public string Log { get; set; }
        public ClientException() { }
    }
}