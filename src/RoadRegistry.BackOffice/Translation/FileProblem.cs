namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    public abstract class FileProblem
    {
        protected FileProblem(string file, string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string File { get; }
        public string Reason { get; }

        public IReadOnlyCollection<ProblemParameter> Parameters { get; }

        public bool Equals(FileProblem other) => other != null
                                             && string.Equals(File, other.File, StringComparison.InvariantCultureIgnoreCase)
                                             && string.Equals(Reason, other.Reason)
                                             && Parameters.SequenceEqual(other.Parameters);
        public override bool Equals(object obj) => obj is FileProblem other && Equals(other);
        public override int GetHashCode() => Parameters.Aggregate(
            File.GetHashCode() ^ Reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());

        public Messages.FileProblem Translate() =>
            new Messages.FileProblem
            {
                File = File,
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
