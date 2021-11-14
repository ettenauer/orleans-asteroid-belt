using System.Threading.Tasks;

namespace Orleans.AsteriodBelt.Grains;

public interface IGravityGrain : IGrainWithIntegerKey
{
    Task RegistryAsteriodAsync(int asteriodId);
}

