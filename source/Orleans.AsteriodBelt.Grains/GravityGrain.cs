using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public class GravityGrain : Grain, IGravityGrain, IRemindable
{
    private readonly HashSet<int> asteriods = new();
    private readonly ILogger<GravityGrain> logger;

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
            TimeSpan.FromSeconds(5));

        return base.OnActivateAsync(); 
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        logger.LogInformation($"GravityGrain {IdentityString}  is alive");

        return Task.CompletedTask;
    }

    public Task RegistryAsteriodAsync(int asteriodId)
    {
        asteriods.Add(asteriodId);

        return Task.CompletedTask;
    }

    private async Task MoveAsync(object state)
    {
        logger.LogInformation("Gravity Pulse");

        foreach (var asteriodId in asteriods)
        {
            var asteriodGrain = GrainFactory.GetGrain<IAsteriodGrain>(asteriodId);
            await asteriodGrain.MoveAsync();
        }
    }

    private class Pulse
    {
        public static readonly Pulse Instance = new Pulse();
        private Pulse() { }
    }
}

