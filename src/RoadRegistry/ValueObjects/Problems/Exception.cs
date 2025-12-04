namespace RoadRegistry.ValueObjects.Problems;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class Problems : IReadOnlyCollection<Problem>
{
    public static readonly Problems None = new(ImmutableList<Problem>.Empty);
    private readonly ImmutableList<Problem> _problems;

    private Problems(ImmutableList<Problem> problems)
    {
        _problems = problems;
    }

    public int Count => _problems.Count;

    public IEnumerator<Problem> GetEnumerator()
    {
        return _problems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Problems Add(Problem problem)
    {
        if (problem == null) throw new ArgumentNullException(nameof(problem));
        return new Problems(_problems.Add(problem));
    }

    public Problems AddRange(IEnumerable<Problem> problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));
        return new Problems(_problems.AddRange(problems));
    }

    public Problems AddRange(Problems problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));
        return new Problems(_problems.AddRange(problems));
    }

    public static Problems Many(params Problem[] problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));

        return None.AddRange(problems);
    }

    public static Problems Many(IEnumerable<Problem> problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));

        return None.AddRange(problems);
    }

    public static Problems operator +(Problems left, Problem right)
    {
        return left.Add(right);
    }

    public static Problems operator +(Problems left, IEnumerable<Problem> right)
    {
        return left.AddRange(right);
    }

    public static Problems operator +(Problems left, Problems right)
    {
        return left.AddRange(right);
    }

    public static Problems Single(Problem problem)
    {
        if (problem == null) throw new ArgumentNullException(nameof(problem));

        return None.Add(problem);
    }

    public bool HasError() => _problems.OfType<Error>().Any();

    public override string ToString()
    {
        return string.Join(Environment.NewLine, _problems);
    }
}
