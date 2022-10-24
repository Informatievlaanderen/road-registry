namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadExtractByFileNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByFileNotFoundException(string message)
        : base(message ?? "Could not find download extract with the specified contour")
    {
    }

    private DownloadExtractByFileNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
