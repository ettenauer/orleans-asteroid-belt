using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo;

public class AsteriodBeltService : IHostedService
{
    //Note: these asteriods should be distributed within the akka cluster
    private static int[] AsteriodIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private static int GravityId = 0;

    private readonly IGrainFactory factory;
    private readonly ILogger<AsteriodBeltService> logger;

    public AsteriodBeltService(IGrainFactory factory, ILogger<AsteriodBeltService> logger)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await factory.GetGrain<IGravityGrain>(GravityId).StartKeepAliveAsync();

        foreach (var id in AsteriodIds)
            await factory.GetGrain<IAsteriodGrain>(id).StartKeepAliveAsync();        
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

