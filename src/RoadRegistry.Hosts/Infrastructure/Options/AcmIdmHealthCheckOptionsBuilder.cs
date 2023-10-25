namespace RoadRegistry.Hosts.Infrastructure.Options;

public class AcmIdmHealthCheckOptionsBuilder : HealthCheckOptionsBuilder<AcmIdmHealthCheckOptions>
{
    public override bool IsValid => true;

    public override AcmIdmHealthCheckOptions Build()
    {
        return new AcmIdmHealthCheckOptions();
    }
}
