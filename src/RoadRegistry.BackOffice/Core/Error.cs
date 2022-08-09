namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Linq;
using Messages;

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

    public override Messages.Problem Translate()
    {
        return new()
        {
            Severity = ProblemSeverity.Error,
            Reason = Reason,
            Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }
}
