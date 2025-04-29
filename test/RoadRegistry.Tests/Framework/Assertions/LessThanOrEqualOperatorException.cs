namespace RoadRegistry.Tests.Framework.Assertions;

/// <summary>
///     Represents an error about an ill-behaved less than or equal operator.
/// </summary>
public class LessThanOrEqualOperatorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LessThanOrEqualOperatorException" /> class.
    /// </summary>
    public LessThanOrEqualOperatorException(Type type)
        : base($"The less than or equal operator on {type?.Name} is ill-behaved.")
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LessThanOrEqualOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    public LessThanOrEqualOperatorException(Type type, string message)
        : base(message)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LessThanOrEqualOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    public LessThanOrEqualOperatorException(Type type, string message, Exception innerException)
        : base(message, innerException)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; }
}
