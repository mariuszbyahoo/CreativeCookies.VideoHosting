namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IAuthorizationCodeRepository
    {
        Task<string> GetAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method);
        Task ClearExpiredAuthorizationCodes();
    }
}
