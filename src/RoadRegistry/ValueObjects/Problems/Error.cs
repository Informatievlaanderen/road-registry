namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;

public class Error : Problem
{
    public Error(string reason, params ProblemParameter[] parameters)
        : base(reason, parameters, null)
    {
    }
    public Error(ProblemContext context, string reason, params ProblemParameter[] parameters)
        : base(reason, parameters, context)
    {
    }

    public Error(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters, null)
    {
    }

    public Error(ProblemContext context, string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters, context)
    {
    }
}
