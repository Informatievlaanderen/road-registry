namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Errors;
using Events.V2;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadNode
{
    public Problems VerifyTopologyAndDetectType(ScopedRoadNetworkContext context)
    {
        var problems = Problems.WithContext(context.IdTranslator.TranslateToTemporaryId(RoadNodeId));

        var segments = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.Attributes is not null && (x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId))
            .ToList();

        if (IsRemoved)
        {
            return problems;
        }

        var byOtherNode =
            context.RoadNetwork.GetNonRemovedRoadNodes().FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.Value.IsReasonablyEqualTo(Geometry.Value, context.Tolerances));
        if (byOtherNode is not null)
        {
            problems += new RoadNodeGeometryTaken(context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId));
        }

        problems += context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(s =>
                segments.All(x => x.RoadSegmentId != s.RoadSegmentId)
                && s.Geometry.Value.IsWithinDistance(Geometry.Value, Distances.RoadNodeTooClose)
            )
            .Aggregate(Problems.None, (current, segment) =>
                current.Add(new RoadNodeTooClose()
                    .WithContext(ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId)))));

        problems += ValidateTypeAndChangeIfNeeded(segments, context);

        return problems;
    }

    private Problems ValidateTypeAndChangeIfNeeded(List<RoadSegment> segments, ScopedRoadNetworkContext context)
    {
        var problems = Problems.None;

        if (segments.Count == 0)
        {
            problems += new RoadNodeNotConnectedToAnySegment();
        }
        else if (segments.Count == 1)
        {
            SetTypeIfDifferent(RoadNodeTypeV2.Eindknoop, context.Provenance);
        }
        else if (segments.Count == 2)
        {
            var segment1 = segments[0];
            var segment2 = segments[1];

            if (Grensknoop)
            {
                SetTypeIfDifferent(RoadNodeTypeV2.Validatieknoop, context.Provenance);
            }
            else
            {
                //TODO-pr logica validatieknoop
                /*
                 * indien grensknoop -> validatieknoop
                 * indien knoop een geometrische invalidatie tegenhoudt -> validatieknoop (zie unflattener voor logica)
                 * anders merge de 2 segmenten:
                 *      - nieuwe ID logica zelfde als in FC
                 *      - indien methode verschillend is, neem Ingemeten indien 75% van de lengte Ingemeten is (zie OGC overlap logica)
                 */
                problems += new RoadNodeIsNotAllowed();
            }
        }
        else
        {
            SetTypeIfDifferent(RoadNodeTypeV2.EchteKnoop, context.Provenance);
        }

        return problems;
    }

    private void SetTypeIfDifferent(RoadNodeTypeV2 type, Provenance provenance)
    {
        if (Type != type)
        {
            Apply(new RoadNodeTypeWasChanged
            {
                RoadNodeId = RoadNodeId,
                Type = type,
                Provenance = new ProvenanceData(provenance)
            });
        }
    }
}
