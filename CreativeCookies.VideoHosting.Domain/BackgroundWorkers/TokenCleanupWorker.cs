using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        var authCodeRepository = scope.ServiceProvider.GetRequiredService<IAuthorizationCodeRepository>();
                        // HACK: TODO Add expired tokens cleanup!
                        await authCodeRepository.ClearExpiredAuthorizationCodes();
                    }
                    await Task.Delay(TimeSpan.FromMinutes(0.5), stoppingToken);
                }
            }
        }
    }

}
