namespace RoadRegistry.BackOffice.Uploads;

using System.Linq;
using CommandHandling.Actions.ChangeRoadNetwork;
using ProblemParameter = ValueObjects.Problems.ProblemParameter;
using ProblemSeverity = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.ProblemSeverity;
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
