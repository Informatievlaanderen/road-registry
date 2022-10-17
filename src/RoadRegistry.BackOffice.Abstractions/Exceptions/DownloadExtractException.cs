namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public abstract class DownloadExtractException : ApplicationException
{
    protected DownloadExtractException(string message)
        : base(message)
    { }

    protected DownloadExtractException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public string Description { get; set; }
}
