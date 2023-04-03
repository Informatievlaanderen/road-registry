namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;

public readonly record struct ProblemSeverity
{
    private readonly byte _value;
    private static readonly SortedList<byte, ProblemSeverity> Values = new();

    public static readonly ProblemSeverity Error = new(1);
    public static readonly ProblemSeverity Warning = new(2);

    private ProblemSeverity(byte value)
    {
        _value = value;
        Values.Add(value, this);
    }

    public static implicit operator ProblemSeverity(byte value) => Values[value];
    public static implicit operator byte(ProblemSeverity value) => value._value;

    public static implicit operator ProblemSeverity(Messages.ProblemSeverity value) => value switch
    {
        Messages.ProblemSeverity.Error => ProblemSeverity.Error,
        Messages.ProblemSeverity.Warning => ProblemSeverity.Warning,
        _ => throw new NotSupportedException()
    };
}
