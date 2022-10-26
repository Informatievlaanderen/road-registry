namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NetTopologySuite.Geometries;

public class RoadSegment
{
    public RoadSegment(RoadSegmentId id, MultiLineString geometry, RoadNodeId start, RoadNodeId end, AttributeHash attributeHash)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        if (start == end)
            throw new ArgumentException("The start and end can not be the same road node.", nameof(start));

        Id = id;
        Geometry = geometry;
        Start = start;
        End = end;
        AttributeHash = attributeHash;
        PartOfEuropeanRoads = ImmutableHashSet<EuropeanRoadNumber>.Empty;
        PartOfNationalRoads = ImmutableHashSet<NationalRoadNumber>.Empty;
        PartOfNumberedRoads = ImmutableHashSet<NumberedRoadNumber>.Empty;
    }

    private RoadSegment(RoadSegmentId id, MultiLineString geometry, RoadNodeId start, RoadNodeId end, AttributeHash attributeHash, ImmutableHashSet<EuropeanRoadNumber> partOfEuropeanRoads, ImmutableHashSet<NationalRoadNumber> partOfNationalRoads, ImmutableHashSet<NumberedRoadNumber> partOfNumberedRoads)
    {
        Id = id;
        Geometry = geometry;
        Start = start;
        End = end;
        AttributeHash = attributeHash;
        PartOfEuropeanRoads = partOfEuropeanRoads;
        PartOfNationalRoads = partOfNationalRoads;
        PartOfNumberedRoads = partOfNumberedRoads;
    }

    public AttributeHash AttributeHash { get; }
    public RoadNodeId End { get; }
    public MultiLineString Geometry { get; }
    public RoadSegmentId Id { get; }

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
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads.Remove(number), PartOfNationalRoads, PartOfNumberedRoads);
    }

    public RoadSegment NotPartOfNationalRoad(NationalRoadNumber number)
    {
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads.Remove(number), PartOfNumberedRoads);
    }

    public RoadSegment NotPartOfNumberedRoad(NumberedRoadNumber number)
    {
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads.Remove(number));
    }

    public RoadSegment PartOfEuropeanRoad(EuropeanRoadNumber number)
    {
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads.Add(number), PartOfNationalRoads, PartOfNumberedRoads);
    }

    public RoadSegment PartOfNationalRoad(NationalRoadNumber number)
    {
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads.Add(number), PartOfNumberedRoads);
    }

    public RoadSegment PartOfNumberedRoad(NumberedRoadNumber number)
    {
        return new RoadSegment(Id, Geometry, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads.Add(number));
    }

    public IEnumerable<RoadNodeId> SelectOppositeNode(RoadNodeId id)
    {
        if (Start == id)
            yield return End;
        else if (End == id) yield return Start;
    }

    public RoadSegment WithAttributeHash(AttributeHash hash)
    {
        return new RoadSegment(Id, Geometry, Start, End, hash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads);
    }

    public RoadSegment WithEnd(RoadNodeId end)
    {
        return new RoadSegment(Id, Geometry, Start, end, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads);
    }

    public RoadSegment WithGeometry(MultiLineString geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new RoadSegment(Id, geometry, Start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads);
    }

    public RoadSegment WithStart(RoadNodeId start)
    {
        return new RoadSegment(Id, Geometry, start, End, AttributeHash, PartOfEuropeanRoads, PartOfNationalRoads, PartOfNumberedRoads);
    }
}