namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class NumberedRoadNumberNotFound : Error
{
    public NumberedRoadNumberNotFound(NumberedRoadNumber number) 
        : base(ProblemCode.NumberedRoad.NumberNotFound,
            new ProblemParameter("Number", number.ToString()))
    {
    }
}
