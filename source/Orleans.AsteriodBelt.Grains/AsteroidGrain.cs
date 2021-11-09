using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class AsteroidGrain : Grain, IAsyncObserver<Move>, IAsyncObserver<AsteroidState>, IAsteriodGrain, IRemindable
{
    private const int DestructionRadius = 5;

    private readonly ILogger<AsteroidGrain> logger;
    private IAsyncStream<Move> moveStream;
    private IAsyncStream<AsteroidState> stateStream;
    private readonly AsteroidMotion motion;
    private readonly int weight;

    private bool destroyed = false;

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
        await stateStream.SubscribeAsync(this);

        await base.OnActivateAsync();
    }

    public Task OnCompletedAsync() =>  Task.CompletedTask;
    
    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;
    
    public Task OnNextAsync(Move item, StreamSequenceToken token = null)
    {
        var (x,y) = motion.Move();

        return stateStream.OnNextAsync(new AsteroidState
        {
            AsteroidId = this.GetPrimaryKeyLong(),
            X = x,
            Y = y,
            Weight = weight,
            Destroyed = destroyed
        });
    }

    public Task OnNextAsync(AsteroidState item, StreamSequenceToken token = null)
    {
        if (item.AsteroidId != this.GetPrimaryKeyLong() && !destroyed)
        {
            var (x, y) = motion.CurrentPosition;

            if (item.X <= x + DestructionRadius && item.X >= x - DestructionRadius &&
                item.Y <= y + DestructionRadius && item.Y >= y - DestructionRadius)
            {
                destroyed = true;
                logger.LogInformation($"Asteroid {this.GetPrimaryKeyLong()} is destroyed by {item.AsteroidId}");
            }
        }

        return Task.CompletedTask;
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
