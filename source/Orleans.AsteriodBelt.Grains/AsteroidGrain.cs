using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains.DomainObjects;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class AsteroidGrain : Grain, IAsyncObserver<AsteroidState>, IAsteriodGrain
{
    private const int DestructionRadius = 5;

    private readonly ILogger<AsteroidGrain> logger;
    private IAsyncStream<AsteroidState> stateStream;
    private readonly AsteroidMotion motion;
    private readonly int weight;

    private bool destroyed = false;

    public AsteroidGrain(ILogger<AsteroidGrain> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        motion = new AsteroidMotion();
        weight = new Random(Guid.NewGuid().GetHashCode()).Next(20, 50);
    }

    public async override Task OnActivateAsync()
    {
        var streamProvider = GetStreamProvider(Constants.StreamProvider);

        stateStream = streamProvider.GetStream<AsteroidState>(Constants.StateStreamId, Constants.StateStreamNamespace);
        await stateStream.SubscribeAsync(this);

        await base.OnActivateAsync();
    }

    public Task OnCompletedAsync() =>  Task.CompletedTask;
    
    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;
    
    public Task MoveAsync()
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
}
