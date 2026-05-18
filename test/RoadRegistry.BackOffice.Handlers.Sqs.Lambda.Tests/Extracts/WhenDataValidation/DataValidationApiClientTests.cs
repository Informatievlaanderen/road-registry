namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Hosts.Infrastructure.Extensions;

public class DataValidationApiClientTests
{
    [Fact]
    public async Task WhenRequestDataValidation_ThenDeliveryId()
    {
        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

        var sp = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddHttpClient()
            .RegisterOptions<DataValidationOptions>()
            .AddSingleton<IDataValidationTokenProvider, DataValidationTokenProvider>()
            .AddSingleton<IDataValidationApiClient, DataValidationApiClient>()
            .BuildServiceProvider();

        var apiClient = sp.GetRequiredService<IDataValidationApiClient>();

        var deliveryId = "b96ef20c-8823-472f-b8a4-7e1d49fd0056";

        var fileStream = File.OpenRead(@"C:\Users\RikDePeuter\Downloads\e8e504cbf2b044b0a44f5babdfdb71aa.zip");
        var result = await apiClient.RequestDataValidationAsync(new UploadId(Guid.NewGuid()), fileStream, CancellationToken.None);

        //TODO-pr test polling and status report
    }
}
