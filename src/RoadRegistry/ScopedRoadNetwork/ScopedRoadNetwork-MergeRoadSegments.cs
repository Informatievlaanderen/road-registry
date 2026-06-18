namespace RoadRegistry.ScopedRoadNetwork;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    public Problems MergeRoadSegments(
        RoadSegment segment1, RoadSegment segment2,
        IRoadNetworkIdGenerator idGenerator,
        ScopedRoadNetworkChangeContext context)
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

        var segmentWithLongestLength = segment1.Geometry.Value.Length > segment2.Geometry.Value.Length ? segment1 : segment2;
        var overlapPercentage = segmentWithLongestLength.Geometry.Value.Length / geometry.Length;
        var modifyLongestSegment = overlapPercentage >= RoadSegmentConstants.MinimumPercentageToKeepIdentifier;

        var mergeSegmentChange = new MergeRoadSegmentChange
        {
            RoadSegmentId = segmentWithLongestLength.RoadSegmentId,
            OtherRoadSegmentId = segmentWithLongestLength.RoadSegmentId == segment1.RoadSegmentId ? segment2.RoadSegmentId : segment1.RoadSegmentId,
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = method,
            Status = segment1.Status,
            AccessRestriction = segment1.Attributes.AccessRestriction.MergeWith(segment2.Attributes!.AccessRestriction, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Category = segment1.Attributes.Category.MergeWith(segment2.Attributes.Category, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Morphology = segment1.Attributes.Morphology.MergeWith(segment2.Attributes.Morphology, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            StreetNameId = segment1.Attributes.StreetNameId.MergeWith(segment2.Attributes.StreetNameId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            MaintenanceAuthorityId = segment1.Attributes.MaintenanceAuthorityId.MergeWith(segment2.Attributes.MaintenanceAuthorityId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            SurfaceType = segment1.Attributes.SurfaceType.MergeWith(segment2.Attributes.SurfaceType, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            CarTrafficDirection = segment1.Attributes.CarTrafficDirection.MergeWith(segment2.Attributes.CarTrafficDirection, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            BikeTrafficDirection = segment1.Attributes.BikeTrafficDirection.MergeWith(segment2.Attributes.BikeTrafficDirection, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            PedestrianTrafficDirection = segment1.Attributes.PedestrianTrafficDirection.MergeWith(segment2.Attributes.PedestrianTrafficDirection, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            EuropeanRoadNumbers = segment1.Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = segment1.Attributes.NationalRoadNumbers
        };

        Problems problems;
        RoadSegment roadSegment;

        if (modifyLongestSegment)
        {
            roadSegment = segmentWithLongestLength;
            problems = Problems.None;
        }
        else
        {
            problems = AddRoadSegment(new AddRoadSegmentChange
            {
                RoadSegmentIdReference = context.IdTranslator.TranslateToTemporaryId(_roadSegments.Keys.Max().Next()),
                Geometry = mergeSegmentChange.Geometry,
                GeometryDrawMethod = mergeSegmentChange.GeometryDrawMethod,
                Status = mergeSegmentChange.Status,
                AccessRestriction = mergeSegmentChange.AccessRestriction,
                Category = mergeSegmentChange.Category,
                Morphology = mergeSegmentChange.Morphology,
                StreetNameId = mergeSegmentChange.StreetNameId,
                MaintenanceAuthorityId = mergeSegmentChange.MaintenanceAuthorityId,
                SurfaceType = mergeSegmentChange.SurfaceType,
                CarTrafficDirection = mergeSegmentChange.CarTrafficDirection,
                BikeTrafficDirection = mergeSegmentChange.BikeTrafficDirection,
                PedestrianTrafficDirection = mergeSegmentChange.PedestrianTrafficDirection,
                EuropeanRoadNumbers = mergeSegmentChange.EuropeanRoadNumbers,
                NationalRoadNumbers = mergeSegmentChange.NationalRoadNumbers
            }, idGenerator, context);
            if (problems.HasError())
            {
                return problems;
            }

            var addedRoadSegmentId = context.Summary.RoadSegments.Added.Last();
            roadSegment = _roadSegments[addedRoadSegmentId];
            mergeSegmentChange = mergeSegmentChange with
            {
                RoadSegmentId = roadSegment.RoadSegmentId
            };
        }

        if (segment1.RoadSegmentId != roadSegment.RoadSegmentId)
        {
            problems += segment1.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
            _roadSegmentsSpatialIndex.Remove(segment1.Geometry.Value.EnvelopeInternal, segment1);
            context.Summary.RoadSegments.Removed.Add(segment1.RoadSegmentId);
        }

        if (segment2.RoadSegmentId != roadSegment.RoadSegmentId)
        {
            problems += segment2.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
            _roadSegmentsSpatialIndex.Remove(segment2.Geometry.Value.EnvelopeInternal, segment2);
            context.Summary.RoadSegments.Removed.Add(segment2.RoadSegmentId);
        }

        var oldEnvelope = roadSegment.Geometry.Value.EnvelopeInternal;
        problems += roadSegment.Merge(mergeSegmentChange, context);
        _roadSegmentsSpatialIndex.Update(oldEnvelope, roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        context.Summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);

        problems += RemoveRoadNode(new RemoveRoadNodeChange
        {
            RoadNodeId = commonNodeId
        }, context);

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
                    LowerRoadSegmentId = mergeSegmentChange.RoadSegmentId
                };
            }

            if (nodeSegmentIds.Contains(junction.UpperRoadSegmentId))
            {
                modify = modify with
                {
                    UpperRoadSegmentId = mergeSegmentChange.RoadSegmentId
                };
            }

            problems += junction.Modify(modify, context.Provenance);
            context.Summary.GradeSeparatedJunctions.Modified.Add(junction.GradeSeparatedJunctionId);
        }

        return problems;
    }
}
