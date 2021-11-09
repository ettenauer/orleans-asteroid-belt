using Microsoft.AspNetCore.SignalR;
using Orleans.AsteriodBelt.Grains;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Hubs;

public class AsteroidHubAdapter : IAsyncObserver<AsteroidState>, IAsteriodHubAdapter
{
    private readonly IHubContext<AsteroidHub> hub;
    private readonly IClusterClient clusterClient;

    public AsteroidHubAdapter(IClusterClient clusterClient, IHubContext<AsteroidHub> hub)
    {
        this.clusterClient = clusterClient;
        this.hub = hub;
    }

    public async Task ConnectStreamAsync()
    {
        var streamProvider = clusterClient.GetStreamProvider(StreamConstants.StreamProvider);
        var stateStream = streamProvider.GetStream<AsteroidState>(StreamConstants.StateStreamId, StreamConstants.StateStreamNamespace);
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
        await hub.Clients.All.SendAsync("writeState", new HubMessageEnvelope
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