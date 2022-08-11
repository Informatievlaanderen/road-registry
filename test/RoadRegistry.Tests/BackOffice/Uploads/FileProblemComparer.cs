namespace RoadRegistry.BackOffice.Uploads;

using System.Collections.Generic;
using System.Linq;
using Core;

public class FileProblemComparer : IEqualityComparer<FileProblem>
{
    public bool Equals(FileProblem expected, FileProblem actual)
    {
        if (expected == null && actual == null) return true;
        if (expected == null || actual == null) return false;
        return expected.GetType() == actual.GetType() &&
               expected.File.Equals(actual.File) &&
               expected.Reason.Equals(actual.Reason) &&
               expected.Parameters.SequenceEqual(actual.Parameters, new ProblemParameterValueContainsComparer());
    }

    public int GetHashCode(FileProblem obj)
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
