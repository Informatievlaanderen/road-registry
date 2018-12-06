namespace RoadRegistry.Model
{
    public class Warning : Reason
    {
        public Warning(string because, params ReasonParameter[] parameters)
            : base(because, parameters)
        {
        }
    }
}
