namespace RoadRegistry.Tests.Framework.Assertions;

/// <summary>
///     Represents an error about an ill-behaved IComparable.Compare method.
/// </summary>
public class ComparableCompareToException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ComparableCompareToException" /> class.
    /// </summary>
    public ComparableCompareToException(Type type)
        : base($"The IComparable<{type?.Name}>.Equals conversion method on {type?.Name} is ill-behaved.")
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComparableCompareToException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    public ComparableCompareToException(Type type, string message)
        : base(message)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComparableCompareToException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    public ComparableCompareToException(Type type, string message, Exception innerException)
        : base(message, innerException)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ;
    }

    public Type Type { get; }
}
