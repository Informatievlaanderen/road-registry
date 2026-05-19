namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Hosts.Infrastructure.Extensions;

public class DataValidationApiClientTests
{
    //[Fact]
    [Fact(Skip = "For debugging")]
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

        var fileStream = File.OpenRead(@"27ad54a9778a4b51a1b3e35a5211032f.zip");
        var deliveryId = await apiClient.RequestDataValidationAsync(new UploadId(Guid.NewGuid()), fileStream, CancellationToken.None);
        //var deliveryId = "8114d8a4-6ffb-47f1-ac68-85448a379954";

        deliveryId.Should().NotBeNullOrEmpty();

        while (true)
        {
            var pollResult = await apiClient.PollDeliveryAsync(deliveryId, CancellationToken.None);
            if (pollResult.Result != ValidationResult.NotYetAvailable)
            {
                var detailResult = await apiClient.GetDeliveryResultAsync(deliveryId, CancellationToken.None);
            }

            switch (pollResult.Status)
            {
                case ValidationJobStatus.Processing:
                    if (pollResult.Stage == "automaticchecks")
                    {
                        var detailResult = await apiClient.GetDeliveryResultAsync(deliveryId, CancellationToken.None);
                    }
                    break;
            }
        }
    }
}
