namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using RoadRegistry.Extracts.DataValidation;

/// <summary>
/// Datavalidatie says in the response body why it refused a request. Without it a failure only shows up in our logs as
/// "400 (Bad Request)", which tells us nothing, so the body has to end up in the exception message.
/// </summary>
public class DataValidationApiClientErrorResponseTests
{
    [Fact]
    public async Task WhenRequestDataValidationFails_ThenTheResponseBodyIsPartOfTheExceptionMessage()
    {
        var apiClient = BuildApiClient(HttpStatusCode.BadRequest, "{\"detail\":\"specificationCode 'WR' is unknown\"}");

        var act = async () => await apiClient.RequestDataValidationAsync(
            new UploadId(Guid.NewGuid()),
            new MemoryStream([1, 2, 3]),
            DataValidationDeliveryMetadata.Empty,
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Message.Should().Contain("400");
        exception.Which.Message.Should().Contain("specificationCode 'WR' is unknown");
    }

    [Fact]
    public async Task WhenPollDeliveryFails_ThenTheResponseBodyIsPartOfTheExceptionMessage()
    {
        var apiClient = BuildApiClient(HttpStatusCode.NotFound, "delivery not found");

        var act = async () => await apiClient.PollDeliveryAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.Which.Message.Should().Contain("delivery not found");
    }

    [Fact]
    public async Task WhenTheResponseHasNoBody_ThenTheExceptionMessageSaysSo()
    {
        var apiClient = BuildApiClient(HttpStatusCode.BadRequest, string.Empty);

        var act = async () => await apiClient.GetDeliveryResultAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.Message.Should().Contain("<empty>");
    }

    private static IDataValidationApiClient BuildApiClient(HttpStatusCode statusCode, string responseBody)
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new StubHttpMessageHandler(statusCode, responseBody)));

        var tokenProvider = new Mock<IDataValidationTokenProvider>();
        tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("token");

        return new DataValidationApiClient(
            httpClientFactory.Object,
            new DataValidationOptions
            {
                ApiBaseUrl = "https://datavalidation.example/api",
                SpecificationCode = "WR"
            },
            tokenProvider.Object);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
