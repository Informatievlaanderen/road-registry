namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Linq;
using Messages;

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

    public override Messages.Problem Translate()
    {
        return new()
        {
            Severity = ProblemSeverity.Warning,
            Reason = Reason,
            Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }
}
