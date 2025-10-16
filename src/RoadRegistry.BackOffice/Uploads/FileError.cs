namespace RoadRegistry.BackOffice.Uploads;

using System.Linq;
using ProblemParameter = Core.ProblemParameter;
using ProblemSeverity = Messages.ProblemSeverity;
using RoadRegistry.BackOffice.Core;

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
