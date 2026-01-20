namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentMorphologyNotValid : Error
{
    public RoadSegmentMorphologyNotValid(RoadSegmentMorphology morphology)
        : base(ProblemCode.RoadSegment.Morphology.NotValid,
        new ProblemParameter("Morphology", morphology))
    {
    }

    public RoadSegmentMorphologyNotValid(RoadSegmentMorphologyV2 morphology)
        : base(ProblemCode.RoadSegment.Morphology.NotValid,
        new ProblemParameter("Morphology", morphology))
    {
    }
}
