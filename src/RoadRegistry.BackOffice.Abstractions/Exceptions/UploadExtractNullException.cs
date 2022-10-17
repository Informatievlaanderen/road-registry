namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class UploadExtractException : ApplicationException
{
    public UploadExtractException()
    { }

    public UploadExtractException(string? message) : base(message)
    { }

    protected UploadExtractException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

}

[Serializable]
public sealed class UploadExtractNullException : UploadExtractException
{
    public UploadExtractNullException()
    { }

    public UploadExtractNullException(string? message) : base(message)
    { }
    
    private UploadExtractNullException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

}
