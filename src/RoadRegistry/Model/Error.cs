namespace RoadRegistry.Model
{
    public class Error : Reason
    {
        public Error(string because, params ReasonParameter[] parameters)
            : base(because, parameters)
        {
        }
    }
}
