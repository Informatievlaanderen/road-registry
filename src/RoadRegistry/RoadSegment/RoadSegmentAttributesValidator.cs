namespace RoadRegistry.RoadSegment;

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
        problems += attributes.CarTrafficDirection.Validate(geometryLength, ProblemCode.RoadSegment.CarTrafficDirection.DynamicAttributeProblemCodes);
        problems += attributes.BikeTrafficDirection.Validate(geometryLength, ProblemCode.RoadSegment.BikeTrafficDirection.DynamicAttributeProblemCodes);
        problems += attributes.PedestrianTrafficDirection.Validate(geometryLength, ProblemCode.RoadSegment.PedestrianTrafficDirection.DynamicAttributeProblemCodes);
        problems += attributes.EuropeanRoadNumbers.ValidateCollectionMustBeUnique(ProblemCode.RoadSegment.EuropeanRoads.NotUnique);
        problems += attributes.NationalRoadNumbers.ValidateCollectionMustBeUnique(ProblemCode.RoadSegment.NationalRoads.NotUnique);

        return problems;
    }
}
