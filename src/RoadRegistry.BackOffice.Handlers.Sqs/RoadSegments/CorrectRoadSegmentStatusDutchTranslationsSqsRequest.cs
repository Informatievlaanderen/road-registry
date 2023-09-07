namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class CorrectRoadSegmentStatusDutchTranslationsSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRoadSegmentStatusDutchTranslationsRequest>
{
    public CorrectRoadSegmentStatusDutchTranslationsRequest Request { get; init; }

    public CorrectRoadSegmentStatusDutchTranslationsSqsRequest()
    {
        Request = new CorrectRoadSegmentStatusDutchTranslationsRequest();
    }
}
