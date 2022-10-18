namespace RoadRegistry.BackOffice.Core;

public class NationalRoadNumberNotFound : Error
{
    public NationalRoadNumberNotFound(NationalRoadNumber number)
        : base(nameof(NationalRoadNumberNotFound),
            new ProblemParameter("Number", number.ToString()))
    {
    }
}