namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;

public class Error : Problem
{
    public Error(string reason, params ProblemParameter[] parameters)
        : base(reason, parameters)
    {
    }

    public Error(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        : base(reason, parameters)
    {
    }
}
