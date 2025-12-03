namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNetwork
{
    public void RemoveRoadSegments(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, Provenance provenance)
    {
        var roadSegmentsProblems = new Dictionary<RoadSegmentId, Problems>();

        var invalidCategories = new[]
        {
            RoadSegmentCategory.EuropeanMainRoad,
            RoadSegmentCategory.FlemishMainRoad,
            RoadSegmentCategory.MainRoad,
            RoadSegmentCategory.PrimaryRoadI,
            RoadSegmentCategory.PrimaryRoadII
        };

        foreach (var roadSegmentId in roadSegmentIds)
        {
            if (!_roadSegments.TryGetValue(roadSegmentId, out var segment))
            {
                roadSegmentsProblems.Add(roadSegmentId, Problems.Single(new RoadSegmentNotFound(roadSegmentId)));
                continue;
            }

            var segmentCategories = segment.Attributes.Category.Values.Select(x => x.Value).ToArray();
            var segmentInvalidCategories = invalidCategories.Intersect(segmentCategories).ToArray();
            if (segmentInvalidCategories.Any())
            {
                var problems = Problems.None;

                foreach (var category in segmentInvalidCategories)
                {
                    problems += new RoadSegmentNotRemovedBecauseCategoryIsInvalid(roadSegmentId, category);
                }

                roadSegmentsProblems.Add(roadSegmentId, problems);
                continue;
            }

            {
                var problems = segment.Remove(provenance);
                if (problems.HasError())
                {
                    roadSegmentsProblems.Add(roadSegmentId, problems);
                }
            }
        }

        if (roadSegmentsProblems.Any())
        {
            throw new RoadSegmentsProblemsException(roadSegmentsProblems);
        }
    }
}
