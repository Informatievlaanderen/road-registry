namespace RoadRegistry.BackOffice.Api.Extracts;

public class DownloadExtractByContourRequestBody
{
    public int Buffer { get; set; }
    public string Contour { get; set; }
    public string Description { get; set; }
}