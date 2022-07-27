namespace RoadRegistry.BackOffice.Exceptions;

using System;

public class UploadExtractBlobClientNotFoundException : ApplicationException
{
    public string ArgumentName { get; init; }
    public UploadExtractBlobClientNotFoundException(string argumentName) : base("Could not find blob client for extract upload") => ArgumentName = argumentName;
}
