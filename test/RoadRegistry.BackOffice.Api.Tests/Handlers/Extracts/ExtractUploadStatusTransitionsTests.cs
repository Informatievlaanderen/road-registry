namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using FluentAssertions;
using RoadRegistry.Extracts.Schema;

// Which failure a rejection by the road network is depends on how far the delivery had come.
public class ExtractUploadStatusTransitionsTests
{
    // Validation had succeeded - ours, and Datavalidatie's where it applies - so the delivery was approved and it is
    // the processing of its changes that failed. That is the state Digitaal Vlaanderen corrects and the uploader is
    // not told about.
    //
    // ProcessingFailed answers the same, because the same rejection can arrive twice - a retried batch, a
    // replay - and the second time the delivery no longer sits on the status it was approved with.
    [Theory]
    [InlineData(ExtractUploadStatus.AutomaticValidationSucceeded)]
    [InlineData(ExtractUploadStatus.ProcessingFailed)]
    public void GivenValidationSucceeded_ThenTheProcessingFailed(ExtractUploadStatus currentStatus)
    {
        ExtractUploadStatusTransitions
            .OnRoadNetworkChangesRejected(currentStatus)
            .Should().Be(ExtractUploadStatus.ProcessingFailed);
    }

    [Theory]
    [InlineData(ExtractUploadStatus.Processing)]
    [InlineData(ExtractUploadStatus.Accepted)]
    [InlineData(ExtractUploadStatus.AutomaticValidationFailed)]
    [InlineData(ExtractUploadStatus.ManualValidationFailed)]
    public void GivenAnythingElse_ThenTheUploadWasRefusedAsBefore(ExtractUploadStatus currentStatus)
    {
        ExtractUploadStatusTransitions
            .OnRoadNetworkChangesRejected(currentStatus)
            .Should().Be(ExtractUploadStatus.AutomaticValidationFailed);
    }
}
