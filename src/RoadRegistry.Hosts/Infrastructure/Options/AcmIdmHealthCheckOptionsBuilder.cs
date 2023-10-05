namespace RoadRegistry.Hosts.Infrastructure.Options;

using System;

public class AcmIdmHealthCheckOptionsBuilder : HealthCheckOptionsBuilder<AcmIdmHealthCheckOptions>
{
    public override bool IsValid { get; }
    public override AcmIdmHealthCheckOptions Build()
    {
        throw new NotImplementedException();
    }
}
