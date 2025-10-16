namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentMorphologyNotValid : Error
{
    public RoadSegmentMorphologyNotValid(RoadSegmentMorphology morphology)
        : base(ProblemCode.RoadSegment.Morphology.NotValid,
        new ProblemParameter("Morphology", morphology))
    {
    }
}
