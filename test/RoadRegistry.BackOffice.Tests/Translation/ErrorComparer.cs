namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    public class ErrorComparer : IEqualityComparer<Error>
    {
        public bool Equals(Error expected, Error actual)
        {
            if (expected == null && actual == null) return true;
            if (expected == null || actual == null) return false;
            return expected.Reason.Equals(actual.Reason) &&
                   expected.Parameters.SequenceEqual(actual.Parameters, new ProblemParameterValueContainsComparer());

        }

        public int GetHashCode(Error obj)
        {
            return obj.GetHashCode();
        }

        private class ProblemParameterValueContainsComparer : IEqualityComparer<ProblemParameter>
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
}
