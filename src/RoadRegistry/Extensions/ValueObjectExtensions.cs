namespace RoadRegistry.Extensions;

using System.Collections.Generic;
using RoadRegistry.ValueObjects.Problems;

public static class ValueObjectExtensions
{
    public static IEnumerable<ProblemParameter> ToRoadSegmentProblemParameters(this RoadSegmentIdReference idReference, string namePrefix = "Wegsegment")
    {
        yield return new ProblemParameter(
            $"{namePrefix}Id",
            idReference.RoadSegmentId.ToInt32().ToString());

        if (idReference.TempIds is not null)
        {
            yield return new ProblemParameter(
                $"{namePrefix}TempIds",
                idReference.GetTempIdsAsString());
        }
    }
}
