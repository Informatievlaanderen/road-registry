namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class ExtractArchiveNotCreatedException : DownloadExtractException
{
    public ExtractArchiveNotCreatedException()
        : base("Extract archive was never created")
    {
    }

    protected ExtractArchiveNotCreatedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
