namespace RoadRegistry.Tests.Framework.Assertions;

/// <summary>
///     Represents an error about an ill-behaved explicit conversion method.
/// </summary>
public class ExplicitConversionMethodException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExplicitConversionMethodException" /> class.
    /// </summary>
    public ExplicitConversionMethodException(Type from, Type to)
        : base($"The explicit conversion method on {from?.Name} to {to?.Name}, called To{to?.Name}(), is ill-behaved.")
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExplicitConversionMethodException" /> class.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="from"></param>
    public ExplicitConversionMethodException(Type from, Type to, string message)
        : base(message)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExplicitConversionMethodException" /> class.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="message">
    ///     The error message that explains the reason for the exception.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    /// <param name="from"></param>
    public ExplicitConversionMethodException(Type from, Type to, string message, Exception innerException)
        : base(message, innerException)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
    }

    public Type From { get; }
    public Type To { get; }
}
