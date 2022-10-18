namespace RoadRegistry.BackOffice.Api.Extracts;

public class DownloadExtractByNisCodeRequestBody
{
    public int Buffer { get; set; }
    public string Description { get; set; }
    public string NisCode { get; set; }
}
