namespace RoadRegistry.BackOffice.Core.ProblemCodes;

using System.Collections.Concurrent;
using System.Linq;

public sealed partial record ProblemCode
{
    private static readonly ConcurrentDictionary<string, ProblemCode> Values = new();
    private readonly string _value;

    private ProblemCode(string value)
    {
        _value = value;
        Values.TryAdd(value, this);
    }

    public static ProblemCode[] GetValues() => Values.Values.ToArray();

    public static ProblemCode? FromReason(string? problemReason)
    {
        if (problemReason is not null && Values.TryGetValue(problemReason, out var problemCode))
        {
            return problemCode;
        }

        return null;
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
