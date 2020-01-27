namespace RoadRegistry.BackOffice.Uploads
{
    using System.Linq;
    using Core;

    public class FileError : FileProblem
    {
        public FileError(string file, string reason, params ProblemParameter[] parameters)
            : base(file, reason, parameters)
        {
        }

        public override Messages.FileProblem Translate() =>
            new Messages.FileProblem
            {
                File = File,
                Severity = Messages.ProblemSeverity.Error,
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
