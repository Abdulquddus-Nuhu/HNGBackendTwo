using HNGBackendTwo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HNGBackendTwo
{
    public class MigrateDb : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrateDb(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrateDb>>();
            try
            {
                logger.LogInformation("Applying HNGBackendTwo_Db Migration!");
                //await context.Database.EnsureCreatedAsync();
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
                logger.LogInformation("HNGBackendTwo_Db Migration Successful!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to apply HNGBackendTwo_Db Migration!");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
