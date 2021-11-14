using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Streams;
using SignalR.Orleans.Core;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Hubs;

public class AsteroidHubAdapter : IAsyncObserver<AsteroidState>, IAsteriodHubAdapter
{
    private HubContext<AsteroidHub> hub;
    private readonly ILogger<AsteroidHubAdapter> logger;

    public AsteroidHubAdapter(ILogger<AsteroidHubAdapter> logger)
    {
        this.logger = logger;
    }

    public async Task ConnectStreamAsync(IClusterClient clusterClient)
    {
        var streamProvider = clusterClient.GetStreamProvider(Constants.StreamProvider);
        hub = clusterClient.GetHub<AsteroidHub>();
        var stateStream = streamProvider.GetStream<AsteroidState>(Constants.StateStreamId, Constants.StateStreamNamespace);
        await stateStream.SubscribeAsync(this);
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public async Task OnNextAsync(AsteroidState item, StreamSequenceToken token = null)
    {
        logger.LogInformation($"{nameof(AsteroidHubAdapter)} received stream item -> {item.AsteroidId} for publishing");

        await hub.Group(Constants.HubGroupId).Send("writeState", new HubMessageEnvelope
        {
            Id = item.AsteroidId.ToString(),
            Message = $"|ID: {item.AsteroidId} | X: {item.X} | Y: {item.Y} | Weight: {item.Weight} | Destroyed: {item.Destroyed}|"
        });
    }

    private sealed class HubMessageEnvelope
    {
        public string Id { get; init; }

        public string Message { get; init; }
    }
}