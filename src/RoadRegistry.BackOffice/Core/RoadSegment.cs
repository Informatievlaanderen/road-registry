namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NetTopologySuite.Geometries;

public class RoadSegment
{
    public RoadSegment(RoadSegmentId id, RoadSegmentVersion version, MultiLineString geometry, GeometryVersion geometryVersion, RoadNodeId start, RoadNodeId end, AttributeHash attributeHash, string lastEventHash)
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
        string lastEventHash)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        
        if (attributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined.ToString() && start == end)
        {
            throw new ArgumentException($"The start and end can not be the same road node ({start}).", nameof(start));
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
        LastEventHash = lastEventHash;
    }

    public AttributeHash AttributeHash { get; }
    public RoadNodeId End { get; }
    public MultiLineString Geometry { get; }
    public GeometryVersion GeometryVersion { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentVersion Version { get; }
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
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes.Remove(attributeId), NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment NotPartOfNationalRoad(AttributeId attributeId)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes.Remove(attributeId), NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment NotPartOfNumberedRoad(AttributeId attributeId)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes.Remove(attributeId), LastEventHash);
    }

    public RoadSegment PartOfEuropeanRoad(RoadSegmentEuropeanRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes.SetItem(attribute.AttributeId, attribute), NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment PartOfNationalRoad(RoadSegmentNationalRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes.SetItem(attribute.AttributeId, attribute), NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment PartOfNumberedRoad(RoadSegmentNumberedRoadAttribute attribute)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes.SetItem(attribute.AttributeId, attribute), LastEventHash);
    }

    public IEnumerable<RoadNodeId> SelectOppositeNode(RoadNodeId id)
    {
        if (Start == id)
        {
            yield return End;
        }
        else if (End == id)
        {
            yield return Start;
        }
    }

    public RoadSegment WithAttributeHash(AttributeHash hash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, hash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment WithGeometry(MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return new RoadSegment(Id, Version, geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new RoadSegment(Id, Version, Geometry, geometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }

    public RoadSegment WithLastEventHash(string lastEventHash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, lastEventHash);
    }

    public RoadSegment WithStartAndEnd(RoadNodeId start, RoadNodeId end)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, start, end, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }
    
    public RoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new RoadSegment(Id, version, Geometry, GeometryVersion, Start, End, AttributeHash, EuropeanRoadAttributes, NationalRoadAttributes, NumberedRoadAttributes, LastEventHash);
    }
}
