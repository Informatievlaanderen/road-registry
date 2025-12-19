namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;

public class Warning : Problem
{
    public Warning(string reason, params ProblemParameter[] parameters)
        : base(reason, parameters, null)
    {
    }
    public Warning(ProblemContext context, string reason, params ProblemParameter[] parameters)
        : base(reason, parameters, context)
    {
    }

    public Warning(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters, null)
    {
    }
    public Warning(ProblemContext context, string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters, context)
    {
    }
}
