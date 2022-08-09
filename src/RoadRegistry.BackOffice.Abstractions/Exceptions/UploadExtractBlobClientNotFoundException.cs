namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class UploadExtractBlobClientNotFoundException : ApplicationException
{
    public UploadExtractBlobClientNotFoundException(string argumentName) : base("Could not find blob client for extract upload")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
