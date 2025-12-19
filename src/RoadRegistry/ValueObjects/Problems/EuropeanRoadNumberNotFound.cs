namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class EuropeanRoadNumberNotFound : Error
{
    public EuropeanRoadNumberNotFound(EuropeanRoadNumber number)
        : base(ProblemCode.EuropeanRoad.NumberNotFound,
            new ProblemParameter("Number", number.ToString()))
    {
    }
}
