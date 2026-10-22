namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using System.Collections.Generic;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

// 'Wijzig de geometrie van een wegsegment'. Unlike the attribute change this is never a bulk operation: the order in
// which geometries are changed can affect the result, so one segment per request.
//
// The API layer already parsed every Dutch value into its value object (validation = "does it parse"), so the request
// carries hard types. Positions stay nullable here: a null vanPositie means 0 and a null totPositie means the end of
// the segment - resolved in the lambda against the length of the NEW geometry, which is what the attributes will
// apply to.
[BlobRequest]
public sealed class ChangeRoadSegmentGeometryV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }

    // VAL-5: which road segments the caller may modify, read off the scopes on their token - including any segment
    // dragged along by a road node that moves, which is why the entitlement travels with the request rather than the
    // verdict: only the domain knows which segments are actually affected.
    public required EditableGeometryDrawMethods EditableGeometryDrawMethods { get; init; }

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
