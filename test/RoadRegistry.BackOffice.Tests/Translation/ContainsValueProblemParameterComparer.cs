namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using Model;

    public class ContainsValueProblemParameterComparer : IEqualityComparer<ProblemParameter>
    {
        public bool Equals(ProblemParameter expected, ProblemParameter actual)
        {
            if (expected == null && actual == null) return true;
            if (expected == null || actual == null) return false;
            return expected.Name.Equals(actual.Name) &&
                   actual.Value.Contains(expected.Value);
        }

        public int GetHashCode(ProblemParameter obj)
        {
            return obj.GetHashCode();
        }
    }
}