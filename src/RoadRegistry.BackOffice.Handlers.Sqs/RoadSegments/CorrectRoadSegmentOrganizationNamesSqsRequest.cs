namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public sealed class CorrectRoadSegmentOrganizationNamesSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRoadSegmentOrganizationNamesRequest>
{
    public CorrectRoadSegmentOrganizationNamesRequest Request { get; init; }

    public CorrectRoadSegmentOrganizationNamesSqsRequest()
    {
        Request = new CorrectRoadSegmentOrganizationNamesRequest();
    }
}
