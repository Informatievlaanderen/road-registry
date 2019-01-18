namespace RoadRegistry.BackOffice.Model
{
    public class Warning : Problem
    {
        public Warning(string reason, params ProblemParameter[] parameters)
            : base(reason, parameters)
        {
        }
    }
}
