using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains;
using Orleans.AsteroidBelt.Silo.Hubs;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo;

public class AsteriodBeltService : IHostedService
{
    //Note: these asteriods should be distributed within the akka cluster
    private static int[] AsteriodIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private const int GravityId = 1;

    private readonly IAsteriodHubAdapter hubAdapter;
    private readonly ILogger<AsteriodBeltService> logger;
    private readonly IClusterClient client;

    public AsteriodBeltService(IClusterClient clusterClient, IAsteriodHubAdapter hubAdapter, ILogger<AsteriodBeltService> logger)
    {
        this.hubAdapter = hubAdapter ?? throw new ArgumentNullException(nameof(hubAdapter));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.client = clusterClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.Connect();

        var gravity = client.GetGrain<IGravityGrain>(GravityId);

        foreach (var id in AsteriodIds)
            await gravity.RegistryAsteriodAsync(id);

        await hubAdapter.ConnectStreamAsync(client);

        logger.LogInformation($"{nameof(AsteriodBeltService)} started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AsteriodBeltService)} stopped");

        await client.Close();
    }
}

