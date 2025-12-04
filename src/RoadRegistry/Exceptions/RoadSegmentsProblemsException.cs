namespace RoadRegistry.BackOffice.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects.Problems;

public class RoadSegmentsProblemsException : RoadRegistryException
{
    public IDictionary<RoadSegmentId, Problems> RoadSegmentsProblems { get; }

    public RoadSegmentsProblemsException(IDictionary<RoadSegmentId, Problems> roadSegmentsProblems)
    {
        ArgumentNullException.ThrowIfNull(roadSegmentsProblems);
        if (!roadSegmentsProblems.Any())
        {
            throw new ArgumentException("At least 1 problem is required", nameof(roadSegmentsProblems));
        }

        RoadSegmentsProblems = roadSegmentsProblems;
    }
}
