namespace RoadRegistry.ValueObjects.Problems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extensions;

public abstract class Problem : IEquatable<Problem>, IEqualityComparer<Problem>
{
    protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters, ProblemContext? context)
    {
        Reason = reason.ThrowIfNull();
        Parameters = parameters.ThrowIfNull();
        WithContext(context);
    }

    public IReadOnlyCollection<ProblemParameter> Parameters { get; private set; }
    public string Reason { get; }
    public ProblemContext? Context { get; private set; }

    public Problem WithContext(ProblemContext? context)
    {
        Context ??= context;
        if (context?.Parameters.Count > 0)
        {
            var parametersToAdd = new List<ProblemParameter>();

            foreach (var contextParam in context.Parameters)
            {
                var hasExactParam = Parameters.Any(p => p.Name == contextParam.Name && p.Value == contextParam.Value.ToInvariantString());
                if (hasExactParam)
                {
                    continue;
                }

                var existingParamWithDifferentValue = Parameters.SingleOrDefault(p => p.Name == contextParam.Name && p.Value != contextParam.Value.ToInvariantString());
                if (existingParamWithDifferentValue is not null)
                {
                    throw new InvalidOperationException(
                        $"Problem already contains parameter with name '{existingParamWithDifferentValue.Name}' " +
                        $"with value '{existingParamWithDifferentValue.Value}', which conflicts with context value '{contextParam.Value.ToInvariantString()}'");
                }

                parametersToAdd.Add(new ProblemParameter(contextParam.Name, contextParam.Value.ToInvariantString()));
            }

            if (parametersToAdd.Count > 0)
            {
                Parameters = Parameters.Concat(parametersToAdd).ToArray();
            }
        }
        return this;
    }

    public override bool Equals(object? obj)
    {
        return obj is Problem other && Equals(other);
    }

    public bool Equals(Problem? x, Problem? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null) return false;
        if (y is null) return false;

        if (x.GetType() != y.GetType()) return false;

        return x.Reason == y.Reason
               && Equals(x.Parameters, y.Parameters);
    }

    public virtual bool Equals(Problem? other)
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Reason}");

        if (Parameters.Any())
        {
            sb.Append($" -> {string.Join(", ", Parameters.Select(parameter => $"{parameter.Name}={parameter.Value}"))}");
        }

        return sb.ToString();
    }
}
