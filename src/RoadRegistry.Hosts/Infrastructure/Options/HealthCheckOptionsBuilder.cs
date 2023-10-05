namespace RoadRegistry.Hosts.Infrastructure.Options;

public abstract class HealthCheckOptionsBuilder<TOptions> : IHealthCheckOptionsBuilder
{
    public abstract bool IsValid { get; }
    public abstract TOptions Build();
}

public interface IHealthCheckOptionsBuilder
{
    public bool IsValid { get; }
}
