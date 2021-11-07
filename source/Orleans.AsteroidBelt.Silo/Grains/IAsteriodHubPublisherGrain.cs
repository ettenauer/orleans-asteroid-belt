using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Grains;

public interface IAsteriodHubPublisherGrain : IGrainWithIntegerKey
{
    Task RegisterAsync(string connectionId);

    Task UnregisterAsync(string connectionId);
}

