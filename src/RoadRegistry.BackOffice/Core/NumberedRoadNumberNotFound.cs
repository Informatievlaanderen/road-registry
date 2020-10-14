namespace RoadRegistry.BackOffice.Core
{
    public class NumberedRoadNumberNotFound : Error
    {
        public NumberedRoadNumberNotFound(NumberedRoadNumber number)
            : base(nameof(NumberedRoadNumberNotFound),
                new ProblemParameter("Number", number.ToString()))
        {
        }
    }
}