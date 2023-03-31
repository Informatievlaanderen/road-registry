namespace RoadRegistry.BackOffice.Core.ProblemCodes;

using System;
using System.Collections.Generic;

public sealed partial record ProblemCode
{
    public static readonly SortedList<string, ProblemCode> Values = new();
    private readonly string _value;

    public ProblemCode(string value)
    {
        _value = value;
        Values.Add(value, this);
    }

    public static ProblemCode FromReason(string problemReason)
    {
        if (Values.TryGetValue(problemReason, out var problemCode))
        {
            return problemCode;
        }

        throw new ArgumentException($"Problem code '{problemReason}' was not found");
    }

    public static implicit operator string(ProblemCode problemCode)
    {
        return problemCode._value;
    }

    public override string ToString()
    {
        return _value;
    }
}
