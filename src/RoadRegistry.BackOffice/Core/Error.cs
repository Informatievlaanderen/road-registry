namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Linq;

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
        return new Messages.Problem
        {
            Severity = Messages.ProblemSeverity.Error,
            Reason = Reason,
            Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }
}
