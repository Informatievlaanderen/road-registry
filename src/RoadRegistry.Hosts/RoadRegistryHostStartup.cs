namespace RoadRegistry.Hosts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class RoadRegistryHostStartup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHealthChecks();
    }
}
