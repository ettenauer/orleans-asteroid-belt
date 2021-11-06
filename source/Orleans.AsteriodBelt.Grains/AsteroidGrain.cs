using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class AsteroidGrain : Grain, IAsyncObserver<Move>, IAsteriodGrain, IRemindable
{
    private readonly ILogger<AsteroidGrain> logger;
    private IAsyncStream<Move> stream;
    private AsteroidMotion motion;

    public AsteroidGrain(ILogger<AsteroidGrain> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async override Task OnActivateAsync()
    {
        motion = new AsteroidMotion();

        var streamProvider = GetStreamProvider("gravityField");

        stream = streamProvider.GetStream<Move>(GravityGrain.StreamId, "default");

        await stream.SubscribeAsync(this).ConfigureAwait(false);

        await base.OnActivateAsync().ConfigureAwait(false);
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

        logger.LogInformation($"[{x},{y}]");

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
