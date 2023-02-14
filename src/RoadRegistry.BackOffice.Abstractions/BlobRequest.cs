namespace RoadRegistry.BackOffice.Abstractions;

public class BlobRequest
{
    public string BlobName { get; set; }
}

public class BlobRequestAttribute : Attribute
{
}
