namespace RoadRegistry.BackOffice.Model
{
    public class Error : Problem
    {
        public Error(string reason, params ProblemParameter[] parameters)
            : base(reason, parameters)
        {
        }
    }
}
