namespace RoadRegistry.BackOffice.Core
{
    using System.Linq;
    using Messages;

    public class Error : Problem
    {
        public Error(string reason, params ProblemParameter[] parameters)
            : base(reason, parameters)
        {
        }

        public override Messages.Problem Translate() =>
            new Messages.Problem
            {
                Severity = ProblemSeverity.Error,
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
