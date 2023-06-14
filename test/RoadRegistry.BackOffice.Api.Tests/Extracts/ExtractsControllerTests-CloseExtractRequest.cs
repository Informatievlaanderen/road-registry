namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Abstractions;
using FluentValidation;
using Messages;
using Xunit.Sdk;

public partial class ExtractsControllerTests
{
    //[Fact]
    public void Close_extract_request()
    {
        var message = new CloseRoadNetworkExtract
        {
            DownloadId = new DownloadId(Guid.NewGuid()),
            Reason = RoadNetworkExtractCloseReason.InformativeExtract
        };
    }
}
