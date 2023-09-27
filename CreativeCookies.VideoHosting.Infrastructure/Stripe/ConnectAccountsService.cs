using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class ConnectAccountsService : IConnectAccountsService
    {
        private readonly IConnectAccountsRepository _repo;

        public ConnectAccountsService(IConnectAccountsRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> CanBeQueriedOnStripe(string accountId)
        {
            var result = await _repo.CanBeQueriedOnStripe(accountId);
            return result;
        }

        public async Task EnsureSaved(string accountId)
        {
            await _repo.EnsureSaved(accountId);
        }

        public string GetConnectedAccountId()
        {
            var result = _repo.GetConnectedAccountId();
            return result;
        }
    }
}
