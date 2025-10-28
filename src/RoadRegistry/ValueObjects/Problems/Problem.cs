namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Problem : IEquatable<Problem>, IEqualityComparer<Problem>
{
    protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters)
    {
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public IReadOnlyCollection<ProblemParameter> Parameters { get; }
    public string Reason { get; }

    public override bool Equals(object obj)
    {
        return obj is Problem other && Equals(other);
    }

    public bool Equals(Problem x, Problem y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null) return false;

        if (y is null) return false;

        if (x.GetType() != y.GetType()) return false;

        return x.Reason == y.Reason
               && Equals(x.Parameters, y.Parameters);
    }

    public virtual bool Equals(Problem other)
    {
        return other != null
               && string.Equals(Reason, other.Reason)
               && Parameters.SequenceEqual(other.Parameters);
    }

    public override int GetHashCode()
    {
        return Parameters.Aggregate(
            Reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());
    }

    public int GetHashCode(Problem obj)
    {
        return HashCode.Combine(obj.Reason, obj.Parameters);
    }
}
