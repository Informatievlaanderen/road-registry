namespace RoadRegistry.RoadSegment;

using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public class RoadSegmentAttributesValidator
{
    public Problems Validate(RoadSegmentAttributes attributes, double geometryLength)
    {
        var problems = Problems.None;

        problems += attributes.AccessRestriction.Validate(geometryLength, ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes);
        problems += attributes.Category.Validate(geometryLength, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes);
        problems += attributes.Morphology.Validate(geometryLength, ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes);
        problems += attributes.StreetNameId.Validate(geometryLength, ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes);
        problems += attributes.MaintenanceAuthorityId.Validate(geometryLength, ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes);
        problems += attributes.SurfaceType.Validate(geometryLength, ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes);
        problems += attributes.CarAccessForward.Validate(geometryLength, ProblemCode.RoadSegment.CarAccessForward.DynamicAttributeProblemCodes);
        problems += attributes.CarAccessBackward.Validate(geometryLength, ProblemCode.RoadSegment.CarAccessBackward.DynamicAttributeProblemCodes);
        problems += attributes.BikeAccessForward.Validate(geometryLength, ProblemCode.RoadSegment.BikeAccessForward.DynamicAttributeProblemCodes);
        problems += attributes.BikeAccessBackward.Validate(geometryLength, ProblemCode.RoadSegment.BikeAccessBackward.DynamicAttributeProblemCodes);
        problems += attributes.PedestrianAccess.Validate(geometryLength, ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes);
        problems += attributes.EuropeanRoadNumbers.ValidateCollectionMustBeUnique(ProblemCode.RoadSegment.EuropeanRoads.NotUnique);
        problems += attributes.NationalRoadNumbers.ValidateCollectionMustBeUnique(ProblemCode.RoadSegment.NationalRoads.NotUnique);

        return problems;
    }
}
