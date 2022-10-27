namespace RoadRegistry.BackOffice.Api.Extracts;

using Microsoft.AspNetCore.Http;

public class DownloadExtractByFileRequestBody
{
    public int Buffer { get; set; }
    public string Description { get; set; }
    public IFormFileCollection Files { get; set; }
}