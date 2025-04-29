namespace RoadRegistry.Tests.Framework.Assertions;

/// <summary>
///     Represents an error about an ill-behaved inequality operator.
/// </summary>
public class InequalityOperatorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InequalityOperatorException" /> class.
    /// </summary>
    public InequalityOperatorException(Type type)
        : base($"The inequality operator on {type?.Name} is ill-behaved.")
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InequalityOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    public InequalityOperatorException(Type type, string message)
        : base(message)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InequalityOperatorException" /> class.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    public InequalityOperatorException(Type type, string message, Exception innerException)
        : base(message, innerException)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Type Type { get; }
}
