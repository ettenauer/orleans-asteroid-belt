using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class GravityGrain : Grain, IGravityGrain, IRemindable
{
    private readonly ILogger<GravityGrain> logger;
    private IAsyncStream<Move> stream;

    public GravityGrain(ILogger<GravityGrain> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task OnActivateAsync()
    {
        var timer = RegisterTimer(
            MoveAsync,
            Pulse.Instance,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15));

        var streamProvider = GetStreamProvider(StreamConstants.StreamProvider);

        stream = streamProvider.GetStream<Move>(StreamConstants.MoveStreamId, StreamConstants.MoveStreamNamespace);

        return base.OnActivateAsync(); 
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        logger.LogInformation($"GravityGrain {IdentityString}  is alive");

        return Task.CompletedTask;
    }

    public async Task StartKeepAliveAsync()
    {
        await RegisterOrUpdateReminder("GravityKeepAlive", TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
    }

    private Task MoveAsync(object state)
    {
        logger.LogInformation("Gravity Pulse send");

        return stream.OnNextAsync(new Move());
    }

    private class Pulse
    {
        public static readonly Pulse Instance = new Pulse();
        private Pulse() { }
    }
}

