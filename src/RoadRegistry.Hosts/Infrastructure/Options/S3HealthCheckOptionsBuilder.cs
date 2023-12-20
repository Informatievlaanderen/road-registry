namespace RoadRegistry.Hosts.Infrastructure.Options;

using Microsoft.Extensions.Configuration;

public class S3HealthCheckOptionsBuilder : HealthCheckPermissionOptionsBuilder<S3HealthCheckOptions>
{
    private readonly IConfiguration _configuration;

    internal S3HealthCheckOptionsBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override S3HealthCheckOptions Build()
    {
        return new S3HealthCheckOptions();
    }
}
