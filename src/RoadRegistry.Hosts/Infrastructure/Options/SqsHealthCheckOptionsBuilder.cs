namespace RoadRegistry.Hosts.Infrastructure.Options;

using Microsoft.Extensions.Configuration;

public class SqsHealthCheckOptionsBuilder : HealthCheckPermissionOptionsBuilder<SqsHealthCheckOptions>
{
    private readonly IConfiguration _configuration;

    internal SqsHealthCheckOptionsBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override SqsHealthCheckOptions Build()
    {
        return new SqsHealthCheckOptions();
    }
}
