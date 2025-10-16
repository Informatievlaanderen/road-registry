namespace RoadRegistry.BackOffice.Uploads;

using System.Linq;
using ProblemParameter = Core.ProblemParameter;
using ProblemSeverity = Messages.ProblemSeverity;
using RoadRegistry.BackOffice.Core;

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
