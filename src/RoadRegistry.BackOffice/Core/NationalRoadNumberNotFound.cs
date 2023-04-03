namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class NationalRoadNumberNotFound : Error
{
    public NationalRoadNumberNotFound(NationalRoadNumber number)
        : base(ProblemCode.NationalRoad.NumberNotFound,
            new ProblemParameter("Number", number.ToString()))
    {
    }
}
