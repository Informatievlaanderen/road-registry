namespace RoadRegistry.Hosts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class RoadRegistryHostStartup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app.ApplicationServices.GetService<HealthCheckService>() is not null)
        {
            app.UseHealthChecks();
        }
    }
}
