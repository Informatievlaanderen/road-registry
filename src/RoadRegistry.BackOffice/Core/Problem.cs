namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Problem : IEquatable<Problem>, IEqualityComparer<Problem>
    {
        protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Reason { get; }

        public IReadOnlyCollection<ProblemParameter> Parameters { get; }

        public virtual bool Equals(Problem other) => other != null
            && string.Equals(Reason, other.Reason)
            && Parameters.SequenceEqual(other.Parameters);

        public override bool Equals(object obj) => obj is Problem other && Equals(other);

        public override int GetHashCode() => Parameters.Aggregate(
            Reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());

        public abstract Messages.Problem Translate();

        public bool Equals(Problem x, Problem y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Reason == y.Reason
                && Equals(x.Parameters, y.Parameters);
        }

        public int GetHashCode(Problem obj)
        {
            return HashCode.Combine(obj.Reason, obj.Parameters);
        }
    }
}
