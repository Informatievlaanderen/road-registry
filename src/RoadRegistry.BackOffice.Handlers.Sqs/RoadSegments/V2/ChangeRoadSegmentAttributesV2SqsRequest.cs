namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using System.Collections.Generic;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

// Bulk "wijzig attribuutwaarden" request. The API layer already parsed every Dutch value into its value object
// (validation = "does it parse"), so the request carries hard types. Positions stay nullable here: a null vanPositie
// means 0 and a null totPositie means "the end of that particular segment" - resolved per segment in the lambda,
// where the segment's true length is known.
[BlobRequest]
public sealed class ChangeRoadSegmentAttributesV2SqsRequest : SqsRequest
{
    public required IReadOnlyList<ChangeRoadSegmentAttributesV2Group> Groups { get; init; }
}

// One request object: the segments it applies to plus the new value(s) for one or more attributes.
public sealed record ChangeRoadSegmentAttributesV2Group
{
    public required IReadOnlyList<RoadSegmentId> RoadSegmentIds { get; init; }

    public IReadOnlyList<AttributeValue<RoadSegmentMorphologyV2>>? Morphology { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentSurfaceTypeV2>>? SurfaceType { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentAccessRestrictionV2>>? AccessRestriction { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentCategoryV2>>? Category { get; init; }
    public IReadOnlyList<SidedAttributeValue<StreetNameLocalId>>? StreetName { get; init; }
    public IReadOnlyList<SidedAttributeValue<OrganizationId>>? MaintenanceAuthority { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentTrafficDirection>>? CarTrafficDirection { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentTrafficDirection>>? BikeTrafficDirection { get; init; }
    public IReadOnlyList<AttributeValue<RoadSegmentPedestrianTrafficDirection>>? PedestrianTrafficDirection { get; init; }
}

// A linear-referenced value: [from, to] (both nullable) with the (hard-typed) attribute value.
public sealed record AttributeValue<T>(RoadSegmentPositionV2? FromPosition, RoadSegmentPositionV2? ToPosition, T Value);

// A sided linear-referenced value (straatnaam / wegbeheerder).
public sealed record SidedAttributeValue<T>(RoadSegmentAttributeSide Side, RoadSegmentPositionV2? FromPosition, RoadSegmentPositionV2? ToPosition, T Value);
