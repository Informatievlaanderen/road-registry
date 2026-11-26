namespace RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

/// <summary>
/// System-internal request that lets V2 road segments go of a street name that was removed or became inactive. It is
/// published only by the StreetNameEventConsumer via a direct SQS copy and handled by a lambda with ticketing disabled.
/// See <see cref="ISystemSqsRequest"/> — do not expose this through the API or reuse it from user-facing flows.
/// </summary>
[BlobRequest]
public sealed class SystemUnlinkRoadSegmentsFromStreetNameSqsRequest : SqsRequest, ISystemSqsRequest
{
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; init; }
    public required StreetNameLocalId StreetNameId { get; init; }
}
