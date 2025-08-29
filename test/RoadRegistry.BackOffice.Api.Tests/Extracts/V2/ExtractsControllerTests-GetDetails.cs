namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions;
using Xunit.Sdk;

public partial class ExtractsControllerTests
{

    [Fact(Skip = "//TODO-pr implement test")]
    public async Task When_checking_extract_status_with_invalid_uploadid()
    {
        throw new NotImplementedException();
        // try
        // {
        //     await Controller.GetStatus(
        //         "not_a_guid_without_dashes",
        //         new ExtractUploadsOptions(),
        //         CancellationToken.None);
        //     throw new XunitException("Expected a validation exception but did not receive any");
        // }
        // catch (ValidationException)
        // {
        // }
    }

    [Fact(Skip = "//TODO-pr implement test")]
    public async Task When_checking_extract_status_with_unknown_uploadid()
    {
        throw new NotImplementedException();
        // var result = await Controller.GetStatus(
        //     Guid.NewGuid().ToString("N"),
        //     new ExtractUploadsOptions(),
        //     CancellationToken.None);
        //
        // Assert.IsType<NotFoundResult>(result);
    }
}
