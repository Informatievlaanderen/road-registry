namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadExtractByContourNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByContourNotFoundException(string message)
        : base(message ?? "Could not find download extract with the specified contour")
    {
    }

    private DownloadExtractByContourNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string Contour { get; init; }
}
