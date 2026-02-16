namespace RoadRegistry.Extracts.Uploads;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoadRegistry.ValueObjects.Problems;

public abstract class FileProblem : IEquatable<FileProblem>, IEqualityComparer<FileProblem>
{
    protected FileProblem(string file, string reason, IReadOnlyCollection<ProblemParameter> parameters)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public string File { get; }
    public IReadOnlyCollection<ProblemParameter> Parameters { get; }
    public string Reason { get; }

    public bool Equals(FileProblem x, FileProblem y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null) return false;

        if (y is null) return false;

        if (x.GetType() != y.GetType()) return false;

        return x.File == y.File
               && x.Reason == y.Reason
               && Equals(x.Parameters, y.Parameters);
    }

    public virtual bool Equals(FileProblem? other)
    {
        return other != null
               && GetType() == other.GetType()
               && string.Equals(File, other.File, StringComparison.InvariantCultureIgnoreCase)
               && string.Equals(Reason, other.Reason)
               && Parameters.SequenceEqual(other.Parameters);
    }

    public override bool Equals(object? obj)
    {
        return obj is FileProblem other && Equals(other);
    }

    public int GetHashCode(FileProblem obj)
    {
        return HashCode.Combine(obj.File, obj.Reason, obj.Parameters);
    }

    public override int GetHashCode()
    {
        return Parameters.Aggregate(
            File.GetHashCode() ^ Reason.GetHashCode(),
            (current, parameter) => current ^ parameter.GetHashCode());
    }

    public string Describe()
    {
        var sb = new StringBuilder();
        sb.Append($"{File}: {Reason}");

        if (Parameters.Any())
        {
            sb.Append($" -> {string.Join(", ", Parameters.Select(parameter => $"{parameter.Name}={parameter.Value}"))}");
        }

        return sb.ToString();
    }

    public abstract Messages.FileProblem Translate();

    public string GetParameterValue(string parameterName)
    {
        var value = GetOptionalParameterValue(parameterName);
        if (value is null)
        {
            throw new ArgumentException($"No parameter found with name '{parameterName}'");
        }

        return value;
    }
    public string? GetOptionalParameterValue(string parameterName)
    {
        return Parameters
            .SingleOrDefault(x => string.Equals(x.Name, parameterName, StringComparison.InvariantCultureIgnoreCase))
            ?.Value;
    }
}
