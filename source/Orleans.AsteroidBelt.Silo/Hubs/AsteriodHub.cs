using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orleans.AsteriodBelt.Grains;
using System;
using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Hubs;

public class AsteroidHub : Hub
{
    private readonly ILogger<AsteroidHub> _logger;

    public AsteroidHub(ILogger<AsteroidHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"{nameof(OnConnectedAsync)} called.");

        await base.OnConnectedAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, Constants.HubGroupId);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation(exception, $"{nameof(OnDisconnectedAsync)} called.");

        await base.OnDisconnectedAsync(exception);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants.HubGroupId);
    }
}

