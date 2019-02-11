namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Problem
    {
        private readonly string _reason;
        private readonly IReadOnlyCollection<ProblemParameter> _parameters;

        protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            _reason = reason ?? throw new ArgumentNullException(nameof(reason));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public bool Equals(Problem other) => other != null
                                                      && string.Equals(_reason, other._reason)
                                                      && _parameters.SequenceEqual(other._parameters);

        public override bool Equals(object obj) => obj is Problem other && Equals(other);
        public override int GetHashCode() => _parameters.Aggregate(
            _reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());

        public Messages.Problem Translate() =>
            new Messages.Problem
            {
                Reason = _reason,
                Parameters = _parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
