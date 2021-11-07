using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.AsteroidBelt.Silo.Hubs;
using Orleans.Runtime;
using Orleans.Streams;
using SignalR.Orleans.Core;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Grains;

public class AsteriodHubPublisherGrain : Grain, IAsteriodHubPublisherGrain, IAsyncObserver<AsteroidState>, IRemindable
{
    private HubContext<IAsteriodHub> hubContext;
    private IAsyncStream<AsteroidState> stateStream;
    private ILogger<IAsteriodHubPublisherGrain> logger;
    
    public AsteriodHubPublisherGrain(ILogger<IAsteriodHubPublisherGrain> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async override Task OnActivateAsync()
    {
        hubContext = GrainFactory.GetHub<IAsteriodHub>();

        var streamProvider = GetStreamProvider(StreamConstants.StreamProvider);
        stateStream = streamProvider.GetStream<AsteroidState>(StreamConstants.StateStreamId, StreamConstants.StateStreamNamespace);
        await stateStream.SubscribeAsync(this);

        await RegisterOrUpdateReminder("GravityKeepAlive", TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public Task OnNextAsync(AsteroidState item, StreamSequenceToken token = null)
    {
        logger.LogInformation($"Received state message from {item.AsteroidId}");

        return hubContext.Group(Guid.Empty.ToString()).Send("writeState", new HubMessageEnvelope
        {
            Id = item.AsteroidId,
            Message = $"|ID: {item.AsteroidId} | X: {item.X} | Y: {item.Y} | Weight: {item.Weight} | Destroyed: {item.Destroyed}|"
        });
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        logger.LogInformation($"{nameof(AsteriodHubPublisherGrain)} {IdentityString} is alive");

        return Task.CompletedTask;
    }

    public Task RegisterAsync(string connectionId)
    {
        hubContext.Group(Guid.Empty.ToString()).Add(connectionId);

        return Task.CompletedTask;
    }

    public Task UnregisterAsync(string connectionId)
    {
        hubContext.Group(Guid.Empty.ToString()).Remove(connectionId);

        return Task.CompletedTask;
    }

    private sealed class HubMessageEnvelope
    {
        public string Id { get; init; }

        public string Message { get; init; }
    }
}

