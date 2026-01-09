namespace RoadRegistry.Extracts.Uploads;

using System.Linq;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.Messages;
using ProblemParameter = ValueObjects.Problems.ProblemParameter;

public class FileWarning : FileProblem
{
    public FileWarning(string file, string reason, params ProblemParameter[] parameters)
        : base(file, reason, parameters)
    {
    }

    public override Messages.FileProblem Translate()
    {
        return new Messages.FileProblem
        {
            File = File,
            Severity = ProblemSeverity.Warning,
            Reason = Reason,
            Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }
}
