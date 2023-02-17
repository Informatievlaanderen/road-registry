namespace RoadRegistry.Legacy.Import;

using BackOffice;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddSingleton(
                    new SqlConnection(
                        hostContext.Configuration.GetConnectionString(WellknownConnectionNames.Events)
                    )
                )
                .AddSingleton(new LegacyStreamArchiveReader(
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                        DateParseHandling = DateParseHandling.DateTime,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }
                ))
                .AddSingleton<LegacyStreamEventsWriter>())
            .Build();

        await roadRegistryHost.RunAsync();
    }
}
