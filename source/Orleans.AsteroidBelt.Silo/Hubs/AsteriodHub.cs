using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orleans.AsteroidBelt.Silo.Grains;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Hubs;

public class AsteroidHub : Hub, IAsteriodHub
{
    private const int HubPublisherId = 1;

    private readonly ILogger<AsteroidHub> logger;
    private readonly IClusterClient clusterClient;

    public AsteroidHub(IClusterClient clusterClient, ILogger<AsteroidHub> logger)
    {
        this.clusterClient = clusterClient ?? throw new ArgumentNullException(nameof(clusterClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation($"{nameof(OnConnectedAsync)} called.");

        var publisherGrain = clusterClient.GetGrain<IAsteriodHubPublisherGrain>(HubPublisherId);
        await publisherGrain.RegisterAsync(Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        logger.LogInformation(exception, $"{nameof(OnDisconnectedAsync)} called.");

        var publisherGrain = clusterClient.GetGrain<IAsteriodHubPublisherGrain>(HubPublisherId);
        await publisherGrain.UnregisterAsync(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}

