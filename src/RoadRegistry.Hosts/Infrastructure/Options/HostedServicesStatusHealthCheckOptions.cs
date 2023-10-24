using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace RoadRegistry.Hosts.Infrastructure.Options;

public class HostedServicesStatusHealthCheckOptions
{
    public ICollection<IHostedService> HostedServices { get; set; }
}
