namespace RoadRegistry.BackOffice.Api.Tests.Extracts;
using Messages;

public partial class ExtractsControllerTests
{
    //[Fact]
    public void Close_extract_request()
    {
        var message = new CloseRoadNetworkExtract
        {
            ExternalRequestId = new ExternalExtractRequestId("TEST"),
            Reason = RoadNetworkExtractCloseReason.InformativeExtract
        };
    }
}
