namespace RoadRegistry.Tests.Framework.Assertions;

/// <summary>
///     Represents an error about an ill-behaved greater than or equal operator.
/// </summary>
public class GreaterThanOrEqualOperatorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GreaterThanOrEqualOperatorException" /> class.
    /// </summary>
    public GreaterThanOrEqualOperatorException(Type type)
        : base($"The greater than or equal operator on {type?.Name} is ill-behaved.")
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GreaterThanOrEqualOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    public GreaterThanOrEqualOperatorException(Type type, string message)
        : base(message)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GreaterThanOrEqualOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    public GreaterThanOrEqualOperatorException(Type type, string message, Exception innerException)
        : base(message, innerException)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; }
}
