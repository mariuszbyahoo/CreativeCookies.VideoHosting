using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CreativeCookies.VideoHosting.Domain.BackgroundWorkers
{

    namespace CreativeCookies.VideoHosting.Domain.Services
    {
        public class TokenCleanupWorker : BackgroundService
        {
            private readonly IServiceScopeFactory _serviceScopeFactory;

            public TokenCleanupWorker(IServiceScopeFactory serviceScopeFactory)
            {
                _serviceScopeFactory = serviceScopeFactory;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var authCodeRepository = scope.ServiceProvider.GetRequiredService<IAuthorizationCodeService>();
                        // HACK: TODO Add expired tokens cleanup!
                        await authCodeRepository.ClearExpiredAuthorizationCodes();
                    }
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
    }

}
