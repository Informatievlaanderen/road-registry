namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class DownloadExtractByNisCodeNotFoundException : DownloadExtractNotFoundException
{
    public DownloadExtractByNisCodeNotFoundException(string message)
        : base(message ?? "Could not find download extract with the specified NIS code")
    { }

    private DownloadExtractByNisCodeNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public string NisCode { get; set; }
}
