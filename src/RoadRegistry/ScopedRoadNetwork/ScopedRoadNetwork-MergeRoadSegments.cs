namespace RoadRegistry.ScopedRoadNetwork;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    public (RoadSegment?, Problems) MergeRoadSegments(
        RoadSegment segment1, RoadSegment segment2,
        IRoadNetworkIdGenerator idGenerator,
        ScopedRoadNetworkContext context)
    {
        var commonNodeId = segment1.GetCommonNode(segment2)!.Value;
        var startNodeId = segment1.GetOppositeNode(commonNodeId)!.Value;
        var endNodeId = segment2.GetOppositeNode(commonNodeId)!.Value;

        var segment1HasIdealDirection = startNodeId == segment1.StartNodeId;
        var segment2HasIdealDirection = endNodeId == segment2.EndNodeId;

        var geometry = RoadSegmentGeometryHelper.MergeGeometries(segment1, segment2, commonNodeId, context);

        var method = RoadSegmentGeometryHelper.DetermineMethod([
            (segment1.Geometry.Value, segment1.Attributes!.GeometryDrawMethod),
            (segment2.Geometry.Value, segment2.Attributes!.GeometryDrawMethod),
        ], geometry)!;

        //TODO-pr niet altijd een nieuwe ID aanmaken, enkel indien de verandering te groot is (zie FC logica maar dan enkel op lengte)
        //RoadSegmentConstants.MinimumPercentageToKeepIdentifier

        var mergedSegment = new MergeRoadSegmentChange
        {
            TemporaryId = _roadSegments.Keys.Max().Next(),
            OriginalIds = [segment1.RoadSegmentId, segment2.RoadSegmentId],
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = method,
            Status = segment1.Attributes.Status,
            AccessRestriction = segment1.Attributes.AccessRestriction.MergeWith(segment2.Attributes!.AccessRestriction, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Category = segment1.Attributes.Category.MergeWith(segment2.Attributes.Category, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Morphology = segment1.Attributes.Morphology.MergeWith(segment2.Attributes.Morphology, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            StreetNameId = segment1.Attributes.StreetNameId.MergeWith(segment2.Attributes.StreetNameId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            MaintenanceAuthorityId = segment1.Attributes.MaintenanceAuthorityId.MergeWith(segment2.Attributes.MaintenanceAuthorityId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            SurfaceType = segment1.Attributes.SurfaceType.MergeWith(segment2.Attributes.SurfaceType, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            CarAccessForward = segment1.Attributes.CarAccessForward.MergeWith(segment2.Attributes.CarAccessForward, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            CarAccessBackward = segment1.Attributes.CarAccessBackward.MergeWith(segment2.Attributes.CarAccessBackward, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            BikeAccessForward = segment1.Attributes.BikeAccessForward.MergeWith(segment2.Attributes.BikeAccessForward, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            BikeAccessBackward = segment1.Attributes.BikeAccessBackward.MergeWith(segment2.Attributes.BikeAccessBackward, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            PedestrianAccess = segment1.Attributes.PedestrianAccess.MergeWith(segment2.Attributes.PedestrianAccess, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            EuropeanRoadNumbers = segment1.Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = segment1.Attributes.NationalRoadNumbers
        };

        var (roadSegment, problems) = RoadSegment.Merge(mergedSegment, idGenerator, context);
        if (problems.HasError())
        {
            return (null, problems);
        }

        problems += context.IdTranslator.RegisterMapping(new RoadSegmentIdReference(mergedSegment.TemporaryId), roadSegment!.RoadSegmentId);
        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);

        problems += segment1.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
        problems += segment2.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
        problems += _roadNodes[commonNodeId].Remove(context.Provenance);

        var nodeSegmentIds = new[] { segment1.RoadSegmentId, segment2.RoadSegmentId };
        var connectedJunctions = _gradeSeparatedJunctions
            .Where(x => nodeSegmentIds.Any(segmentId => x.Value.IsConnectedTo(segmentId)))
            .Select(x => x.Value)
            .ToArray();

        foreach (var junction in connectedJunctions)
        {
            var modify = new ModifyGradeSeparatedJunctionChange
            {
                GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
            };

            if (nodeSegmentIds.Contains(junction.LowerRoadSegmentId))
            {
                modify = modify with
                {
                    LowerRoadSegmentId = mergedSegment.TemporaryId
                };
            }

            if (nodeSegmentIds.Contains(junction.UpperRoadSegmentId))
            {
                modify = modify with
                {
                    UpperRoadSegmentId = mergedSegment.TemporaryId
                };
            }

            problems += junction.Modify(modify, context.Provenance);
        }

        return (roadSegment, problems);
    }
}
