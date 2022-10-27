namespace RoadRegistry.Tests.Framework.Assertions;

using System.Runtime.Serialization;

/// <summary>
///     Represents an error about an ill-behaved explicit conversion method.
/// </summary>
[Serializable]
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExplicitConversionMethodException" /> class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="System.Runtime.Serialization.SerializationInfo" /> that holds the
    ///     serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="System.Runtime.Serialization.StreamingContext" /> that contains
    ///     contextual information about the source or destination.
    /// </param>
    protected ExplicitConversionMethodException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        From = (Type)info.GetValue("From", typeof(Type));
        To = (Type)info.GetValue("To", typeof(Type));
    }

    public Type From { get; }
    public Type To { get; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
        info.AddValue("From", From);
        info.AddValue("To", To);
    }
}