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
            ImmutableHashSet<EuropeanRoadNumber>.Empty,
            ImmutableHashSet<NationalRoadNumber>.Empty,
            ImmutableHashSet<NumberedRoadNumber>.Empty,
            lastEventHash)
    {
    }

    private RoadSegment(RoadSegmentId id, RoadSegmentVersion version, MultiLineString geometry, GeometryVersion geometryVersion, RoadNodeId start, RoadNodeId end, AttributeHash attributeHash, ImmutableHashSet<EuropeanRoadNumber> partOfEuropeanRoads, ImmutableHashSet<NationalRoadNumber> partOfNationalRoads, ImmutableHashSet<NumberedRoadNumber> partOfNumberedRoads, string lastEventHash)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        
        if (attributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined.ToString() && start == end)
        {
            throw new ArgumentException("The start and end can not be the same road node.", nameof(start));
        }

        Id = id;
        Version = version;
        Geometry = geometry;
        GeometryVersion = geometryVersion;
        Start = start;
        End = end;
        AttributeHash = attributeHash;
        PartOfEuropeanRoads = partOfEuropeanRoads;
        PartOfNationalRoads = partOfNationalRoads;
        PartOfNumberedRoads = partOfNumberedRoads;
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

    public ImmutableHashSet<EuropeanRoadNumber> PartOfEuropeanRoads { get; }
    public ImmutableHashSet<NationalRoadNumber> PartOfNationalRoads { get; }
    public ImmutableHashSet<NumberedRoadNumber> PartOfNumberedRoads { get; }
    public RoadNodeId Start { get; }

    public RoadSegment NotPartOfEuropeanRoad(EuropeanRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads.Remove(number), PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment NotPartOfNationalRoad(NationalRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads.Remove(number), PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment NotPartOfNumberedRoad(NumberedRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads.Remove(number), LastEventHash);
    }

    public RoadSegment PartOfEuropeanRoad(EuropeanRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads.Add(number), PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment PartOfNationalRoad(NationalRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads.Add(number), PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment PartOfNumberedRoad(NumberedRoadNumber number)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads.Add(number), LastEventHash);
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
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, hash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment WithEnd(RoadNodeId end)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, end, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment WithGeometry(MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return new RoadSegment(Id, Version, geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new RoadSegment(Id, Version, Geometry, geometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment WithLastEventHash(string lastEventHash)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, lastEventHash);
    }

    public RoadSegment WithStart(RoadNodeId start)
    {
        return new RoadSegment(Id, Version, Geometry, GeometryVersion, start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }

    public RoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new RoadSegment(Id, version, Geometry, GeometryVersion, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads, LastEventHash);
    }
}
