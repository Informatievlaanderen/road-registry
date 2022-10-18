namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

public class DownloadExtractByNisCodeNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByNisCodeNotFoundException(string message)
        : base(message ?? "Could not find download extract with the specified NIS code")
    {
    }

    private DownloadExtractByNisCodeNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public string NisCode { get; set; }
}
