namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Model;

    public class FileWarning : FileProblem
    {
        public FileWarning(string file, string reason, params ProblemParameter[] parameters)
            : base(file, reason, parameters)
        {
        }

        public override Messages.FileProblem Translate() =>
            new Messages.FileProblem
            {
                File = File,
                Severity = Messages.ProblemSeverity.Warning,
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
