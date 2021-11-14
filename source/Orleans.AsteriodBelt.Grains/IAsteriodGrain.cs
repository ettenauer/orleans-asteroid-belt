using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public interface IAsteriodGrain : IGrainWithIntegerKey
{
    Task MoveAsync();
}

