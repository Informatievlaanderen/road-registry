namespace RoadRegistry.Extracts.Uploads;

using System.Linq;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.Messages;
using ProblemParameter = ValueObjects.Problems.ProblemParameter;

public class FileError : FileProblem
{
    public FileError(string file, string reason, params ProblemParameter[] parameters)
        : base(file, reason, parameters)
    {
    }

    public override Messages.FileProblem Translate()
    {
        return new Messages.FileProblem
        {
            File = File,
            Severity = ProblemSeverity.Error,
            Reason = Reason,
            Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
        };
    }
}
