namespace RoadRegistry.BackOffice.Api.Extracts
{
    public class DownloadExtractByNisCodeRequestBody
    {
        public string NisCode { get; set; }
        public int Buffer { get; set; }
        public string Description { get; set; }
    }
}
