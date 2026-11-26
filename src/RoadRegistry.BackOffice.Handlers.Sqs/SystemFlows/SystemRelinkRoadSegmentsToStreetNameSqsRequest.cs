namespace RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

/// <summary>
/// System-internal request that relinks V2 road segments from an old to a new street name id, a renamed street name
/// for instance. It is published only by the StreetNameEventConsumer via a direct SQS copy and handled by a lambda with
/// ticketing disabled. See <see cref="ISystemSqsRequest"/> — do not expose this through the API or reuse it from
/// user-facing flows. Letting go of a street name is <see cref="SystemUnlinkRoadSegmentsFromStreetNameSqsRequest"/>.
/// </summary>
[BlobRequest]
public sealed class SystemRelinkRoadSegmentsToStreetNameSqsRequest : SqsRequest, ISystemSqsRequest
{
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; init; }
    public required StreetNameLocalId OldStreetNameId { get; init; }
    public required StreetNameLocalId NewStreetNameId { get; init; }
}
