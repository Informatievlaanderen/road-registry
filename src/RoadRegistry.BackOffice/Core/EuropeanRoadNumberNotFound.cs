namespace RoadRegistry.BackOffice.Core;

public class EuropeanRoadNumberNotFound : Error
{
    public EuropeanRoadNumberNotFound(EuropeanRoadNumber number)
        : base(nameof(EuropeanRoadNumberNotFound),
            new ProblemParameter("Number", number.ToString()))
    {
    }
}