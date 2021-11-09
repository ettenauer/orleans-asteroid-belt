namespace Orleans.AsteriodBelt.Grains.DomainObjects;

public class AsteroidState
{
    public long AsteroidId { get; init; }

    public double X { get; init; }

    public double Y { get; init; }

    public int Weight { get; init; }

    public bool Destroyed { get; init; }
}
