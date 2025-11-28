namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;

public class Warning : Problem
{
    public Warning(string reason, params ProblemParameter[] parameters)
        : base(reason, parameters)
    {
    }

    public Warning(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters)
    {
    }
}
