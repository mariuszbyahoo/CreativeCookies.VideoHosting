using CreativeCookies.VideoHosting.Contracts.DTOs;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    public class ClientException : IErrorLog
    {
        public Guid Id { get; set; }
        public string Log { get; set; }
        public ClientException() { }
    }
}