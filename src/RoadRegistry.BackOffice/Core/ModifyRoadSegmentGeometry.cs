namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using NetTopologySuite.Geometries;

public class ModifyRoadSegmentGeometry : IRequestedChange, IHaveHash
{
    public const string EventName = "ModifyRoadSegmentGeometry";

    public ModifyRoadSegmentGeometry(
        RoadSegmentId id,
        RoadSegmentVersion version,
        GeometryVersion geometryVersion,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        MultiLineString geometry,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyList<RoadSegmentWidthAttribute> widths)
    {
        Id = id;
        Version = version;
        GeometryVersion = geometryVersion;
        GeometryDrawMethod = geometryDrawMethod;
        Geometry = geometry.ThrowIfNull();
        Lanes = lanes.ThrowIfNull();
        Surfaces = surfaces.ThrowIfNull();
        Widths = widths.ThrowIfNull();
    }

    public RoadSegmentId Id { get; }
    public RoadSegmentVersion Version { get; }
    public GeometryVersion GeometryVersion { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public MultiLineString Geometry { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentGeometryModified = new RoadSegmentGeometryModified
            {
                Id = Id,
                Version = Version,
                Geometry = GeometryTranslator.Translate(Geometry),
                GeometryVersion = GeometryVersion.ToInt32(),
                Lanes = Lanes
                    .Select(item => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = GeometryVersion.Initial,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = GeometryVersion.Initial,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = GeometryVersion.Initial,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            }
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        message.ModifyRoadSegmentGeometry = new Messages.ModifyRoadSegmentGeometry
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod,
            Geometry = GeometryTranslator.Translate(Geometry),
            Lanes = Lanes
                .Select(item => new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = item.TemporaryId,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces
                .Select(item => new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = item.TemporaryId,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray()
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Problems.None;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        if (!context.BeforeView.Segments.ContainsKey(Id))
        {
            problems += new RoadSegmentNotFound();
        }

        var line = Geometry.GetSingleLineString();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(Id, context.Tolerances);
        }
        else
        {
            problems += line.GetProblemsForRoadSegmentGeometry(Id, context.Tolerances);
        }

        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
