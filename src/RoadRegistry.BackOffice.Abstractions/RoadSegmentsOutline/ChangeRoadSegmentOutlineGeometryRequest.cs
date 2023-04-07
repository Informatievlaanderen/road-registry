namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;
using Messages;

public sealed record ChangeRoadSegmentOutlineGeometryRequest(RoadSegmentId RoadSegmentId, RoadSegmentGeometry Geometry) : IRequest<ETagResponse>;
