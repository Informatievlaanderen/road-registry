namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Problem
    {
        protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Reason { get; }

        public IReadOnlyCollection<ProblemParameter> Parameters { get; }

        public bool Equals(Problem other) => other != null
                                                      && string.Equals(Reason, other.Reason)
                                                      && Parameters.SequenceEqual(other.Parameters);
        public override bool Equals(object obj) => obj is Problem other && Equals(other);
        public override int GetHashCode() => Parameters.Aggregate(
            Reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());

        public Messages.Problem Translate() =>
            new Messages.Problem
            {
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
