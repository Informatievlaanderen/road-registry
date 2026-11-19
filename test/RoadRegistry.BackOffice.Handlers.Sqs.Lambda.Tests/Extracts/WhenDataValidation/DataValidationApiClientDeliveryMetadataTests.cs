namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using RoadRegistry.Extracts.DataValidation;

/// <summary>
/// The names of the multipart parts are the contract with Datavalidatie - they are read off their OpenAPI document
/// and nothing in the type system keeps them right - so they are pinned down here.
/// </summary>
public class DataValidationApiClientDeliveryMetadataTests
{
    [Fact]
    public async Task WhenMetadataIsKnown_ThenItIsSentAsFormFields()
    {
        var (apiClient, capture) = BuildApiClient();

        await apiClient.RequestDataValidationAsync(
            new UploadId(Guid.NewGuid()),
            new MemoryStream([1, 2, 3]),
            new DataValidationDeliveryMetadata("0316380841", "6bcbfd27-3e9b-4e18-9b37-5d0b0ae37c6d", "44021"),
            CancellationToken.None);

        var content = capture.Content!;
        content.Should().Contain("name=specificationCode");
        content.Should().Contain("name=delivery");
        content.Should().Contain("name=metadata.surveyorOrganisationKbo");
        content.Should().Contain("0316380841");
        content.Should().Contain("name=metadata.correlationId");
        content.Should().Contain("6bcbfd27-3e9b-4e18-9b37-5d0b0ae37c6d");
        content.Should().Contain("name=metadata.nisCode");
        content.Should().Contain("44021");
    }

    [Fact]
    public async Task WhenMetadataIsUnknown_ThenThoseFormFieldsAreLeftOut()
    {
        var (apiClient, capture) = BuildApiClient();

        await apiClient.RequestDataValidationAsync(
            new UploadId(Guid.NewGuid()),
            new MemoryStream([1, 2, 3]),
            DataValidationDeliveryMetadata.Empty,
            CancellationToken.None);

        var content = capture.Content!;
        content.Should().Contain("name=specificationCode");
        content.Should().Contain("name=delivery");
        content.Should().NotContain("name=metadata.");
    }

    private static (IDataValidationApiClient ApiClient, RequestCapture Capture) BuildApiClient()
    {
        var capture = new RequestCapture();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new CapturingHttpMessageHandler(capture)));

        var tokenProvider = new Mock<IDataValidationTokenProvider>();
        tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        var apiClient = new DataValidationApiClient(
            httpClientFactory.Object,
            new DataValidationOptions
            {
                ApiBaseUrl = "https://datavalidation.example/api",
                SpecificationCode = "WR"
            },
            tokenProvider.Object);

        return (apiClient, capture);
    }

    private sealed class RequestCapture
    {
        public string? Content { get; set; }
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly RequestCapture _capture;

        public CapturingHttpMessageHandler(RequestCapture capture)
        {
            _capture = capture;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _capture.Content = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"id\":\"{Guid.NewGuid()}\"}}", Encoding.UTF8, "application/json")
            };
        }
    }
}
