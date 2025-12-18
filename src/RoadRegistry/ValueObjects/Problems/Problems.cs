namespace RoadRegistry.ValueObjects.Problems;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class Problems : IReadOnlyCollection<Problem>
{
    public static readonly Problems None = new(ImmutableList<Problem>.Empty, null);
    private readonly ImmutableList<Problem> _problems;

    public ProblemContext? Context { get; }

    private Problems(ImmutableList<Problem> problems, ProblemContext? context)
    {
        _problems = problems;
        Context = context;
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

    public static Problems For(RoadNodeId roadNodeId)
    {
        return None.WithContext(ProblemContext.For(roadNodeId));
    }
    public static Problems For(RoadSegmentId roadSegmentId)
    {
        return None.WithContext(ProblemContext.For(roadSegmentId));
    }
    public static Problems For(GradeSeparatedJunctionId gradeSeparatedJunctionId)
    {
        return None.WithContext(ProblemContext.For(gradeSeparatedJunctionId));
    }
    public Problems WithContext(ProblemContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new Problems(_problems.Select(x => x.WithContext(context)).ToImmutableList(), context);
    }

    public Problems Add(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return new Problems(_problems.Add(problem.WithContext(Context)), Context);
    }

    public Problems AddRange(IEnumerable<Problem> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        return new Problems(_problems.AddRange(problems.Select(x => x.WithContext(Context))), Context);
    }

    public Problems AddRange(Problems problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        return new Problems(_problems.AddRange(problems.Select(x => x.WithContext(Context))), Context);
    }

    public static Problems Many(params Problem[] problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        return None.AddRange(problems);
    }

    public static Problems Many(IEnumerable<Problem> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
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
        ArgumentNullException.ThrowIfNull(problem);
        return None.Add(problem);
    }

    public bool HasError() => _problems.OfType<Error>().Any();

    public override string ToString()
    {
        return string.Join(Environment.NewLine, _problems);
    }
}
