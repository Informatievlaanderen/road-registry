namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using FluentValidation;
using RoadRegistry.BackOffice.Abstractions;
using Xunit.Sdk;

public partial class ExtractsControllerTests
{
    [Fact(Skip = "//TODO-pr implement test")]
    public async Task WhenClosingRequest()
    {
        throw new NotImplementedException();
        // try
        // {
        //     await Controller.GetDownload(
        //         "not_a_guid_without_dashes",
        //         new ExtractDownloadsOptions(),
        //         CancellationToken.None);
        //     throw new XunitException("Expected a validation exception but did not receive any");
        // }
        // catch (ValidationException)
        // {
        // }
    }
}
