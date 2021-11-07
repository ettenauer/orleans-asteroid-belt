using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class AsteroidGrain : Grain, IAsyncObserver<Move>, IAsteriodGrain, IRemindable
{
    private readonly ILogger<AsteroidGrain> logger;
    private IAsyncStream<Move> moveStream;
    private IAsyncStream<AsteroidState> stateStream;
    private readonly AsteroidMotion motion;
    private readonly int weight;

    public AsteroidGrain(ILogger<AsteroidGrain> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.motion = new AsteroidMotion();
        this.weight = new Random(Guid.NewGuid().GetHashCode()).Next(20, 50);
    }

    public async override Task OnActivateAsync()
    {
        var streamProvider = GetStreamProvider(StreamConstants.StreamProvider);

        moveStream = streamProvider.GetStream<Move>(StreamConstants.MoveStreamId, StreamConstants.MoveStreamNamespace);
        await moveStream.SubscribeAsync(this);

        stateStream = streamProvider.GetStream<AsteroidState>(StreamConstants.StateStreamId, StreamConstants.MoveStreamNamespace);

        await base.OnActivateAsync();
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        logger.LogError(ex, ex.Message);

        return Task.CompletedTask;
    }

    public Task OnNextAsync(Move item, StreamSequenceToken token = null)
    {
        var (x,y) = motion.Move();

        return stateStream.OnNextAsync(new AsteroidState
        {
            AsteroidId = IdentityString,
            X = x,
            Y = y,
            Weight = weight,
            Destroyed = false
        });
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        logger.LogInformation($"AsteriodGrain {IdentityString} is alive");

        return Task.CompletedTask;
    }

    public async Task StartKeepAliveAsync()
    {
        await RegisterOrUpdateReminder($"AsteriodKeepAlive-{IdentityString}", TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));
    }
}
