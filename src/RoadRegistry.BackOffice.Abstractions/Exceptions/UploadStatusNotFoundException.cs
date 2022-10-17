namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class UploadStatusNotFoundException : ApplicationException
{
    public UploadStatusNotFoundException(string? message)
        : base(message)
    { }
    
    private UploadStatusNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

}
