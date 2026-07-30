namespace RoadRegistry.Extracts.ZipArchiveWriters;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.RoadSegment.Flattening;
using RoadSegment.ValueObjects;
using static RoadRegistry.RoadSegment.Flattening.RoadSegmentFlattenEngine;

public static class RoadSegmentFlattener
{
    private static int SideLeft => RoadSegmentAttributeSide.Links.Translation.Identifier;
    private static int SideRight => RoadSegmentAttributeSide.Rechts.Translation.Identifier;

    // Flattens the extract item into its smaller sub-segments through the shared RoadSegmentFlattenEngine (the same
    // split/normalize/resolve algorithm the WmsWfsV2 and PBS derived-wegsegment flatteners use), then maps each
    // sub-range to a FlatRoadSegment.
    public static IReadOnlyList<FlatRoadSegment> Flatten(this RoadSegmentExtractItem roadSegment)
    {
        try
        {
            var length = roadSegment.Geometry.Value.Length;

            var access = Normalize(roadSegment.AccessRestriction.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var category = Normalize(roadSegment.Category.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var morphology = Normalize(roadSegment.Morphology.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var surface = Normalize(roadSegment.SurfaceType.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var streetName = Normalize(roadSegment.StreetNameId.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var maintainer = Normalize(roadSegment.MaintenanceAuthorityId.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var carForward = Normalize(roadSegment.CarAccessForward.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var carBackward = Normalize(roadSegment.CarAccessBackward.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var bikeForward = Normalize(roadSegment.BikeAccessForward.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var bikeBackward = Normalize(roadSegment.BikeAccessBackward.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());
            var pedestrian = Normalize(roadSegment.PedestrianAccess.Values, length, x => x.From.ToDouble(), x => x.To.ToDouble());

            var ranges = Ranges(length,
                access.Pairs(), category.Pairs(), morphology.Pairs(), surface.Pairs(),
                streetName.Pairs(), maintainer.Pairs(),
                carForward.Pairs(), carBackward.Pairs(), bikeForward.Pairs(), bikeBackward.Pairs(), pedestrian.Pairs());

            var lengthIndexedLine = new LengthIndexedLine(roadSegment.Geometry.Value);
            var flatSegments = new List<FlatRoadSegment>();

            foreach (var range in ranges)
            {
                var from = range.From;
                var to = range.To;

                var fromToGeometry = ((LineString)lengthIndexedLine.ExtractLine(from, range.ToActual)).ToMultiLineString();

                flatSegments.Add(new FlatRoadSegment
                {
                    RoadSegmentId = roadSegment.RoadSegmentId,
                    Geometry = RoadSegmentGeometry.Create(fromToGeometry),
                    GeometryDrawMethod = roadSegment.GeometryDrawMethod,
                    Status = roadSegment.Status,
                    AccessRestriction = Resolve(access, from, to)?.Value!,
                    Category = Resolve(category, from, to)?.Value!,
                    Morphology = Resolve(morphology, from, to)?.Value!,
                    LeftStreetNameId = ResolveSided(streetName, from, to, SideLeft, x => x.Side.Translation.Identifier)?.Value ?? default,
                    RightStreetNameId = ResolveSided(streetName, from, to, SideRight, x => x.Side.Translation.Identifier)?.Value ?? default,
                    LeftMaintenanceAuthorityId = ResolveSided(maintainer, from, to, SideLeft, x => x.Side.Translation.Identifier)?.Value ?? default,
                    RightMaintenanceAuthorityId = ResolveSided(maintainer, from, to, SideRight, x => x.Side.Translation.Identifier)?.Value ?? default,
                    SurfaceType = Resolve(surface, from, to)?.Value!,
                    CarAccessForward = Resolve(carForward, from, to)?.Value,
                    CarAccessBackward = Resolve(carBackward, from, to)?.Value,
                    BikeAccessForward = Resolve(bikeForward, from, to)?.Value,
                    BikeAccessBackward = Resolve(bikeBackward, from, to)?.Value,
                    PedestrianAccess = Resolve(pedestrian, from, to)?.Value,
                    EuropeanRoadNumbers = roadSegment.EuropeanRoadNumbers,
                    NationalRoadNumbers = roadSegment.NationalRoadNumbers,
                    Origin = roadSegment.Origin,
                    LastModified = roadSegment.LastModified,
                    IsV2 = roadSegment.IsV2
                });
            }

            return flatSegments;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to flatten RoadSegment {roadSegment.Id}: {ex.Message}", ex);
        }
    }
}

public sealed class FlatRoadSegment
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required string GeometryDrawMethod { get; init; }
    public required string Status { get; init; }
    public required string AccessRestriction { get; init; }
    public required string Category { get; init; }
    public required string Morphology { get; init; }
    public required StreetNameLocalId LeftStreetNameId { get; init; }
    public required StreetNameLocalId RightStreetNameId { get; init; }
    public required OrganizationId LeftMaintenanceAuthorityId { get; init; }
    public required OrganizationId RightMaintenanceAuthorityId { get; init; }
    public required string SurfaceType { get; init; }
    public required bool? CarAccessForward { get; init; }
    public required bool? CarAccessBackward { get; init; }
    public required bool? BikeAccessForward { get; init; }
    public required bool? BikeAccessBackward { get; init; }
    public required bool? PedestrianAccess { get; init; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; init; }
    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; init; }
    public required bool IsV2 { get; init; }
}
