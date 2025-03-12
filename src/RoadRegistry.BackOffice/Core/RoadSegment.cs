namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NetTopologySuite.Geometries;

public class RoadSegment
{
    public RoadSegment(RoadSegmentId id,
        RoadSegmentVersion version,
        MultiLineString geometry,
        GeometryVersion geometryVersion,
        RoadNodeId start,
        RoadNodeId end,
        AttributeHash attributeHash,
        string lastEventHash)
        : this(id,
            version,
            geometry,
            geometryVersion,
            start,
            end,
            attributeHash,
            Array.Empty<BackOffice.RoadSegmentLaneAttribute>(),
            Array.Empty<BackOffice.RoadSegmentSurfaceAttribute>(),
            Array.Empty<BackOffice.RoadSegmentWidthAttribute>(),
            lastEventHash)
    {
    }

    public RoadSegment(RoadSegmentId id,
        RoadSegmentVersion version,
        MultiLineString geometry,
        GeometryVersion geometryVersion,
        RoadNodeId start,
        RoadNodeId end,
        AttributeHash attributeHash,
        IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> lanes,
        IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> widths,
        string lastEventHash)
        : this(id,
            version,
            geometry,
            geometryVersion,
            start,
            end,
            attributeHash,
            ImmutableDictionary<AttributeId, RoadSegmentEuropeanRoadAttribute>.Empty,
            ImmutableDictionary<AttributeId, RoadSegmentNationalRoadAttribute>.Empty,
            ImmutableDictionary<AttributeId, RoadSegmentNumberedRoadAttribute>.Empty,
            lanes,
            surfaces,
            widths,
            lastEventHash)
    {
    }

    private RoadSegment(RoadSegmentId id,
        RoadSegmentVersion version,
        MultiLineString geometry,
        GeometryVersion geometryVersion,
        RoadNodeId start,
        RoadNodeId end,
        AttributeHash attributeHash,
        ImmutableDictionary<AttributeId, RoadSegmentEuropeanRoadAttribute> europeanRoadAttributes,
        ImmutableDictionary<AttributeId, RoadSegmentNationalRoadAttribute> nationalRoadAttributes,
        ImmutableDictionary<AttributeId, RoadSegmentNumberedRoadAttribute> numberedRoadAttributes,
        IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> lanes,
        IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> widths,
        string lastEventHash)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        if (attributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined && start == end)
        {
            throw new ArgumentException($"The start and end can not be the same road node ({start}) for road segment with ID {id}.", nameof(start));
        }

        Id = id;
        Version = version;
        Geometry = geometry;
        GeometryVersion = geometryVersion;
        Start = start;
        End = end;
        AttributeHash = attributeHash;
        EuropeanRoadAttributes = europeanRoadAttributes;
        NationalRoadAttributes = nationalRoadAttributes;
        NumberedRoadAttributes = numberedRoadAttributes;
        Lanes = lanes;
        Surfaces = surfaces;
        Widths = widths;
        LastEventHash = lastEventHash;
    }

    public AttributeHash AttributeHash { get; }
    public RoadNodeId End { get; }
    public MultiLineString Geometry { get; }
    public GeometryVersion GeometryVersion { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentVersion Version { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> Lanes { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> Widths { get; }
    public string LastEventHash { get; }

    public IEnumerable<RoadNodeId> Nodes
    {
        get
        {
            yield return Start;
            yield return End;
        }
    }

    public ImmutableDictionary<AttributeId, RoadSegmentEuropeanRoadAttribute> EuropeanRoadAttributes { get; }
    public ImmutableDictionary<AttributeId, RoadSegmentNationalRoadAttribute> NationalRoadAttributes { get; }
    public ImmutableDictionary<AttributeId, RoadSegmentNumberedRoadAttribute> NumberedRoadAttributes { get; }
    public RoadNodeId Start { get; }

    public RoadSegment NotPartOfEuropeanRoad(AttributeId attributeId)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes.Remove(attributeId), NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment NotPartOfNationalRoad(AttributeId attributeId)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes.Remove(attributeId), NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment NotPartOfNumberedRoad(AttributeId attributeId)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes.Remove(attributeId), Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment PartOfEuropeanRoad(RoadSegmentEuropeanRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes.SetItem(attribute.AttributeId, attribute), NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment PartOfNationalRoad(RoadSegmentNationalRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes.SetItem(attribute.AttributeId, attribute), NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment PartOfNumberedRoad(RoadSegmentNumberedRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes.SetItem(attribute.AttributeId, attribute), Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithAttributeHash(AttributeHash hash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, hash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithGeometry(MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return new RoadSegment(Id, Version, geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new RoadSegment(Id, Version, Geometry, geometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithLastEventHash(string lastEventHash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, lastEventHash);
    }

    public RoadSegment WithStartAndEndAndAttributeHash(RoadNodeId start, RoadNodeId end, AttributeHash attributeHash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, start, end, attributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new RoadSegment(Id, version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithLanes(IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> lanes)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, lanes, Surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithSurfaces(IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> surfaces)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, surfaces, Widths, LastEventHash);
    }

    public RoadSegment WithWidths(IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> widths)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, Lanes, Surfaces, widths, LastEventHash);
    }

    public RoadNodeId? GetOppositeNode(RoadNodeId id)
    {
        if (Start == id)
        {
            return End;
        }

        if (End == id)
        {
            return Start;
        }

        return null;
    }

    public RoadNodeId? GetCommonNode(RoadSegment other)
    {
        if (Start == other.Start || Start == other.End)
        {
            return Start;
        }

        if (End == other.Start || End == other.End)
        {
            return End;
        }

        return null;
    }
}
