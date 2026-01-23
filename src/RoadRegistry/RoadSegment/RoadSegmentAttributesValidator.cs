namespace RoadRegistry.RoadSegment;

using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public class RoadSegmentAttributesValidator
{
    public Problems Validate(RoadSegmentId roadSegmentId, RoadSegmentAttributes attributes, double geometryLength)
    {
        var problems = Problems.None;

        problems += attributes.AccessRestriction.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes);
        problems += attributes.Category.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes);
        problems += attributes.Morphology.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes);
        problems += attributes.Status.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes);
        problems += attributes.StreetNameId.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes);
        problems += attributes.MaintenanceAuthorityId.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes);
        problems += attributes.SurfaceType.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes);
        problems += attributes.CarAccess.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.CarAccess.DynamicAttributeProblemCodes);
        problems += attributes.BikeAccess.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.BikeAccess.DynamicAttributeProblemCodes);
        problems += attributes.PedestrianAccess.Validate(roadSegmentId, geometryLength, ProblemCode.RoadSegment.PedestrianAccess.DynamicAttributeProblemCodes);
        problems += attributes.EuropeanRoadNumbers.ValidateCollectionMustBeUnique(roadSegmentId, ProblemCode.RoadSegment.EuropeanRoads.NotUnique);
        problems += attributes.NationalRoadNumbers.ValidateCollectionMustBeUnique(roadSegmentId, ProblemCode.RoadSegment.NationalRoads.NotUnique);

        return problems;
    }
}
