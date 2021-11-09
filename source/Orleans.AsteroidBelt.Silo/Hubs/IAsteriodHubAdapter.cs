using System.Threading.Tasks;

namespace Orleans.AsteroidBelt.Silo.Hubs;

public interface IAsteriodHubAdapter
{
    Task ConnectStreamAsync();
}

