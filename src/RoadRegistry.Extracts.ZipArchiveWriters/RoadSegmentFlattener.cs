namespace RoadRegistry.Extracts.ZipArchiveWriters;

using System.Collections;
using System.Reflection;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extracts.Projections;
using RoadSegment.ValueObjects;

public static class RoadSegmentFlattener
{
    public static IReadOnlyList<FlatRoadSegment> Flatten(this RoadSegmentExtractItem roadSegment)
    {
        try
        {
            var positions = GetPositions(roadSegment);

            var flatSegments = new List<FlatRoadSegment>();

            var from = positions[0];
            for (var i = 1; i < positions.Count; i++)
            {
                var to = positions[i];

                var toActualDistance = i < positions.Count - 1 ? to.ToDouble() : roadSegment.Geometry.Value.Length;
                var lil = new LengthIndexedLine(roadSegment.Geometry.Value);
                var fromToGeometry = ((LineString)lil.ExtractLine(from, toActualDistance)).ToMultiLineString();

                flatSegments.Add(new FlatRoadSegment
                {
                    RoadSegmentId = roadSegment.RoadSegmentId,
                    Geometry = RoadSegmentGeometry.Create(fromToGeometry),
                    GeometryDrawMethod = roadSegment.GeometryDrawMethod,
                    AccessRestriction = roadSegment.AccessRestriction.GetValue(from, to),
                    Category = roadSegment.Category.GetValue(from, to),
                    Morphology = roadSegment.Morphology.GetValue(from, to),
                    Status = roadSegment.Status.GetValue(from, to),
                    LeftStreetNameId = roadSegment.StreetNameId.GetValue(from, to, RoadSegmentAttributeSide.Left),
                    RightStreetNameId = roadSegment.StreetNameId.GetValue(from, to, RoadSegmentAttributeSide.Right),
                    LeftMaintenanceAuthorityId = roadSegment.MaintenanceAuthorityId.GetValue(from, to, RoadSegmentAttributeSide.Left),
                    RightMaintenanceAuthorityId = roadSegment.MaintenanceAuthorityId.GetValue(from, to, RoadSegmentAttributeSide.Right),
                    SurfaceType = roadSegment.SurfaceType.GetValue(from, to),
                    CarAccess = roadSegment.CarAccess.TryGetValue(from, to, RoadSegmentAttributeSide.Both, out var carAccess) ? carAccess : null,
                    BikeAccess = roadSegment.BikeAccess.TryGetValue(from, to, RoadSegmentAttributeSide.Both, out var bikeAccess) ? bikeAccess : null,
                    PedestrianAccess = roadSegment.PedestrianAccess.TryGetValue(from, to, RoadSegmentAttributeSide.Both, out var pedestrianAccess) ? pedestrianAccess : null,
                    EuropeanRoadNumbers = roadSegment.EuropeanRoadNumbers,
                    NationalRoadNumbers = roadSegment.NationalRoadNumbers,
                    Origin = roadSegment.Origin,
                    LastModified = roadSegment.LastModified,
                    IsV2 = roadSegment.IsV2
                });

                from = positions[i];
            }

            return flatSegments;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to flatten RoadSegment {roadSegment.Id}: {ex.Message}", ex);
        }
    }

    private static IReadOnlyList<RoadSegmentPositionV2> GetPositions(RoadSegmentExtractItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var result = new List<RoadSegmentPositionV2>();

        var dynamicProperties = GetDynamicAttributeProperties(typeof(RoadSegmentExtractItem)).ToList();
        foreach (var prop in dynamicProperties)
        {
            var dynObj = prop.GetValue(item);
            if (dynObj is null)
            {
                continue;
            }

            var valuesProp = dynObj.GetType().GetProperty(nameof(ExtractRoadSegmentDynamicAttribute<object>.Values), BindingFlags.Instance | BindingFlags.Public);
            if (valuesProp?.GetValue(dynObj) is not IEnumerable values)
            {
                continue;
            }

            foreach (var v in values)
            {
                if (v is not IExtractRoadSegmentDynamicAttributeValueCoverage coverage)
                {
                    continue;
                }

                if (!result.Any(x => x.IsReasonablyEqualTo(coverage.From)))
                {
                    result.Add(coverage.From);
                }

                if (!result.Any(x => x.IsReasonablyEqualTo(coverage.To)))
                {
                    result.Add(coverage.To);
                }
            }
        }

        return result.OrderBy(x => x).Select(x => new RoadSegmentPositionV2(x)).ToList();
    }

    private static IEnumerable<PropertyInfo> GetDynamicAttributeProperties(Type itemType)
    {
        var openGeneric = typeof(ExtractRoadSegmentDynamicAttribute<>);

        return itemType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == openGeneric &&
                p.GetMethod is not null);
    }

    private static T GetValue<T>(this ExtractRoadSegmentDynamicAttribute<T> attributeValues, RoadSegmentPositionV2 from, RoadSegmentPositionV2 to)
    {
        return GetValue(attributeValues, from, to, RoadSegmentAttributeSide.Both);
    }

    private static T GetValue<T>(this ExtractRoadSegmentDynamicAttribute<T> attributeValues, RoadSegmentPositionV2 from, RoadSegmentPositionV2 to, RoadSegmentAttributeSide side)
    {
        TryGetValue(attributeValues, from, to, side, out var value);
        return value;
    }

    private static bool TryGetValue<T>(this ExtractRoadSegmentDynamicAttribute<T> attributeValues, RoadSegmentPositionV2 from, RoadSegmentPositionV2 to, RoadSegmentAttributeSide side, out T value)
    {
        var values = attributeValues.Values
            .Where(x => from >= x.From && to <= x.To)
            .ToList();

        if (!values.Any())
        {
            if (typeof(T) == typeof(string))
            {
                //TODO-pr temp
            }
            value = default;
            return false;
        }

        switch (side)
        {
            case RoadSegmentAttributeSide.Both:
                value = values.Single(x => x.Side == RoadSegmentAttributeSide.Both).Value;
                return true;
            case RoadSegmentAttributeSide.Left:
            case RoadSegmentAttributeSide.Right:
                value = values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == side).Value;
                return true;
            default:
                throw new ArgumentOutOfRangeException(side.ToString());
        }
    }
}

public sealed class FlatRoadSegment
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required string GeometryDrawMethod { get; init; }
    public required string AccessRestriction { get; init; }
    public required string Category { get; init; }
    public required string Morphology { get; init; }
    public required string Status { get; init; }
    public required StreetNameLocalId LeftStreetNameId { get; init; }
    public required StreetNameLocalId RightStreetNameId { get; init; }
    public required OrganizationId LeftMaintenanceAuthorityId { get; init; }
    public required OrganizationId RightMaintenanceAuthorityId { get; init; }
    public required string SurfaceType { get; init; }
    public required VehicleAccess? CarAccess { get; init; }
    public required VehicleAccess? BikeAccess { get; init; }
    public required bool? PedestrianAccess { get; init; }
    public required List<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public required List<NationalRoadNumber> NationalRoadNumbers { get; init; }
    public required EventTimestamp Origin { get; init; }
    public required EventTimestamp LastModified { get; init; }
    public required bool IsV2 { get; init; }
}
