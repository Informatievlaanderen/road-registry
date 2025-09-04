namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenCloseExtract;

using Abstractions.Extracts.V2;
using AutoFixture;
using Xunit.Abstractions;

public class WithInvalidRequest : WhenCloseExtractTestBase
{
    public WithInvalidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task GivenUnknownDownloadId_ThenFailed()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new CloseExtractRequest(
            downloadId
        );

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError("ExtractNietGekend", "Het extract werd niet gevonden.");
    }
}
