namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Exceptions;

public sealed class ZipArchiveProblems : IReadOnlyCollection<FileProblem>, IEquatable<ZipArchiveProblems>
{
    public static readonly ZipArchiveProblems None = new(ImmutableList<FileProblem>.Empty);
    private readonly ImmutableList<FileProblem> _problems;

    private ZipArchiveProblems(ImmutableList<FileProblem> problems)
    {
        _problems = problems;
    }

    public int Count => _problems.Count;

    public bool Equals(ZipArchiveProblems other)
    {
        return other != null && _problems.SequenceEqual(other._problems);
    }

    public IEnumerator<FileProblem> GetEnumerator()
    {
        return _problems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ZipArchiveProblems Add(FileProblem problem)
    {
        if (problem == null) throw new ArgumentNullException(nameof(problem));

        return new ZipArchiveProblems(_problems.Add(problem));
    }

    public ZipArchiveProblems AddRange(IEnumerable<FileProblem> problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));

        return new ZipArchiveProblems(_problems.AddRange(problems));
    }

    public ZipArchiveProblems Remove(Func<FileProblem, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return new ZipArchiveProblems(_problems.Where(problem => !predicate(problem)).ToImmutableList());
    }

    public override bool Equals(object obj)
    {
        return obj is ZipArchiveProblems other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _problems.Aggregate(0, (current, error) => current ^ error.GetHashCode());
    }

    public static ZipArchiveProblems Many(params FileProblem[] problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));

        return None.AddRange(problems);
    }

    public static ZipArchiveProblems Many(IEnumerable<FileProblem> problems)
    {
        if (problems == null) throw new ArgumentNullException(nameof(problems));

        return None.AddRange(problems);
    }

    public static ZipArchiveProblems operator +(ZipArchiveProblems left, FileProblem right)
    {
        return left.Add(right);
    }

    public static ZipArchiveProblems operator +(ZipArchiveProblems left, IEnumerable<FileProblem> right)
    {
        return left.AddRange(right);
    }

    public static ZipArchiveProblems operator +(ZipArchiveProblems left, ZipArchiveProblems right)
    {
        return left.AddRange(right);
    }

    public ZipArchiveProblems RequiredFileMissing(string file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));

        return new ZipArchiveProblems(_problems.Add(
            new FileError(file.ToUpperInvariant(), nameof(RequiredFileMissing)))
        );
    }

    public bool HasError()
    {
        return _problems.OfType<FileError>().Any();
    }

    public void ThrowIfError()
    {
        if (HasError())
        {
            throw new ZipArchiveValidationException(this);
        }
    }

    public IEnumerable<FileProblem> GetMissingOrInvalidFileProblems()
    {
        var allowedIntegrationProblemReasons = new[]
        {
            nameof(ZipArchiveProblems.RequiredFileMissing),
            nameof(DbaseFileProblems.HasDbaseHeaderFormatError),
            nameof(DbaseFileProblems.HasDbaseSchemaMismatch)
        };
        return _problems.Where(x => allowedIntegrationProblemReasons.Contains(x.Reason));
    }

    public static ZipArchiveProblems Single(FileProblem problem)
    {
        if (problem == null) throw new ArgumentNullException(nameof(problem));

        return None.Add(problem);
    }
}
