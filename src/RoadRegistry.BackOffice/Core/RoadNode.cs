namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.RoadNode.Errors;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class RoadNode
{
    private readonly ImmutableHashSet<RoadSegmentId> _segments;

    public RoadNode(RoadNodeId id, RoadNodeVersion version, RoadNodeType type, Point geometry)
        : this(id,
            version,
            type ?? throw new ArgumentNullException(nameof(type)),
            geometry ?? throw new ArgumentNullException(nameof(geometry)),
            ImmutableHashSet<RoadSegmentId>.Empty
        )
    {
    }

    private RoadNode(RoadNodeId id, RoadNodeVersion version, RoadNodeType type, Point geometry, ImmutableHashSet<RoadSegmentId> segments)
    {
        Id = id;
        Version = version;
        Type = type;
        Geometry = geometry;
        _segments = segments;
    }

    public Point Geometry { get; }
    public RoadNodeId Id { get; }
    public RoadNodeVersion Version { get; }
    public IReadOnlyCollection<RoadSegmentId> Segments => _segments;

    public IReadOnlyCollection<RoadNodeType> SupportedRoadNodeTypes
    {
        get
        {
            if (_segments.Count == 0) return Array.Empty<RoadNodeType>();
            if (_segments.Count == 1) return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.EndNode };
            if (_segments.Count == 2) return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode };

            // 3 or more
            return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.RealNode };
        }
    }

    public RoadNodeType Type { get; }

    public RoadNode ConnectWith(RoadSegmentId segment)
    {
        return new RoadNode(Id, Version, Type, Geometry, _segments.Add(segment));
    }

    public RoadNode DisconnectFrom(RoadSegmentId segment)
    {
        return new RoadNode(Id, Version, Type, Geometry, _segments.Remove(segment));
    }

    public Problems VerifyTypeMatchesConnectedSegmentCount(IRoadNetworkView view, IRequestedChangeIdentityTranslator translator)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));

        var problems = Problems.None;

        if (Segments.Count == 0)
        {
            problems = problems.Add(new RoadNodeNotConnectedToAnySegment(translator.TranslateToTemporaryOrId(Id)));
        }
        else if (Segments.Count == 1 && Type != RoadNodeType.EndNode)
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(translator.TranslateToTemporaryOrId(Id), Segments.Select(translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, new[] { RoadNodeType.EndNode }));
        }
        else if (Segments.Count == 2)
        {
            if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
            {
                problems = problems.Add(RoadNodeTypeMismatch.New(translator.TranslateToTemporaryOrId(Id), Segments.Select(translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, new[] { RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode }));
            }
            else if (Type == RoadNodeType.FakeNode)
            {
                var segments = Segments.Select(segmentId => view.Segments[segmentId])
                    .ToArray();
                var segment1 = segments[0];
                var segment2 = segments[1];
                if (segment1.AttributeHash.Equals(segment2.AttributeHash))
                {
                    problems = problems.Add(new FakeRoadNodeConnectedSegmentsDoNotDiffer(
                        translator.TranslateToTemporaryOrId(Id),
                        translator.TranslateToOriginalOrTemporaryOrId(segment1.Id),
                        translator.TranslateToOriginalOrTemporaryOrId(segment2.Id)
                    ));
                }
            }
        }
        else if (Segments.Count > 2 && !Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(translator.TranslateToTemporaryOrId(Id), Segments.Select(translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, new[] { RoadNodeType.RealNode, RoadNodeType.MiniRoundabout }));
        }

        return problems;
    }

    public RoadNode WithVersion(RoadNodeVersion version)
    {
        return new RoadNode(Id, version, Type, Geometry, _segments);
    }

    public RoadNode WithGeometry(Point geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return new RoadNode(Id, Version, Type, geometry, _segments);
    }

    public RoadNode WithType(RoadNodeType type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return new RoadNode(Id, Version, type, Geometry, _segments);
    }
}
